import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { InputTextModule } from 'primeng/inputtext';
import { CommonModule } from '@angular/common';

import { FormFieldComponent } from '../../../core/components/form-field/form-field.component';
import { FormValidationHelperService } from '../../../core/services/form-validation-helper.service';
import { ApiValidationMapperService } from '../../../core/services/api-validation-mapper.service';
import { AuthService } from '../../../core/services/auth.service';
import { UtentiService } from '../../../core/services/utenti.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-forced-password-change',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    MessageModule,
    InputTextModule,
    FormFieldComponent
  ],
  providers: [MessageService],
  templateUrl: './forced-password-change.component.html',
  styleUrl: './forced-password-change.component.scss'
})
export class ForcedPasswordChangeComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly utentiService = inject(UtentiService);
  private readonly apiValidationMapper = inject(ApiValidationMapperService);
  private readonly msg = inject(MessageService);
  readonly formValidation = inject(FormValidationHelperService);

  readonly loading = signal(false);
  readonly utenteId = signal<number>(0);

  readonly form = this.fb.nonNullable.group({
    passwordAttuale: ['', [Validators.required]],
    passwordNuova: ['', [Validators.required, Validators.minLength(6)]],
    passwordConferma: ['', [Validators.required]]
  }, { validators: this.passwordMatchValidator });

  ngOnInit(): void {
    const utente = this.auth.utenteCorrente();
    if (!utente || !localStorage.getItem('must_change_password')) {
      this.router.navigate(['/login']);
      return;
    }
    
    // Estrai l'ID dal JWT (è nel claim 'sub')
    const utenteId = parseInt(utente.uid, 10);
    if (!utenteId) {
      this.router.navigate(['/login']);
      return;
    }
    
    this.utenteId.set(utenteId);
  }

  private passwordMatchValidator(form: any) {
    const pwd = form.get('passwordNuova')?.value;
    const confirm = form.get('passwordConferma')?.value;
    return pwd && confirm && pwd !== confirm ? { passwordMismatch: true } : null;
  }

  cambiaPassword(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      if (this.form.errors?.['passwordMismatch']) {
        this.msg.add({
          severity: 'warn',
          summary: 'Errore',
          detail: 'Le password non coincidono.'
        });
      } else {
        this.msg.add({
          severity: 'warn',
          summary: 'Validazione',
          detail: 'Compila i campi obbligatori correttamente.'
        });
      }
      return;
    }

    this.loading.set(true);
    const value = this.form.getRawValue();
    
    this.utentiService.cambiaPassword(this.utenteId(), {
      passwordAttuale: value.passwordAttuale.trim(),
      passwordNuova: value.passwordNuova.trim()
    }).subscribe({
      next: () => {
        localStorage.removeItem('must_change_password');
        this.msg.add({
          severity: 'success',
          summary: 'Successo',
          detail: 'Password cambiata con successo. Accesso consentito.'
        });
        this.form.reset();
        setTimeout(() => {
          this.router.navigate(['/dashboard']);
        }, 1500);
      },
      error: (error) => {
        this.loading.set(false);

        const mapped = this.apiValidationMapper.applyToForm(this.form, error);
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
}
