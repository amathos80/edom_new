import { Component, HostBinding, Input, OnChanges, SimpleChanges, forwardRef, inject } from '@angular/core';
import {
  AbstractControl,
  ControlValueAccessor,
  FormsModule,
  NG_VALIDATORS,
  NG_VALUE_ACCESSOR,
  ValidationErrors,
  Validator
} from '@angular/forms';
import { CustomSelectInputComponent } from '../../../features/custom-components/components/custom-select/custom-select.component';
import { ValidationMessageDictionaryService } from '../../services/validation-message-dictionary.service';

export type Sesso = 'M' | 'F' | null;

interface SessoOption {
  label: string;
  value: Sesso;
}

@Component({
  selector: 'app-sesso-select',
  standalone: true,
  imports: [FormsModule, CustomSelectInputComponent],
  templateUrl: './sesso-select.component.html',
  styleUrls: ['./sesso-select.component.scss'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SessoSelectComponent),
      multi: true
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => SessoSelectComponent),
      multi: true
    }
  ]
})
export class SessoSelectComponent implements ControlValueAccessor, Validator, OnChanges {
  private readonly dictionary = inject(ValidationMessageDictionaryService);
  private control: AbstractControl | null = null;
  readonly options: SessoOption[] = [
    { label: 'Maschio', value: 'M' },
    { label: 'Femmina', value: 'F' }
  ];

  @Input() required = false;
  @Input() disabled = false;
  @Input() errorMessageKeys: Record<string, string> = {};
  @Input() errorMessagePlaceholders: Record<string, unknown> = {};
  @Input() errorMessagePlaceholdersByKey: Record<string, Record<string, unknown>> = {};

  @HostBinding('class.required') get hostClassRequired(): boolean {
    return this.required;
  }

  value: Sesso = null;
  isFormDisabled = false;

  private onChange: (value: Sesso) => void = () => {};
  private onTouched: () => void = () => {};
  private onValidatorChange: () => void = () => {};

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['required']) {
      this.onValidatorChange();
    }
  }

  writeValue(value: Sesso): void {
    this.value = value ?? null;
  }

  registerOnChange(fn: (value: Sesso) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isFormDisabled = isDisabled;
  }

  get isDisabled(): boolean {
    return this.disabled || this.isFormDisabled;
  }

  validate(control: AbstractControl): ValidationErrors | null {
    this.control = control;

    if (!this.required) {
      return null;
    }

    const value = control.value;
    if (value === null || value === undefined || value === '') {
      return { required: true };
    }

    return null;
  }

  registerOnValidatorChange(fn: () => void): void {
    this.onValidatorChange = fn;
  }

  get effectiveInvalid(): boolean {
    const control = this.control;
    return !!control && control.invalid && (control.touched || control.dirty);
  }

  get activeErrorMessage(): string | null {
    const control = this.control;
    if (!control || !control.errors || !(control.touched || control.dirty)) {
      return null;
    }

    const firstKey = Object.keys(control.errors)[0];
    if (!firstKey) {
      return null;
    }

    const normalizedKey = firstKey.toLowerCase() === 'required' ? 'required' : firstKey;
    const messageKey = this.errorMessageKeys[normalizedKey] ?? `validation:${normalizedKey}`;
    const placeholders = {
      ...this.errorMessagePlaceholders,
      ...(this.errorMessagePlaceholdersByKey[normalizedKey] ?? {})
    };

    return this.dictionary.getMessage(messageKey, placeholders);
  }

  onValueChange(value: Sesso): void {
    this.value = value;
    this.onChange(value);
    this.onTouched();
  }
}
