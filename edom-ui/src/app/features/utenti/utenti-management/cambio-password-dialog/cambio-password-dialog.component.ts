import { Component, Input, output, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';

import { FormFieldComponent } from '../../../../core/components/form-field/form-field.component';
import { FormValidationHelperService } from '../../../../core/services/form-validation-helper.service';
import { ApiValidationMapperService } from '../../../../core/services/api-validation-mapper.service';
import { UtentiService } from '../../../../core/services/utenti.service';

@Component({
  selector: 'app-cambio-password-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    FormFieldComponent
  ],
  template: `
    <form [formGroup]="formPassword" (ngSubmit)="cambiaPassword()">
      <app-form-field label="Password attuale" forId="passwordAttuale" [error]="formValidation.getFieldError(formPassword, 'passwordAttuale')">
        <input id="passwordAttuale" pInputText type="password" formControlName="passwordAttuale" />
      </app-form-field>

      <app-form-field label="Nuova password" forId="passwordNuova" [error]="formValidation.getFieldError(formPassword, 'passwordNuova')">
        <input id="passwordNuova" pInputText type="password" formControlName="passwordNuova" />
      </app-form-field>

      <app-form-field label="Conferma password" forId="passwordConferma" [error]="formValidation.getFieldError(formPassword, 'passwordConferma')">
        <input id="passwordConferma" pInputText type="password" formControlName="passwordConferma" />
      </app-form-field>

      <div class="dialog-actions">
        <button pButton type="button" label="Annulla" severity="secondary" (click)="annulla()"></button>
        <button pButton type="submit" label="Cambio password" icon="pi pi-save" [loading]="saving"></button>
      </div>
    </form>
  `,
  styles: [`
    form {
      display: grid;
      gap: 0.55rem;
    }

    .dialog-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.5rem;
      margin-top: 0.25rem;
    }
  `]
})
export class CambioPasswordDialogComponent {
  @Input() utenteId = 0;
  @Input() saving = false;
  readonly annullaClicked = output<void>();
  readonly passwordCambiata = output<void>();

  private readonly fb = inject(FormBuilder);
  private readonly utentiService = inject(UtentiService);
  private readonly apiValidationMapper = inject(ApiValidationMapperService);
  private readonly msg = inject(MessageService);
  readonly formValidation = inject(FormValidationHelperService);

  readonly formPassword = this.fb.nonNullable.group({
    passwordAttuale: ['', [Validators.required, Validators.minLength(6)]],
    passwordNuova: ['', [Validators.required, Validators.minLength(6)]],
    passwordConferma: ['', [Validators.required]]
  }, { validators: this.passwordMatchValidator });

  private passwordMatchValidator(form: any) {
    const pwd = form.get('passwordNuova')?.value;
    const confirm = form.get('passwordConferma')?.value;
    return pwd && confirm && pwd !== confirm ? { passwordMismatch: true } : null;
  }

  cambiaPassword(): void {
    if (this.formPassword.invalid) {
      this.formPassword.markAllAsTouched();
      const err = this.formPassword.errors?.['passwordMismatch'];
      if (err) {
        this.msg.add({ severity: 'warn', summary: 'Errore', detail: 'Le password non coincidono.' });
      } else {
        this.msg.add({ severity: 'warn', summary: 'Validazione', detail: 'Compila i campi obbligatori.' });
      }
      return;
    }

    const value = this.formPassword.getRawValue();
    this.utentiService.cambiaPassword(this.utenteId, {
      passwordAttuale: value.passwordAttuale.trim(),
      passwordNuova: value.passwordNuova.trim()
    }).subscribe({
      next: () => {
        this.msg.add({
          severity: 'success',
          summary: 'Successo',
          detail: 'Password cambiata con successo.'
        });
        this.formPassword.reset();
        this.passwordCambiata.emit();
      },
      error: (error) => {
        const mapped = this.apiValidationMapper.applyToForm(this.formPassword, error);
        if (mapped) {
          return;
        }

        this.msg.add({
          severity: 'error',
          summary: 'Errore',
          detail: 'Password attuale non corretta o cambio password non riuscito.'
        });
      }
    });
  }

  annulla(): void {
    this.formPassword.reset();
    this.annullaClicked.emit();
  }
}
