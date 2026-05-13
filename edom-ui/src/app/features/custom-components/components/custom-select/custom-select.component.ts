import { CommonModule } from '@angular/common';
import { Component, EventEmitter, HostBinding, Injector, Input, OnChanges, OnInit, Output, SimpleChanges, forwardRef } from '@angular/core';
import { AbstractControl, AsyncValidatorFn, ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR, NgControl, ValidatorFn, Validators } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { ValidationMessageDictionaryService } from '../../../../core/services/validation-message-dictionary.service';

type CustomValidatorEntry = {
  sync?: ValidatorFn[];
  async?: AsyncValidatorFn[];
};

@Component({
  selector: 'app-custom-select-input',
  standalone: true,
  imports: [CommonModule, FormsModule, SelectModule],
  templateUrl: './custom-select.component.html',
  styleUrl: './custom-select.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CustomSelectInputComponent),
      multi: true
    }
  ]
})
export class CustomSelectInputComponent implements ControlValueAccessor, OnInit, OnChanges {
  @Input() value: unknown = null;

  // PrimeNG Select-related properties
  @Input() options: unknown[] | null = [];
  @Input() optionLabel = '';
  @Input() optionValue = '';
  @Input() optionDisabled = '';
  @Input() optionGroupLabel = 'label';
  @Input() optionGroupChildren = 'items';
  @Input() group = false;
  @Input() filter = false;
  @Input() filterBy = '';
  @Input() filterMatchMode: 'contains' | 'startsWith' | 'endsWith' | 'equals' | 'notEquals' | 'in' | 'lt' | 'lte' | 'gt' | 'gte' = 'contains';
  @Input() placeholder = '';
  @Input() appendTo: HTMLElement | string | null = null;
  @Input() loading = false;
  @Input() readonly = false;
  @Input() editable = false;
  @Input() showClear = false;
  @Input() checkmark = false;
  @Input() highlightOnSelect = true;
  @Input() variant: 'filled' | 'outlined' | undefined;
  @Input() size: 'small' | 'large' | undefined;
  @Input() fluid = false;
  @Input() invalid = false;
  @Input() styleClass = '';
  @Input() inputStyleClass = '';
  @Input() inputStyle: Record<string, string | number> | null = null;
  @Input() scrollHeight = '14rem';
  @Input() virtualScroll = false;
  @Input() virtualScrollItemSize: number | null = null;
  @Input() autofocus = false;
  @Input() autoOptionFocus = false;
  @Input() autofocusFilter = false;
  @Input() resetFilterOnHide = false;
  @Input() emptyMessage = '';
  @Input() emptyFilterMessage = '';
  @Input() tabindex: number | null = null;
  @Input() name = '';
  @Input() inputId = '';
  @Input() showEmptyOption = false;
  @Input() emptyLabel = '\u200B';

  // Common field behavior
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

  // Error message resolution from SI_SISMESS dictionary
  @Input() showErrorMessage = true;
  @Input() errorMessageKeys: Record<string, string> = {};
  @Input() errorMessagePlaceholders: Record<string, unknown> = {};
  @Input() errorMessagePlaceholdersByKey: Record<string, Record<string, unknown>> = {};

  @Output() readonly valueChange = new EventEmitter<unknown>();
  @Output() readonly blurEvent = new EventEmitter<Event>();
  @Output() readonly focusEvent = new EventEmitter<Event>();
  @Output() readonly changeEvent = new EventEmitter<unknown>();

  isFormDisabled = false;

  private baseSyncValidator: ValidatorFn | null = null;
  private baseAsyncValidator: AsyncValidatorFn | null = null;
  private baseValidatorsCaptured = false;

  private onChange: (value: unknown) => void = () => {};
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

  writeValue(value: unknown): void {
    this.value = value ?? null;
  }

  registerOnChange(fn: (value: unknown) => void): void {
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

  handleModelChange(nextValue: unknown): void {
    this.value = nextValue ?? null;
    this.onChange(this.value);
    this.valueChange.emit(this.value);
  }

  handleBlur(event: Event): void {
    this.onTouched();
    this.blurEvent.emit(event);
  }

  handleFocus(event: Event): void {
    this.focusEvent.emit(event);
  }

  handleChange(event: unknown): void {
    this.changeEvent.emit(event);
  }

  get isDisabled(): boolean {
    return this.disabled || this.isFormDisabled;
  }

  get effectiveInvalid(): boolean {
    return this.invalid || this.hasValidationError();
  }

  get effectiveOptions(): unknown[] {
    const baseOptions = this.options ?? [];
    if (!this.showEmptyOption) {
      return baseOptions;
    }

    // Create empty option with both optionLabel and optionValue properties
    const emptyOption: Record<string, unknown> = {};
    if (this.optionLabel) {
      emptyOption[this.optionLabel] = this.emptyLabel;
    }
    if (this.optionValue) {
      emptyOption[this.optionValue] = null;
    }
    // Fallback if neither is specified
    if (!this.optionLabel && !this.optionValue) {
      emptyOption['label'] = this.emptyLabel;
      emptyOption['value'] = null;
    }

    return [emptyOption, ...baseOptions];
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

  private extractErrorPlaceholders(_errorKey: string, _errorValue: unknown): Record<string, unknown> {
    return {};
  }
}