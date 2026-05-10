import { Component, Input, OnChanges, OnInit, SimpleChanges, forwardRef, inject, signal } from '@angular/core';
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

import { NumeroPuaDto } from '../../models/pua.model';
import { PuaService } from '../../services/pua.service';

@Component({
  selector: 'app-numero-pua-select',
  standalone: true,
  imports: [FormsModule, CustomSelectInputComponent],
  templateUrl: './numero-pua-select.component.html',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => NumeroPuaSelectComponent),
      multi: true
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => NumeroPuaSelectComponent),
      multi: true
    }
  ]
})
export class NumeroPuaSelectComponent implements ControlValueAccessor, Validator, OnInit, OnChanges {
  private readonly puaService = inject(PuaService);
  private readonly dictionary = inject(ValidationMessageDictionaryService);
  private control: AbstractControl | null = null;

  @Input() required = false;
  @Input() errorMessageKeys: Record<string, string> = {};
  @Input() errorMessagePlaceholders: Record<string, unknown> = {};
  @Input() errorMessagePlaceholdersByKey: Record<string, Record<string, unknown>> = {};

  readonly options = signal<NumeroPuaDto[]>([]);
  readonly loading = signal(false);

  value: number | null = null;
  disabled = false;

  private onChange: (value: number | null) => void = () => {};
  private onTouched: () => void = () => {};
  private onValidatorChange: () => void = () => {};

  ngOnInit(): void {
    this.loading.set(true);
    this.puaService.getNumeriPua().subscribe({
      next: items => {
        this.options.set(items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['required']) {
      this.onValidatorChange();
    }
  }

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
    this.disabled = isDisabled;
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

  onValueChange(value: number | null): void {
    this.value = value;
    this.onChange(value);
    this.onTouched();
    this.onValidatorChange();
  }
}
