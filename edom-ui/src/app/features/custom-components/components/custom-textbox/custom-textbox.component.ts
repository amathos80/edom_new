import { Component, EventEmitter, Injector, Input, OnChanges, OnInit, Output, SimpleChanges, forwardRef } from '@angular/core';
import { AbstractControl, AsyncValidatorFn, ControlValueAccessor, NG_VALUE_ACCESSOR, NgControl, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { ValidationMessageDictionaryService } from '../../../../core/services/validation-message-dictionary.service';

type CustomValidatorEntry = {
  sync?: ValidatorFn[];
  async?: AsyncValidatorFn[];
};

@Component({
  selector: 'app-custom-textbox-input',
  standalone: true,
  imports: [InputTextModule],
  templateUrl: './custom-textbox.component.html',
  styleUrl: './custom-textbox.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CustomTextboxInputComponent),
      multi: true
    }
  ]
})
export class CustomTextboxInputComponent implements ControlValueAccessor, OnInit {
  @Input() value = '';

  // PrimeNG InputText-related properties
  @Input() type = 'text';
  @Input() variant: 'filled' | 'outlined' | undefined;
  @Input() size: 'small' | 'large' | undefined;
  @Input() fluid = false;
  @Input() invalid = false;
  @Input() readonly = false;
  @Input() name = '';
  @Input() styleClass = '';

  // Common field behavior
  @Input() placeholder = '';
  @Input() uppercase = false;
  @Input() disabled = false;
  @Input() maxLength: number | null = null;

  // Built-in declarative validation
  @Input() required = false;
  @Input() minLength: number | null = null;
  @Input() pattern: string | null = null;
  @Input() email = false;
  @Input() number = false;

  // Custom validation (Approach C)
  @Input() customValidators: ValidatorFn[] = [];
  @Input() customAsyncValidators: AsyncValidatorFn[] = [];
  @Input() customValidationType: string | null = null;
  @Input() customValidationMap: Record<string, CustomValidatorEntry> = {};

  // Error message resolution from SI_SISMESS dictionary
  @Input() showErrorMessage = true;
  @Input() errorMessageKeys: Record<string, string> = {};
  @Input() errorMessagePlaceholders: Record<string, unknown> = {};
  @Input() errorMessagePlaceholdersByKey: Record<string, Record<string, unknown>> = {};

  /** Emette il valore elaborato (utile anche fuori da reactive forms). */
  @Output() readonly valueChange = new EventEmitter<string>();

  isFormDisabled = false;

  private baseSyncValidator: ValidatorFn | null = null;
  private baseAsyncValidator: AsyncValidatorFn | null = null;
  private baseValidatorsCaptured = false;

  private onChange: (value: string) => void = () => {};
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
    this.value = this.normalizeValue(this.value);

    if (changes['disabled']) {
      this.syncDisabledState();
    }

    this.applyValidators();
  }

  // --- ControlValueAccessor ---

  writeValue(value: string | null): void {
    this.value = this.normalizeValue(value ?? '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isFormDisabled = isDisabled;

    if (!isDisabled) {
      // Reset tracking when the parent form re-enables the control.
      this.disabledByComponent = false;
    }
  }

  // --- template handler ---

  handleInput(event: Event): void {
    const element = event.target as HTMLInputElement | null;
    const nextValue = this.normalizeValue(element?.value ?? '');

    this.value = nextValue;
    this.onChange(nextValue);
    this.valueChange.emit(nextValue);
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

  private normalizeValue(value: string): string {
    return this.uppercase ? value.toUpperCase() : value;
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

    if (this.minLength != null) {
      validators.push(Validators.minLength(this.minLength));
    }

    if (this.maxLength != null) {
      validators.push(Validators.maxLength(this.maxLength));
    }

    if (this.pattern) {
      validators.push(Validators.pattern(this.pattern));
    }

    if (this.email) {
      validators.push(Validators.email);
    }

    if (this.number) {
      validators.push(this.numberValidator);
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

  private readonly numberValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as string | null | undefined;
    if (value == null || value === '') {
      return null;
    }

    return /^\d+$/.test(value) ? null : { number: true };
  };

  private normalizeErrorKey(key: string): string {
    const lower = key.toLowerCase();
    if (lower === 'maxlength') {
      return 'maxLength';
    }

    if (lower === 'minlength') {
      return 'minLength';
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

    if (errorKey === 'maxLength') {
      return {
        maxLength: details['requiredLength'],
        requiredLength: details['requiredLength'],
        actualLength: details['actualLength']
      };
    }

    if (errorKey === 'minLength') {
      return {
        minLength: details['requiredLength'],
        requiredLength: details['requiredLength'],
        actualLength: details['actualLength']
      };
    }

    if (errorKey === 'pattern') {
      return {
        pattern: details['requiredPattern'],
        requiredPattern: details['requiredPattern'],
        actualValue: details['actualValue']
      };
    }

    return {};
  }
}
