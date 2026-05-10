import { Component, forwardRef } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CustomSelectInputComponent } from '../../../features/custom-components/components/custom-select/custom-select.component';

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
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SessoSelectComponent),
      multi: true
    }
  ]
})
export class SessoSelectComponent implements ControlValueAccessor {
  readonly options: SessoOption[] = [
    { label: 'Maschio', value: 'M' },
    { label: 'Femmina', value: 'F' }
  ];

  value: Sesso = null;
  disabled = false;

  private onChange: (value: Sesso) => void = () => {};
  private onTouched: () => void = () => {};

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
    this.disabled = isDisabled;
  }

  onValueChange(value: Sesso): void {
    this.value = value;
    this.onChange(value);
    this.onTouched();
  }
}
