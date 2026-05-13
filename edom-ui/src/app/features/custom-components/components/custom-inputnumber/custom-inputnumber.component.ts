import { Component, EventEmitter, HostBinding, Injector, Input, OnChanges, OnInit, Output, SimpleChanges, forwardRef } from '@angular/core';
import { AbstractControl, AsyncValidatorFn, ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR, NgControl, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { InputNumberModule } from 'primeng/inputnumber';
import { ValidationMessageDictionaryService } from '../../../../core/services/validation-message-dictionary.service';

type CustomValidatorEntry = {
  sync?: ValidatorFn[];
  async?: AsyncValidatorFn[];
};

@Component({
  selector: 'app-custom-inputnumber-input',
  standalone: true,
  imports: [InputNumberModule, FormsModule],
  templateUrl: './custom-inputnumber.component.html',
  styleUrl: './custom-inputnumber.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CustomInputNumberComponent),
      multi: true
    }
  ]
})
export class CustomInputNumberComponent implements ControlValueAccessor, OnInit, OnChanges {
  @Input() value: number | null = null;

  // PrimeNG InputNumber-related properties
  @Input() useGrouping = true;
  @Input() mode: 'decimal' | 'currency' = 'decimal';
  @Input() currency: string | undefined;
  @Input() locale: string | undefined;
  @Input() minFractionDigits: number | undefined;
  @Input() maxFractionDigits: number | undefined;
  @Input() prefix: string | undefined;
  @Input() suffix: string | undefined;
  @Input() min: number | undefined;
  @Input() max: number | undefined;
  @Input() step = 1;
  @Input() showButtons = false;
  @Input() variant: 'filled' | 'outlined' | undefined;
  @Input() size: 'small' | 'large' | undefined;
  @Input() fluid = false;
  @Input() invalid = false;
  @Input() readonly = false;
  @Input() name = '';
  @Input() styleClass = '';

  // Common field behavior
  @Input() placeholder = '';
  @Input() disabled = false;

  // Built-in declarative validation
  @Input() required = false;
  @HostBinding('class.required') get hostClassRequired(): boolean {
    return this.required;
  }

  // Custom validation
  @Input() customValidators: ValidatorFn[] = [];
  @Input() customAsyncValidators: AsyncValidatorFn[] = [];
  @Input() customValidationType: string | null = null;
  @Input() customValidationMap: Record<string, CustomValidatorEntry> = {};

  // Error message resolution
  @Input() showErrorMessage = true;
  @Input() errorMessageKeys: Record<string, string> = {};
  @Input() errorMessagePlaceholders: Record<string, unknown> = {};
  @Input() errorMessagePlaceholdersByKey: Record<string, Record<string, unknown>> = {};

  @Output() readonly valueChange = new EventEmitter<number | null>();

  isFormDisabled = false;

  private baseSyncValidator: ValidatorFn | null = null;
  private baseAsyncValidator: AsyncValidatorFn | null = null;
  private baseValidatorsCaptured = false;

  private onChange: (value: number | null) => void = () => {};
  private onTouched: () => void = () => {};
  private disabledByComponent = false;

  ngControl: NgControl | null = null;

  constructor(
    private readonly injector: Injector,
    private readonly dictionary: ValidationMessageDictionaryService
  ) {}

  ngOnInit(): void {
    this.ngControl = this.injector.get(NgControl, null, { self: true, optional: true });
    this.syncDisabledState();
    this.applyValidators();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['disabled']) {
      this.syncDisabledState();
    }

    this.applyValidators();
  }

  // --- ControlValueAccessor ---

  writeValue(value: number | null): void {
    this.value = value ?? null;
  }

  registerOnChange(fn: (value: number | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isFormDisabled = isDisabled;

    if (!isDisabled) {
      this.disabledByComponent = false;
    }
  }

  // --- template handlers ---

  handleValueChange(value: number | null): void {
    this.value = value;
    this.onChange(value);
    this.valueChange.emit(value);
  }

  handleBlur(): void {
    this.onTouched();
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

    if (this.min != null) {
      validators.push(Validators.min(this.min));
    }

    if (this.max != null) {
      validators.push(Validators.max(this.max));
    }

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

  private normalizeErrorKey(key: string): string {
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

    if (errorKey === 'min') {
      return { min: details['min'], actual: details['actual'] };
    }

    if (errorKey === 'max') {
      return { max: details['max'], actual: details['actual'] };
    }

    return {};
  }
}
