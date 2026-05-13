import { NgTemplateOutlet } from '@angular/common';
import { Component, ContentChild, EventEmitter, HostBinding, Injector, Input, OnChanges, OnInit, Output, SimpleChanges, forwardRef, signal } from '@angular/core';
import { AbstractControl, AsyncValidatorFn, ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR, NgControl, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { InputTextModule } from 'primeng/inputtext';

import { ValidationMessageDictionaryService } from '../../services/validation-message-dictionary.service';
import { LookupDialogContentDirective } from './lookup-dialog-content.directive';
import { LookupSelectionEvent, LookupFieldMapping } from './lookup.types';
import { CustomTextboxInputComponent } from "../../../features/custom-components/components/custom-textbox/custom-textbox.component";

type CustomValidatorEntry = {
  sync?: ValidatorFn[];
  async?: AsyncValidatorFn[];
};

@Component({
  selector: 'app-lookup-input',
  standalone: true,
  imports: [
    NgTemplateOutlet,
    FormsModule,
    ButtonModule,
    DialogModule,
    InputGroupModule,
    InputGroupAddonModule,
    InputTextModule,
    CustomTextboxInputComponent
],
  templateUrl: './lookup-input.component.html',
  styleUrl: './lookup-input.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => LookupInputComponent),
      multi: true
    }
  ]
})
export class LookupInputComponent<T = Record<string, any>> implements ControlValueAccessor {
  @Input() placeholder = '';
  @Input() label = '';
  @Input() dialogTitle = 'Ricerca';
  @Input() disabled = false;
  @Input() readonly = false;
  @Input() clearButtonAriaLabel = 'Annulla selezione';
  @Input() searchButtonAriaLabel = 'Apri ricerca';
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

  // New: Field mapping and generic result handling
  @Input() fieldMappings?: LookupFieldMapping;
  @Input() autoPopulateForm = false;
  @Input() resultDisplayField?: string;

  @HostBinding('class.required') get hostClassRequired(): boolean {
    return this.required;
  }

  @Output() readonly search = new EventEmitter<void>();
  @Output() readonly opened = new EventEmitter<void>();
  @Output() readonly cleared = new EventEmitter<void>();
  @Output() readonly searchTextChanged = new EventEmitter<string>();
  @Output() readonly selectedResult = new EventEmitter<LookupSelectionEvent<T>>();

  @ContentChild(LookupDialogContentDirective)
  dialogContent?: LookupDialogContentDirective;

  readonly dialogVisible = signal(false);
  isFormDisabled = false;

  value: T | string = '';
  displayValue: string = '';

  private baseSyncValidator: ValidatorFn | null = null;
  private baseAsyncValidator: AsyncValidatorFn | null = null;
  private baseValidatorsCaptured = false;
  private disabledByComponent = false;

  ngControl: NgControl | null = null;

  private onChange: (value: T | string) => void = () => {};
  private onTouched: () => void = () => {};

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

  writeValue(value: T | string | null): void {
    if (value === null || value === undefined) {
      this.value = '';
      this.displayValue = '';
    } else {
      this.value = value;
      this.displayValue = this.extractDisplayValue(value);
    }
  }

  registerOnChange(fn: (value: T | string) => void): void {
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

  onInput(value: string): void {
    this.displayValue = value;
    this.onChange(value);
    this.searchTextChanged.emit(value);
  }

  onBlur(): void {
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

  openDialog(): void {
    if (this.disabled) {
      return;
    }

    this.dialogVisible.set(true);
    this.search.emit();
    this.opened.emit();
  }

  closeDialog(): void {
    this.dialogVisible.set(false);
  }

  clearValue(): void {
    if (this.disabled || this.readonly) {
      return;
    }

    this.value = '';
    this.displayValue = '';
    this.onChange('');
    this.cleared.emit();
  }

  /**
   * Called by dialog content when a result is selected.
   * Emits the LookupSelectionEvent and optionally auto-populates form if configured.
   */
  emitResult(result: T): void {
    const mapped = this.fieldMappings ? this.mapResultToForm(result, this.fieldMappings) : undefined;
    const displayValue = this.extractDisplayValue(result);

    this.value = result;
    this.displayValue = displayValue;
    this.onChange(result);

    const event: LookupSelectionEvent<T> = {
      raw: result,
      mapped,
      closed: true
    };

    this.selectedResult.emit(event);

    // Auto-populate form if configured
    if (this.autoPopulateForm && mapped) {
      const control = this.ngControl?.control;
      if (control?.parent) {
        control.parent.patchValue(mapped as any, { emitEvent: false });
      }
    }
  }

  /**
   * Maps result object fields to form control names using fieldMappings configuration.
   */
  private mapResultToForm(result: T, mappings: LookupFieldMapping): Record<string, unknown> {
    const mapped: Record<string, unknown> = {};

    for (const [sourceField, targetField] of Object.entries(mappings)) {
      const value = this.getNestedValue(result as Record<string, any>, sourceField);
      if (value !== undefined) {
        mapped[targetField] = value;
      }
    }

    return mapped;
  }

  /**
   * Extracts display value from result object.
   * Uses resultDisplayField config if provided, or converts to string for simple types.
   */
  private extractDisplayValue(value: T | string): string {
    if (typeof value === 'string') {
      return value;
    }

    if (!value) {
      return '';
    }

    if (this.resultDisplayField && typeof value === 'object') {
      const displayVal = this.getNestedValue(value as Record<string, any>, this.resultDisplayField);
      return displayVal ? String(displayVal) : '';
    }

    return '';
  }

  /**
   * Gets a value from an object using dot notation for nested paths.
   * Example: getNestedValue(obj, 'paziente.id') returns obj.paziente.id
   */
  private getNestedValue(obj: Record<string, any>, path: string): any {
    const parts = path.split('.');
    let current: any = obj;

    for (const part of parts) {
      if (current != null && typeof current === 'object' && part in current) {
        current = current[part];
      } else {
        return undefined;
      }
    }

    return current;
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
    return key.toLowerCase() === 'required' ? 'required' : key;
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
