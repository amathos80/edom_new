import { Component } from '@angular/core';
import { AsyncValidatorFn, FormControl, ReactiveFormsModule, ValidationErrors, ValidatorFn, FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { map, timer } from 'rxjs';
import { CustomTextboxInputComponent } from '../../components/custom-textbox/custom-textbox.component';

@Component({
  selector: 'app-custom-textbox',
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule, CardModule, MessageModule, CustomTextboxInputComponent],
  templateUrl: './custom-textbox.component.html',
  styleUrl: './custom-textbox.component.scss'
})
export class CustomTextboxComponent {
  normalValue = '';
  uppercaseValue = '';

  readonly emailControl = new FormControl<string>('', {
    nonNullable: true
  });

  readonly codiceFiscaleControl = new FormControl<string>('', {
    nonNullable: true
  });

  readonly emailUniqueValidator: AsyncValidatorFn = control =>
    timer(350).pipe(
      map(() => {
        const value = String(control.value ?? '').trim().toLowerCase();
        if (!value) {
          return null;
        }

        return this.existingEmails.has(value) ? { emailExists: true } : null;
      })
    );

  readonly codiceFiscaleRemoteValidator: AsyncValidatorFn = control =>
    timer(350).pipe(
      map(() => {
        const value = String(control.value ?? '').trim().toUpperCase();
        if (!value) {
          return null;
        }

        return this.existingCodiciFiscali.has(value) ? { cfExists: true } : null;
      })
    );

  readonly customValidationMap: Record<string, { sync?: ValidatorFn[]; async?: AsyncValidatorFn[] }> = {
    emailUnique: { async: [this.emailUniqueValidator] },
    codiceFiscaleUnique: { async: [this.codiceFiscaleRemoteValidator] }
  };

  private readonly existingEmails = new Set(['taken@example.com', 'assistito@demo.it']);
  private readonly existingCodiciFiscali = new Set(['RSSMRA85M01H501U', 'VRDLGI80A01F205X']);

  hasError(control: FormControl<string>, errorKey: string): boolean {
    return control.touched && !!control.errors?.[errorKey];
  }
}
