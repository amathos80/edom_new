import { NgTemplateOutlet } from '@angular/common';
import { Component, ContentChild, EventEmitter, Input, Output, forwardRef, signal } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputGroupAddonModule } from 'primeng/inputgroupaddon';
import { InputTextModule } from 'primeng/inputtext';

import { LookupDialogContentDirective } from './lookup-dialog-content.directive';
import { CustomTextboxInputComponent } from "../../../features/custom-components/components/custom-textbox/custom-textbox.component";

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
export class LookupInputComponent implements ControlValueAccessor {
  @Input() placeholder = '';
  @Input() label = '';
  @Input() dialogTitle = 'Ricerca';
  @Input() disabled = false;
  @Input() readonly = false;
  @Input() clearButtonAriaLabel = 'Annulla selezione';
  @Input() searchButtonAriaLabel = 'Apri ricerca';

  @Output() readonly search = new EventEmitter<void>();
  @Output() readonly opened = new EventEmitter<void>();
  @Output() readonly cleared = new EventEmitter<void>();
  @Output() readonly searchTextChanged = new EventEmitter<string>();

  @ContentChild(LookupDialogContentDirective)
  dialogContent?: LookupDialogContentDirective;

  readonly dialogVisible = signal(false);

  value = '';

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: string | null): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onInput(value: string): void {
    this.value = value;
    this.onChange(value);
    this.searchTextChanged.emit(value);
  }

  onBlur(): void {
    this.onTouched();
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
    this.onChange('');
    this.cleared.emit();
  }
}
