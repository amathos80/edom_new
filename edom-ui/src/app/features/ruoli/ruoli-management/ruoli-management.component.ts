import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { MessageService } from 'primeng/api';
import { Table, TableModule, TableLazyLoadEvent } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ToolbarModule } from 'primeng/toolbar';
import { SelectModule } from 'primeng/select';

import { RuoliService } from '../../../core/services/ruoli.service';
import { ProcedureService } from '../../../core/services/procedure.service';
import { ApiValidationMapperService } from '../../../core/services/api-validation-mapper.service';
import { FormValidationHelperService } from '../../../core/services/form-validation-helper.service';
import { LazyLoadProvider } from '../../../core/providers/lazy-load.provider';
import { AggiornaRuoloRequest, CreaRuoloRequest, Ruolo } from '../../../core/models/ruolo.model';
import { CondizioneFiltro } from '../../../core/models/pagination.model';
import { Procedura } from '../../../core/models/procedura.model';

type ModalitaDialog = 'crea' | 'modifica' | 'dettaglio';

@Component({
  selector: 'app-gestione-ruoli',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    DatePipe,
    ButtonModule,
    CardModule,
    CheckboxModule,
    DialogModule,
    IconFieldModule,
    InputTextModule,
    InputIconModule,
    TableModule,
    TagModule,
    ToastModule,
    ToolbarModule,
    SelectModule,
  ],
  providers: [MessageService],
  templateUrl: './ruoli-management.component.html',
  styleUrl: './ruoli-management.component.scss'
})
export class GestioneRuoliComponent implements OnInit {
  private readonly svc = inject(RuoliService);
  private readonly procedureService = inject(ProcedureService);
  private readonly fb = inject(FormBuilder);
  private readonly msg = inject(MessageService);
  private readonly lazyLoadProvider = inject(LazyLoadProvider);
  private readonly validationMapper = inject(ApiValidationMapperService);
  readonly formValidation = inject(FormValidationHelperService);

  ruoli = signal<Ruolo[]>([]);
  procedure = signal<Procedura[]>([]);
  ruoliSelezionati: Ruolo[] = [];
  caricamento = signal(false);
  caricamentoProcedure = signal(false);
  totaleRuoli = signal(0);

  dialogVisibile = signal(false);
  modalitaDialog = signal<ModalitaDialog>('crea');
  idSelezionato = signal<number | null>(null);

  // Filtri aggiuntivi (sidebar/toolbar)
  filtriAggiuntivi = {
    soloAmministratore: false,
  };

  formRuolo = this.fb.nonNullable.group({
    proceduraId: [1, [Validators.required, Validators.min(1)]],
    codice: ['', [Validators.required, Validators.maxLength(50)]],
    descrizione: ['', [Validators.required, Validators.maxLength(200)]],
    flagAmministratore: [false],
  });

  ngOnInit(): void {
    this.caricaProcedure();
  }

  /**
   * Gestisce l'evento lazy load della tabella PrimeNG
   */
  onLazyLoad(event: TableLazyLoadEvent): void {
    this.caricamento.set(true);

    const filtriAggiuntivi: CondizioneFiltro[] = [];
    if (this.filtriAggiuntivi.soloAmministratore) {
      filtriAggiuntivi.push({
        field: 'flagAmministratore',
        op: 'eq',
        value: true,
        type: 'boolean'
      });
    }

    const parametri = this.lazyLoadProvider.estraiParametri(event, filtriAggiuntivi, [
      'codice',
      'descrizione',
      'proceduraId'
    ]);
    const parametriNormalizzati = this.lazyLoadProvider.normalizzaParametri(parametri);

    // Chiama il service
    this.svc.cercaPaginata(parametriNormalizzati).subscribe({
      next: risposta => {
        this.ruoli.set(risposta.items);
        this.totaleRuoli.set(risposta.totale);
        this.caricamento.set(false);
      },
      error: err => {
        console.error('Errore ricerca paginata:', err);
        this.caricamento.set(false);
        this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Impossibile caricare i ruoli.' });
      }
    });
  }

  apriCreazione(): void {
    this.modalitaDialog.set('crea');
    this.idSelezionato.set(null);
    this.formRuolo.reset({
      proceduraId: 1,
      codice: '',
      descrizione: '',
      flagAmministratore: false,
    });
    this.formRuolo.enable();
    this.dialogVisibile.set(true);
  }

  apriModifica(ruolo: Ruolo): void {
    this.modalitaDialog.set('modifica');
    this.idSelezionato.set(ruolo.id);
    this.formRuolo.reset({
      proceduraId: ruolo.proceduraId,
      codice: ruolo.codice,
      descrizione: ruolo.descrizione,
      flagAmministratore: ruolo.flagAmministratore,
    });
    this.formRuolo.enable();
    this.dialogVisibile.set(true);
  }

  apriDettaglio(ruolo: Ruolo): void {
    this.modalitaDialog.set('dettaglio');
    this.idSelezionato.set(ruolo.id);
    this.formRuolo.reset({
      proceduraId: ruolo.proceduraId,
      codice: ruolo.codice,
      descrizione: ruolo.descrizione,
      flagAmministratore: ruolo.flagAmministratore,
    });
    this.formRuolo.disable();
    this.dialogVisibile.set(true);
  }

  salva(): void {
    if (this.modalitaDialog() === 'dettaglio') {
      this.dialogVisibile.set(false);
      return;
    }

    if (this.formRuolo.invalid) {
      this.formRuolo.markAllAsTouched();
      this.msg.add({
        severity: 'warn',
        summary: 'Validazione',
        detail: 'Compila i campi obbligatori prima di salvare.'
      });
      return;
    }

    const value = this.formRuolo.getRawValue();
    const payload: CreaRuoloRequest = {
      proceduraId: value.proceduraId,
      codice: value.codice.trim(),
      descrizione: value.descrizione.trim(),
      flagAmministratore: value.flagAmministratore,
    };

    if (this.modalitaDialog() === 'crea') {
      this.svc.crea(payload).subscribe({
        next: () => {
          this.dialogVisibile.set(false);
          this.msg.add({ severity: 'success', summary: 'Creato', detail: 'Ruolo creato con successo.' });
          this.ricaricaTabella();
        },
        error: (err) => {
          const mapped = this.validationMapper.applyToForm(this.formRuolo, err);
          if (!mapped) {
            this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Creazione ruolo non riuscita.' });
          }
        }
      });
      return;
    }

    const id = this.idSelezionato();
    if (!id) return;

    const payloadAggiornamento: AggiornaRuoloRequest = payload;
    this.svc.aggiorna(id, payloadAggiornamento).subscribe({
      next: () => {
        this.dialogVisibile.set(false);
        this.msg.add({ severity: 'success', summary: 'Aggiornato', detail: 'Ruolo aggiornato con successo.' });
        this.ricaricaTabella();
      },
      error: (err) => {
        const mapped = this.validationMapper.applyToForm(this.formRuolo, err);
        if (!mapped) {
          this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Aggiornamento ruolo non riuscito.' });
        }
      }
    });
  }

  elimina(ruolo: Ruolo): void {
    if (!confirm(`Eliminare il ruolo ${ruolo.codice}?`)) return;

    this.svc.elimina(ruolo.id).subscribe({
      next: () => {
        this.msg.add({ severity: 'success', summary: 'Eliminato', detail: 'Ruolo eliminato con successo.' });
        this.ricaricaTabella();
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Eliminazione ruolo non riuscita.' })
    });
  }

  eliminaSelezionati(): void {
    const selezionati = this.ruoliSelezionati;
    if (selezionati.length === 0) return;

    if (!confirm(`Eliminare ${selezionati.length} ruoli selezionati?`)) return;

    const ids = selezionati.map(r => r.id);
    let completati = 0;
    let errori = 0;

    ids.forEach(id => {
      this.svc.elimina(id).subscribe({
        next: () => {
          completati++;
          if (completati + errori === ids.length) {
            this.msg.add({
              severity: 'success',
              summary: 'Eliminazione completata',
              detail: `${completati} ruoli eliminati${errori > 0 ? `, ${errori} errori` : ''}.`
            });
            this.ricaricaTabella();
          }
        },
        error: () => {
          errori++;
          if (completati + errori === ids.length) {
            this.msg.add({
              severity: errori === ids.length ? 'error' : 'warn',
              summary: 'Eliminazione',
              detail: `${completati} ruoli eliminati${errori > 0 ? `, ${errori} errori` : ''}.`
            });
            this.ricaricaTabella();
          }
        }
      });
    });

    this.ruoliSelezionati = [];
  }

  filtraGlobale(tabella: Table, event: Event): void {
    const valore = (event.target as HTMLInputElement).value;
    tabella.filterGlobal(valore, 'contains');
  }

  pulisciFiltri(tabella: Table): void {
    this.filtriAggiuntivi.soloAmministratore = false;
    tabella.clear();
  }

  private ricaricaTabella(): void {
    this.totaleRuoli.set(0);
    this.ruoli.set([]);
  }

  private caricaProcedure(): void {
    this.caricamentoProcedure.set(true);

    this.procedureService.cerca({ soloVisibili: true }).subscribe({
      next: lista => {
        this.procedure.set(lista);
        this.caricamentoProcedure.set(false);
      },
      error: () => {
        this.caricamentoProcedure.set(false);
        this.msg.add({ severity: 'warn', summary: 'Attenzione', detail: 'Impossibile caricare la lista procedure.' });
      }
    });
  }
}
