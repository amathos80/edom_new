import { Component, EventEmitter, forwardRef, Input, Output } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';

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
export class CustomTextboxInputComponent implements ControlValueAccessor {
  @Input() placeholder = 'Inserisci testo';
  @Input() uppercase = false;
  @Input() maxLength: number | null = null;

  /** Emette il valore elaborato (utile anche fuori da reactive forms). */
  @Output() readonly valueChange = new EventEmitter<string>();

  value = '';
  isDisabled = false;

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  // --- ControlValueAccessor ---

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
    this.isDisabled = isDisabled;
  }

  // --- template handler ---

  handleInput(event: Event): void {
    const element = event.target as HTMLInputElement | null;
    const currentValue = element?.value ?? '';
    const nextValue = this.uppercase ? currentValue.toUpperCase() : currentValue;

    this.value = nextValue;
    this.onChange(nextValue);
    this.valueChange.emit(nextValue);
  }

  handleBlur(): void {
    this.onTouched();
  }
}
