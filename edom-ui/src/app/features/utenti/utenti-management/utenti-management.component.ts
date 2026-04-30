import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { Table, TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ToolbarModule } from 'primeng/toolbar';

import { FormValidationHelperService } from '../../../core/services/form-validation-helper.service';
import { AggiornaUtenteRequest, CreaUtenteRequest, Utente } from '../../../core/models/utente.model';
import { UtentiService } from '../../../core/services/utenti.service';

type ModalitaDialog = 'crea' | 'modifica';

@Component({
  selector: 'app-utenti-management',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DatePipe,
    ButtonModule,
    CardModule,
    CheckboxModule,
    DialogModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    TableModule,
    TagModule,
    ToastModule,
    ToolbarModule
  ],
  providers: [MessageService],
  templateUrl: './utenti-management.component.html',
  styleUrl: './utenti-management.component.scss'
})
export class UtentiManagementComponent implements OnInit {
  private readonly utentiService = inject(UtentiService);
  private readonly fb = inject(FormBuilder);
  private readonly msg = inject(MessageService);
  readonly formValidation = inject(FormValidationHelperService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly dialogVisibile = signal(false);
  readonly modalitaDialog = signal<ModalitaDialog>('crea');

  readonly utenti = signal<Utente[]>([]);
  readonly utenteSelezionato = signal<Utente | null>(null);

  readonly formUtente = this.fb.nonNullable.group({
    codice: ['', [Validators.required, Validators.maxLength(50)]],
    cognome: ['', [Validators.required, Validators.maxLength(100)]],
    nome: ['', [Validators.required, Validators.maxLength(100)]],
    codiceFiscale: ['', [Validators.maxLength(20)]],
    email: ['', [Validators.maxLength(200)]],
    matricola: ['', [Validators.maxLength(50)]],
    flagSmartCard: [false],
    flagCambiaPwd: [false],
    disattivo: [false]
  });

  ngOnInit(): void {
    this.caricaUtenti();
  }

  caricaUtenti(): void {
    this.loading.set(true);
    this.utentiService.cerca({}).subscribe({
      next: (items) => {
        this.utenti.set(items.map((u) => ({
          ...u,
          dataScadenzaPasswordDate: this.toDateOnly(u.dataScadenzaPassword),
          dataDisattivazioneDate: this.toDateOnly(u.dataDisattivazione),
          dataRiattivazioneDate: this.toDateOnly(u.dataRiattivazione),
          ultimoLoginDate: this.toDateOnly(u.ultimoLogin),
          stato: u.dataDisattivazione ? 'Disattivato' : 'Attivo'
        })));
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Impossibile caricare gli utenti.' });
      }
    });
  }

  apriCreazione(): void {
    this.modalitaDialog.set('crea');
    this.utenteSelezionato.set(null);
    this.formUtente.reset({
      codice: '',
      cognome: '',
      nome: '',
      codiceFiscale: '',
      email: '',
      matricola: '',
      flagSmartCard: false,
      flagCambiaPwd: true,
      disattivo: false
    });
    this.dialogVisibile.set(true);
  }

  apriModifica(utente: Utente): void {
    this.modalitaDialog.set('modifica');
    this.utenteSelezionato.set(utente);
    this.formUtente.reset({
      codice: utente.codice,
      cognome: utente.cognome,
      nome: utente.nome,
      codiceFiscale: utente.codiceFiscale ?? '',
      email: utente.email ?? '',
      matricola: utente.matricola ?? '',
      flagSmartCard: utente.flagSmartCard,
      flagCambiaPwd: utente.flagCambiaPwd,
      disattivo: !!utente.dataDisattivazione
    });
    this.dialogVisibile.set(true);
  }

  chiudiDialog(): void {
    this.dialogVisibile.set(false);
    this.utenteSelezionato.set(null);
  }

  salva(): void {
    if (this.formUtente.invalid) {
      this.formUtente.markAllAsTouched();
      this.msg.add({ severity: 'warn', summary: 'Validazione', detail: 'Compila i campi obbligatori.' });
      return;
    }

    const value = this.formUtente.getRawValue();
    const payloadBase: CreaUtenteRequest = {
      codice: value.codice.trim(),
      cognome: value.cognome.trim(),
      nome: value.nome.trim(),
      codiceFiscale: value.codiceFiscale.trim() ? value.codiceFiscale.trim() : null,
      email: value.email.trim() ? value.email.trim() : null,
      matricola: value.matricola.trim() ? value.matricola.trim() : null,
      flagSmartCard: value.flagSmartCard,
      flagCambiaPwd: value.flagCambiaPwd,
      dataDisattivazione: value.disattivo ? new Date().toISOString() : null
    };

    this.saving.set(true);
    if (this.modalitaDialog() === 'crea') {
      this.utentiService.crea(payloadBase).subscribe({
        next: () => {
          this.saving.set(false);
          this.dialogVisibile.set(false);
          this.msg.add({ severity: 'success', summary: 'Creato', detail: 'Utente creato con successo.' });
          this.caricaUtenti();
        },
        error: () => {
          this.saving.set(false);
          this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Creazione utente non riuscita.' });
        }
      });
      return;
    }

    const utente = this.utenteSelezionato();
    if (!utente) {
      this.saving.set(false);
      return;
    }

    const payloadAggiorna: AggiornaUtenteRequest = {
      ...payloadBase,
      dataDisattivazione: value.disattivo ? (utente.dataDisattivazione ?? new Date().toISOString()) : null
    };

    this.utentiService.aggiorna(utente.id, payloadAggiorna).subscribe({
      next: () => {
        this.saving.set(false);
        this.dialogVisibile.set(false);
        this.msg.add({ severity: 'success', summary: 'Salvato', detail: 'Utente aggiornato con successo.' });
        this.caricaUtenti();
      },
      error: () => {
        this.saving.set(false);
        this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Aggiornamento utente non riuscito.' });
      }
    });
  }

  elimina(utente: Utente): void {
    if (!confirm(`Eliminare l'utente ${utente.codice}?`)) {
      return;
    }

    this.utentiService.elimina(utente.id).subscribe({
      next: () => {
        this.msg.add({ severity: 'success', summary: 'Eliminato', detail: 'Utente eliminato con successo.' });
        this.caricaUtenti();
      },
      error: () => {
        this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Eliminazione utente non riuscita.' });
      }
    });
  }

  resetPassword(): void {
    const utente = this.utenteSelezionato();
    if (!utente) {
      return;
    }

    this.utentiService.resetPassword(utente.id).subscribe({
      next: () => {
        this.msg.add({
          severity: 'success',
          summary: 'Password resettata',
          detail: 'Password impostata al valore di default. Verrà richiesto cambio password al login.'
        });
      },
      error: () => {
        this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Reset password non riuscito.' });
      }
    });
  }

  riattiva(): void {
    const utente = this.utenteSelezionato();
    if (!utente) {
      return;
    }

    this.utentiService.riattiva(utente.id).subscribe({
      next: () => {
        this.formUtente.patchValue({ disattivo: false });
        this.msg.add({ severity: 'success', summary: 'Riattivato', detail: 'Utente riattivato con successo.' });
        this.caricaUtenti();
      },
      error: () => {
        this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Riattivazione non riuscita.' });
      }
    });
  }

  filtraGlobale(tabella: Table, event: Event): void {
    const valore = (event.target as HTMLInputElement).value;
    tabella.filterGlobal(valore, 'contains');
  }

  private toDateOnly(value?: string | null): Date | null {
    if (!value) {
      return null;
    }

    const parsed = new Date(value);
    if (isNaN(parsed.getTime())) {
      return null;
    }

    return new Date(parsed.getFullYear(), parsed.getMonth(), parsed.getDate());
  }
}
