import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, take } from 'rxjs';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ToolbarModule } from 'primeng/toolbar';

import { FormValidationHelperService } from '../../../core/services/form-validation-helper.service';
import { AggiornaUtenteRequest, CreaUtenteRequest, Utente } from '../../../core/models/utente.model';
import { UtentiService } from '../../../core/services/utenti.service';
import { SmartTableComponent } from '../../../core/components/smart-table/smart-table.component';
import { SmartTableColumn } from '../../../core/components/smart-table/smart-table.model';
import { SmartTableActionsTemplateDirective, SmartTableCellTemplateDirective } from '../../../core/components/smart-table/smart-table.templates';
import { FormFieldComponent } from '../../../core/components/form-field/form-field.component';
import { CambioPasswordDialogComponent } from './cambio-password-dialog/cambio-password-dialog.component';

type ModalitaDialog = 'crea' | 'modifica';
type VistaTabella = 'operativa' | 'sicurezza' | 'anagrafica' | 'custom';
type ColonnaKey =
  | 'codice'
  | 'nominativo'
  | 'cognome'
  | 'nome'
  | 'codiceFiscale'
  | 'email'
  | 'dataScadenzaPasswordDate'
  | 'dataDisattivazioneDate'
  | 'dataRiattivazioneDate'
  | 'ultimoLoginDate'
  | 'stato';

@Component({
  selector: 'app-utenti-management',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    CheckboxModule,
    DialogModule,
    InputTextModule,
    FormFieldComponent,
    SmartTableComponent,
    SmartTableCellTemplateDirective,
    SmartTableActionsTemplateDirective,
    TagModule,
    ToastModule,
    ToolbarModule,
    CambioPasswordDialogComponent
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

  readonly dialogCambioPasswordVisibile = signal(false);
  readonly savingCambioPassword = signal(false);

  readonly utenti = signal<Utente[]>([]);
  readonly utenteSelezionato = signal<Utente | null>(null);

  readonly tutteLeColonne: SmartTableColumn[] = [
    { key: 'codice', field: 'codice', header: 'Codice', filterType: 'text' },
    { key: 'nominativo', field: 'nominativo', header: 'Cognome e Nome', filterType: 'text' },
    { key: 'cognome', field: 'cognome', header: 'Cognome', filterType: 'text' },
    { key: 'nome', field: 'nome', header: 'Nome', filterType: 'text' },
    { key: 'codiceFiscale', field: 'codiceFiscale', header: 'Cod. fiscale', fullHeader: 'Codice fiscale', filterType: 'text' },
    { key: 'email', field: 'email', header: 'Email', filterType: 'text' },
    {
      key: 'dataScadenzaPasswordDate',
      field: 'dataScadenzaPasswordDate',
      header: 'Scad. pwd',
      fullHeader: 'Data scadenza password',
      filterType: 'date'
    },
    {
      key: 'dataDisattivazioneDate',
      field: 'dataDisattivazioneDate',
      header: 'Disattivato il',
      fullHeader: 'Data disattivazione',
      filterType: 'date'
    },
    {
      key: 'dataRiattivazioneDate',
      field: 'dataRiattivazioneDate',
      header: 'Riattivato il',
      fullHeader: 'Data riattivazione',
      filterType: 'date'
    },
    {
      key: 'ultimoLoginDate',
      field: 'ultimoLoginDate',
      header: 'Ultimo accesso',
      fullHeader: 'Data ultimo accesso',
      filterType: 'date'
    },
    { key: 'stato', field: 'stato', header: 'Stato', filterType: 'text' }
  ];

  readonly colonneSemprePresenti: ColonnaKey[] = ['codice', 'nominativo', 'stato'];

  private readonly presetColonne: Record<Exclude<VistaTabella, 'custom'>, ColonnaKey[]> = {
    operativa: ['codice', 'nominativo', 'email', 'stato', 'ultimoLoginDate'],
    sicurezza: ['codice', 'nominativo', 'email', 'dataScadenzaPasswordDate', 'ultimoLoginDate', 'stato'],
    anagrafica: ['codice', 'cognome', 'nome', 'codiceFiscale', 'email', 'stato']
  };

  readonly presetAttivo = signal<VistaTabella>('operativa');
  readonly colonneVisibili = signal<ColonnaKey[]>(this.presetColonne.operativa);
  readonly colonneSelezionate = computed(() => this.colonneVisibili());
  readonly campiFiltroGlobale = this.tutteLeColonne.map((c) => c.field);

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
    this.utentiService.cerca({}).pipe(
      take(1),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (items) => {
        try {
          const normalizedItems = this.normalizeUtentiResponse(items);
          this.utenti.set(normalizedItems.map((u) => ({
            ...u,
            nominativo: [u.cognome, u.nome].filter(Boolean).join(' '),
            dataScadenzaPasswordDate: this.toDateOnly(u.dataScadenzaPassword),
            dataDisattivazioneDate: this.toDateOnly(u.dataDisattivazione),
            dataRiattivazioneDate: this.toDateOnly(u.dataRiattivazione),
            ultimoLoginDate: this.toDateOnly(u.ultimoLogin),
            stato: u.dataDisattivazione ? 'Disattivato' : 'Attivo'
          })));
        } catch {
          this.utenti.set([]);
          this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Formato risposta utenti non valido.' });
        }
      },
      error: () => {
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

  apriCambioPassword(): void {
    if (!this.utenteSelezionato()) {
      return;
    }
    this.dialogCambioPasswordVisibile.set(true);
  }

  chiudiCambioPassword(): void {
    this.dialogCambioPasswordVisibile.set(false);
  }

  onPasswordCambiata(): void {
    this.dialogCambioPasswordVisibile.set(false);
    this.caricaUtenti();
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

  applicaPreset(vista: Exclude<VistaTabella, 'custom'>): void {
    this.presetAttivo.set(vista);
    this.colonneVisibili.set([...this.presetColonne[vista]]);
  }

  aggiornaColonne(colonne: string[] | null | undefined): void {
    const nuovaLista = this.tutteLeColonne
      .map((c) => c.key as ColonnaKey)
      .filter((key) => (colonne ?? []).includes(key));

    this.colonneVisibili.set(nuovaLista);

    const presetTrovato = (Object.entries(this.presetColonne) as [Exclude<VistaTabella, 'custom'>, ColonnaKey[]][])
      .find(([, preset]) => this.stesseColonne(preset, nuovaLista));

    this.presetAttivo.set(presetTrovato?.[0] ?? 'custom');
  }

  private stesseColonne(left: ColonnaKey[], right: ColonnaKey[]): boolean {
    if (left.length !== right.length) {
      return false;
    }

    const rightSet = new Set(right);
    return left.every((item) => rightSet.has(item));
  }

  private normalizeUtentiResponse(items: unknown): Utente[] {
    if (Array.isArray(items)) {
      return items as Utente[];
    }

    const wrappedItems = (items as { items?: unknown })?.items;
    if (Array.isArray(wrappedItems)) {
      return wrappedItems as Utente[];
    }

    return [];
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
