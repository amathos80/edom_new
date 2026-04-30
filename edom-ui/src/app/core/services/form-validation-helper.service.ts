import { Injectable } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';

@Injectable({ providedIn: 'root' })
export class FormValidationHelperService {
  getFieldError(form: FormGroup, controlName: string): string | null {
    const control = form.get(controlName);
    return this.getControlError(control);
  }

  getControlError(control: AbstractControl | null | undefined): string | null {
    if (!control || !(control.dirty || control.touched) || !control.errors) {
      return null;
    }

    if (control.errors['required']) {
      return 'Campo obbligatorio.';
    }

    if (control.errors['min']) {
      return 'Valore non valido.';
    }

    if (control.errors['maxlength']) {
      const maxLength = control.errors['maxlength']?.requiredLength;
      return `Lunghezza massima consentita: ${maxLength} caratteri.`;
    }

    if (control.errors['api']) {
      return String(control.errors['api']);
    }

    return 'Valore non valido.';
  }
}