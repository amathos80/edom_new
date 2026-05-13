import {
  Component,
  HostBinding,
  Injector,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  forwardRef,
  inject,
  signal
} from '@angular/core';
import {
  AbstractControl,
  AsyncValidatorFn,
  ControlValueAccessor,
  FormsModule,
  NG_VALUE_ACCESSOR,
  NgControl,
  ValidatorFn,
  Validators
} from '@angular/forms';

import { DatePickerModule } from 'primeng/datepicker';

import { ValidationMessageDictionaryService } from '../../services/validation-message-dictionary.service';
import { UtilityService } from '../../services/utility.service';

type CustomValidatorEntry = {
  sync?: ValidatorFn[];
  async?: AsyncValidatorFn[];
};

@Component({
  selector: 'app-date-input',
  standalone: true,
  imports: [FormsModule, DatePickerModule],
  templateUrl: './date-input.component.html',
  styleUrl: './date-input.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DateInputComponent),
      multi: true
    }
  ]
})
export class DateInputComponent implements ControlValueAccessor, OnInit, OnChanges {
  /** Allow dates after today. Default: false. */
  @Input() allowFuture = false;

  /** Allow dates before today. Default: true. */
  @Input() allowPast = true;

  /** Minimum allowed date (ISO yyyy-MM-dd). Applied in addition to allowPast. */
  @Input() minDate: string | null = null;

  /** Maximum allowed date (ISO yyyy-MM-dd). Applied in addition to allowFuture. */
  @Input() maxDate: string | null = null;

  @Input() invalid = false;
  @Input() required = false;
  @Input() customValidators: ValidatorFn[] = [];
  @Input() customAsyncValidators: AsyncValidatorFn[] = [];
  @Input() customValidationType: string | null = null;
  @Input() customValidationMap: Record<string, CustomValidatorEntry> = {};

  @Input() showErrorMessage = true;
  @Input() errorMessageKeys: Record<string, string> = {};
  @Input() errorMessagePlaceholders: Record<string, unknown> = {};
  @Input() errorMessagePlaceholdersByKey: Record<string, Record<string, unknown>> = {};

  @HostBinding('class.required') get hostClassRequired(): boolean {
    return this.required;
  }

  /** Current value as ISO string (yyyy-MM-dd) or null. */
  value: string | null = null;

  /** Reflected as p-datepicker Date object for binding. */
  dateValue: Date | null = null;

  @Input() disabled = false;
  isFormDisabled = false;

  /** Server date loaded on init, drives validation. */
  private readonly serverDate = signal<string | null>(null);

  private baseSyncValidator: ValidatorFn | null = null;
  private baseAsyncValidator: AsyncValidatorFn | null = null;
  private baseValidatorsCaptured = false;
  private disabledByComponent = false;

  ngControl: NgControl | null = null;

  private readonly utilityService = inject(UtilityService);
  private readonly dictionary = inject(ValidationMessageDictionaryService);
  private readonly injector = inject(Injector);
  private _onChange: (value: string | null) => void = () => {};
  private _onTouched: () => void = () => {};

  ngOnInit(): void {
    this.ngControl = this.injector.get(NgControl, null, { self: true, optional: true });
    this.syncDisabledState();
    this.applyValidators();

    this.utilityService.getDataCorrente().subscribe(date => {
      this.serverDate.set(date);
      this.ngControl?.control?.updateValueAndValidity({ emitEvent: false });
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['disabled']) {
      this.syncDisabledState();
    }

    this.applyValidators();
  }

  // ── ControlValueAccessor ──────────────────────────────────────────────────

  writeValue(value: string | null): void {
    this.value = value ?? null;
    this.dateValue = value ? this.isoToDate(value) : null;
  }

  registerOnChange(fn: (value: string | null) => void): void {
    this._onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this._onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isFormDisabled = isDisabled;

    if (!isDisabled) {
      this.disabledByComponent = false;
    }
  }

  // ── Template event handler ────────────────────────────────────────────────

  onDateChange(date: Date | null): void {
    if (date && date.getFullYear() < 100) {
      // PrimeNG parsed a 2-digit year (e.g. "26" → year 26 AD) — expand it.
      date = new Date(this.expandTwoDigitYear(date.getFullYear()), date.getMonth(), date.getDate());
      this.dateValue = date;
    } else {
      this.dateValue = date;
    }
    this.value = date ? this.dateToIso(date) : null;
    this._onChange(this.value);
    this._onTouched();
  }

  /**
   * Safety-net blur handler: catches cases where PrimeNG did NOT emit
   * ngModelChange (it considered the typed text invalid), but the raw text
   * matches one of these patterns:
   *   dd/mm/yy        → 2-digit year, expanded
   *   ddmmyy          → 6 digits no separators, 2-digit year, expanded
   *   ddmmyyyy        → 8 digits no separators, 4-digit year
   */
  onInputBlur(event: Event): void {
    const input = event.target as HTMLInputElement | null;
    if (!input) return;
    const text = input.value.trim();

    let day = 0, month = 0, year = 0;
    let matched = false;

    // dd/mm/yy  or  d/m/yy  (2-digit year)
    const slashShort = text.match(/^(\d{1,2})\/(\d{1,2})\/(\d{2})$/);
    if (slashShort) {
      day   = parseInt(slashShort[1], 10);
      month = parseInt(slashShort[2], 10) - 1;
      year  = this.expandTwoDigitYear(parseInt(slashShort[3], 10));
      matched = true;
    }

    // ddmmyy  (6 digits, 2-digit year)
    if (!matched) {
      const noSepShort = text.match(/^(\d{2})(\d{2})(\d{2})$/);
      if (noSepShort) {
        day   = parseInt(noSepShort[1], 10);
        month = parseInt(noSepShort[2], 10) - 1;
        year  = this.expandTwoDigitYear(parseInt(noSepShort[3], 10));
        matched = true;
      }
    }

    // ddmmyyyy  (8 digits, 4-digit year)
    if (!matched) {
      const noSepFull = text.match(/^(\d{2})(\d{2})(\d{4})$/);
      if (noSepFull) {
        day   = parseInt(noSepFull[1], 10);
        month = parseInt(noSepFull[2], 10) - 1;
        year  = parseInt(noSepFull[3], 10);
        matched = true;
      }
    }

    if (!matched) return;

    const corrected = new Date(year, month, day);
    // Reject invalid dates (e.g. 30/02/2026 would shift to March)
    if (isNaN(corrected.getTime()) || corrected.getDate() !== day) return;

    this.dateValue = corrected;
    this.value = this.dateToIso(corrected);
    this._onChange(this.value);
    this._onTouched();
    this.ngControl?.control?.updateValueAndValidity({ emitEvent: false });
  }

  get isDisabled(): boolean {
    return this.disabled || this.isFormDisabled;
  }

  get effectiveInvalid(): boolean {
    return this.invalid || this.hasValidationError();
  }

  get activeErrorMessage(): string | null {
    if (!this.showErrorMessage) {
      return null;
    }

    const control = this.ngControl?.control;
    if (!control || !control.errors || !(control.touched || control.dirty)) {
      return null;
    }

    const firstKey = Object.keys(control.errors)[0];
    if (!firstKey) {
      return null;
    }

    const normalizedKey = this.normalizeErrorKey(firstKey);
    const messageKey = this.errorMessageKeys[normalizedKey] ?? `validation:${normalizedKey}`;
    const placeholders = this.buildPlaceholders(normalizedKey, control.errors[firstKey]);

    return this.dictionary.getMessage(messageKey, placeholders);
  }

  hasValidationError(): boolean {
    const control = this.ngControl?.control;
    return !!control && control.invalid && (control.touched || control.dirty);
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  /**
   * Given a 2-digit year, returns the closest plausible 4-digit year.
   * e.g. current year 2026: "26" → 2026 (|2026-2026|=0 vs |1926-2026|=100)
   *                          "80" → 1980 (|1980-2026|=46 vs |2080-2026|=54)
   */
  private expandTwoDigitYear(yy: number): number {
    const today = this.serverDate();
    const currentYear = today ? parseInt(today.substring(0, 4), 10) : new Date().getFullYear();
    const y2000 = 2000 + yy;
    const y1900 = 1900 + yy;
    return Math.abs(y2000 - currentYear) <= Math.abs(y1900 - currentYear) ? y2000 : y1900;
  }

  /** Converts "yyyy-MM-dd" → Date at midnight local time. */
  private isoToDate(iso: string): Date {
    const [y, m, d] = iso.split('-').map(Number);
    return new Date(y, m - 1, d);
  }

  /** Converts Date → "yyyy-MM-dd". */
  private dateToIso(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  private syncDisabledState(): void {
    const control = this.ngControl?.control;
    if (!control) {
      return;
    }

    if (this.disabled && control.enabled) {
      control.disable({ emitEvent: false });
      this.disabledByComponent = true;
      return;
    }

    if (!this.disabled && control.disabled && this.disabledByComponent && !this.isFormDisabled) {
      control.enable({ emitEvent: false });
      this.disabledByComponent = false;
    }
  }

  private applyValidators(): void {
    const control = this.ngControl?.control;
    if (!control) {
      return;
    }

    if (!this.baseValidatorsCaptured) {
      this.baseSyncValidator = control.validator;
      this.baseAsyncValidator = control.asyncValidator;
      this.baseValidatorsCaptured = true;
    }

    const syncValidators = this.collectSyncValidators();
    const asyncValidators = this.collectAsyncValidators();

    control.setValidators(this.baseSyncValidator ? [this.baseSyncValidator, ...syncValidators] : syncValidators);
    control.setAsyncValidators(this.baseAsyncValidator ? [this.baseAsyncValidator, ...asyncValidators] : asyncValidators);
    control.updateValueAndValidity({ emitEvent: false });
  }

  private collectSyncValidators(): ValidatorFn[] {
    const validators: ValidatorFn[] = [];

    if (this.required) {
      validators.push(Validators.required);
    }

    validators.push(this.dateConstraintValidator);
    validators.push(...this.customValidators);

    const customByType = this.customValidationType ? this.customValidationMap[this.customValidationType] : undefined;
    if (customByType?.sync?.length) {
      validators.push(...customByType.sync);
    }

    return validators;
  }

  private collectAsyncValidators(): AsyncValidatorFn[] {
    const validators: AsyncValidatorFn[] = [];

    validators.push(...this.customAsyncValidators);

    const customByType = this.customValidationType ? this.customValidationMap[this.customValidationType] : undefined;
    if (customByType?.async?.length) {
      validators.push(...customByType.async);
    }

    return validators;
  }

  private readonly dateConstraintValidator: ValidatorFn = (control: AbstractControl) => {
    const today = this.serverDate();
    const entered = (control.value as string | null) ?? null;

    if (!today || !entered) {
      return null;
    }

    if (!this.allowFuture && entered > today) {
      return { dateNotAllowedFuture: { entered, today } };
    }

    if (!this.allowPast && entered < today) {
      return { dateNotAllowedPast: { entered, today } };
    }

    if (this.minDate && entered < this.minDate) {
      return { dateBeforeMin: { entered, min: this.minDate } };
    }

    if (this.maxDate && entered > this.maxDate) {
      return { dateAfterMax: { entered, max: this.maxDate } };
    }

    return null;
  };

  private normalizeErrorKey(key: string): string {
    if (key.toLowerCase() === 'required') {
      return 'required';
    }

    return key;
  }

  private buildPlaceholders(errorKey: string, errorValue: unknown): Record<string, unknown> {
    const base = this.extractErrorPlaceholders(errorKey, errorValue);
    return {
      ...base,
      ...this.errorMessagePlaceholders,
      ...(this.errorMessagePlaceholdersByKey[errorKey] ?? {})
    };
  }

  private extractErrorPlaceholders(errorKey: string, errorValue: unknown): Record<string, unknown> {
    const details = (errorValue as Record<string, unknown> | null) ?? {};

    if (errorKey === 'dateNotAllowedFuture' || errorKey === 'dateNotAllowedPast') {
      return {
        entered: details['entered'],
        today: details['today']
      };
    }

    if (errorKey === 'dateBeforeMin') {
      return {
        entered: details['entered'],
        minDate: details['min'],
        min: details['min']
      };
    }

    if (errorKey === 'dateAfterMax') {
      return {
        entered: details['entered'],
        maxDate: details['max'],
        max: details['max']
      };
    }

    return {};
  }
}
