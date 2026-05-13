import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputNumberModule } from 'primeng/inputnumber';
import { DatePickerModule } from 'primeng/datepicker';
import { TabsModule } from 'primeng/tabs';
import { MessageService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { TextareaModule } from 'primeng/textarea';
import { AreaDto, CreatePuaRequest, Pua, UpdatePuaRequest } from '../../../core/models/pua.model';
import { Paziente, PazientePuaData } from '../../../core/models/paziente.model';
import { PuaService } from '../../../core/services/pua.service';
import { PazientiService } from '../../../core/services/pazienti.service';
import { LookupInputComponent } from '../../../core/components/lookup-input/lookup-input.component';
import { LookupDialogContentDirective } from '../../../core/components/lookup-input/lookup-dialog-content.directive';
import { AnagraficaSearchComponent, AnagraficaSearchResult } from '../anagrafica-search/anagrafica-search.component';
import { SessoSelectComponent } from '../../../core/components/sesso-select/sesso-select.component';
import { NumeroPuaSelectComponent } from '../../../core/components/numero-pua-select/numero-pua-select.component';
import { DateInputComponent } from '../../../core/components/date-input/date-input.component';
import { CustomTextboxInputComponent } from '../../custom-components/components/custom-textbox/custom-textbox.component';
import { CustomSelectInputComponent } from '../../custom-components/components/custom-select/custom-select.component';
import { CustomTextareaInputComponent } from '../../custom-components/components/custom-textarea/custom-textarea.component';
import { CustomInputNumberComponent } from "../../custom-components/components/custom-inputnumber/custom-inputnumber.component";

type PuaTab = 'dati-generali' | 'gestione-domanda';
type AreaOption = { label: string; value: number };

@Component({
  selector: 'app-pua-edit',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    InputNumberModule,
    DatePickerModule,
    TabsModule,
    ToastModule,
    ConfirmDialogModule,
    LookupInputComponent,
    LookupDialogContentDirective,
    AnagraficaSearchComponent,
    SessoSelectComponent,
    NumeroPuaSelectComponent,
    DateInputComponent,
    CustomTextboxInputComponent,
    CustomSelectInputComponent,
    TextareaModule,
    CustomTextareaInputComponent,
    CustomInputNumberComponent
],
  providers: [MessageService, ConfirmationService],
  templateUrl: './pua-edit.component.html',
  styleUrl: './pua-edit.component.scss'
})
export class PuaEditComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly puaService = inject(PuaService);
  private readonly pazientiService = inject(PazientiService);
  private readonly msg = inject(MessageService);
  private readonly confirm = inject(ConfirmationService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly editMode = signal(false);
  readonly puaId = signal<number | null>(null);
  readonly activeTab = signal<PuaTab>('dati-generali');
  readonly areeResidenzaOptions = signal<AreaOption[]>([]);
  readonly areeDomicilioOptions = signal<AreaOption[]>([]);
  readonly areeReperibilitaOptions = signal<AreaOption[]>([]);

  readonly form = this.fb.nonNullable.group({
    numeroPuaId: [null as number | null],
    numero: [  null as number | null],
    data: [''],

    pazienteId: [0],
    pazienteCognome: [''],
    pazienteNome: [{ value: '', disabled: true }, [Validators.required, Validators.maxLength(100)]],
    pazienteCodiceFiscale: [{ value: '', disabled: true }, Validators.maxLength(20)],
    pazienteDataNascita: [{ value: '', disabled: true }],
    pazienteSearch: [''],

    areaInterventoId: [0, [Validators.required, Validators.min(1)]],
    origineId: [null as number | null, [Validators.required, Validators.min(1)]],
    dataAvvio: ['', Validators.required],

    accessoId: [null as number | null, [Validators.required, Validators.min(1)]],
    accessoNote: ['', Validators.maxLength(2000)],

    motivoId: [null as number | null],
    motivoNote: ['', Validators.maxLength(2000)],

    richiestaId: [null as number | null, [Validators.required, Validators.min(1)]],
    richiestaAltro: ['', Validators.maxLength(2000)],

    esitoId: [null as number | null, [Validators.required, Validators.min(1)]],
    esitoNote: ['', Validators.maxLength(2000)],

    urgente: [false],
    attivo: [true],

    dataChiusura: [''],
    motivoChiusuraId: [null as number | null],

    // Campi UI per rispecchiare il layout legacy (non persistiti lato API).
    uiSesso: ['M' as 'M' | 'F' | ''],
    uiEmail: [''],
    uiTelefono1: [''],
    uiTelefono2: [''],
    uiComuneResidenza: [''],
    uiIndirizzoResidenza: [''],
    uiCapResidenza: [''],
    uiAreaResidenzaId: [null as number | null],
    uiComuneDomicilio: [''],
    uiIndirizzoDomicilio: [''],
    uiCapDomicilio: [''],
    uiAreaDomicilioId: [null as number | null],
    uiComuneAltroRecapito: [''],
    uiIndirizzoAltroRecapito: [''],
    uiCapAltroRecapito: [''],
    uiAreaReperibilitaId: [null as number | null],
    uiNoteAltroRecapito: [''],
    uiMedicoCodice: [''],
    uiMedicoNominativo: [''],
    uiMedicoEmail: [''],
    uiMedicoTelefono1: [''],
    uiMedicoTelefono2: [''],
    uiMedicoNote: [''],

    uiTipoSegnalazione: [''],
    uiSegnalanteCognome: [''],
    uiSegnalanteNome: [''],
    uiSegnalanteDataNascita: [''],
    uiSegnalanteComuneNascita: [''],
    uiSegnalanteComuneResidenza: [''],
    uiSegnalanteIndirizzo: [''],
    uiSegnalanteRelazione: [''],
    uiSegnalanteTelefono1: [''],
    uiSegnalanteTelefono2: [''],

    uiReferenteCognome: [''],
    uiReferenteNome: [''],
    uiReferenteDataNascita: [''],
    uiReferenteComuneNascita: [''],
    uiReferenteComuneResidenza: [''],
    uiReferenteIndirizzo: [''],
    uiReferenteRelazione: [''],
    uiReferenteTelefono1: [''],
    uiReferenteTelefono2: [''],

    uiViveSolo: [false]
  });

  ngOnInit(): void {
    this.loadAreeOptions();

    const idParam = this.route.snapshot.paramMap.get('id');

    if (!idParam) {
      this.editMode.set(false);
      this.prepareDefaults();
      return;
    }

    const id = Number(idParam);
    if (Number.isNaN(id) || id <= 0) {
      this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Identificativo PUA non valido.' });
      this.goBack();
      return;
    }

    this.editMode.set(true);
    this.puaId.set(id);
    this.load(id);
  }

  setTab(tab: string | number | undefined): void {
    if (tab != null) this.activeTab.set(tab as PuaTab);
  }

  onPazienteSearchClick(): void {
    // Emit event to signal search initiated (for analytics/tracking if needed)
    console.log('User initiated paziente search');
  }

  onAssistitoSelected(paziente: AnagraficaSearchResult, closeDialog: () => void): void {
    // Close dialog immediately
    closeDialog();

    // Check if patient comes from V_ANAGRAFE_ASSISTITI (codice is empty)
    if (!paziente.codice || paziente.codice.trim() === '') {
      // Patient from V_ANAGRAFE_ASSISTITI - show confirmation before creating
      this.confirm.confirm({
        message: "Attenzione, il paziente è stato prelevato nell'anagrafe assistiti. Verrà creata un'analoga voce nell'anagrafe pazienti al salvataggio.",
        header: 'Creazione Paziente',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
          this.createPazienteFromAnagrafe(paziente.id);
        },
        reject: () => {
          // Do nothing
        }
      });
    } else {
      // Normal patient from CO_PAZIENTI - fetch full details with resolved lookups
      this.loading.set(true);
      this.pazientiService.getPuaData(paziente.id).subscribe({
        next: (data: PazientePuaData) => {
          this.populateForm(data);
          this.loading.set(false);
        },
        error: () => {
          this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Impossibile caricare i dati del paziente.' });
          this.loading.set(false);
        }
      });
    }
  }

  private createPazienteFromAnagrafe(assistitoId: number): void {
    this.loading.set(true);
    // Call backend endpoint to create patient from assistito, then load pua-data
    this.pazientiService.createFromAssistito(assistitoId).subscribe({
      next: (createdPaziente: Paziente) => {
        // Fetch full pua-data for the newly created patient
        this.pazientiService.getPuaData(createdPaziente.id).subscribe({
          next: (data: PazientePuaData) => {
            this.populateForm(data);
            this.msg.add({
              severity: 'success',
              summary: 'Paziente Creato',
              detail: `Nuovo paziente ${createdPaziente.cognome} ${createdPaziente.nome} creato con successo.`
            });
            this.loading.set(false);
          },
          error: () => {
            this.populateFormFromPaziente(createdPaziente);
            this.loading.set(false);
          }
        });
      },
      error: () => {
        this.msg.add({
          severity: 'error',
          summary: 'Errore',
          detail: 'Impossibile creare il paziente.'
        });
        this.loading.set(false);
      }
    });
  }

  private populateForm(data: PazientePuaData): void {
    this.form.patchValue({
      pazienteId: data.id,
      pazienteCognome: data.cognome,
      pazienteNome: data.nome,
      pazienteCodiceFiscale: data.codiceFiscale || '',
      pazienteDataNascita: data.dataNascita || '',
      uiSesso: (data.sesso as 'M' | 'F') || 'M',
      uiEmail: data.email || '',
      uiTelefono1: data.telefono1 || '',
      uiTelefono2: data.telefono2 || '',
      uiComuneResidenza: data.comuneResidenzaDescr || '',
      uiIndirizzoResidenza: data.indirizzoResidenza || '',
      uiCapResidenza: data.capResidenza || '',
      uiAreaResidenzaId: data.areaResidenzaId || null,
      uiComuneDomicilio: data.comuneDomicilioDescr || '',
      uiIndirizzoDomicilio: data.indirizzoDomicilio || '',
      uiCapDomicilio: data.capDomicilio || '',
      uiAreaDomicilioId: data.areaDomicilioId || null,
      uiMedicoCodice: data.medicoCodice || '',
      uiMedicoNominativo: data.medicoNominativo || '',
      uiMedicoEmail: data.medicoEmail || '',
      uiMedicoTelefono1: data.medicoTelefono1 || '',
      uiMedicoTelefono2: data.medicoTelefono2 || '',
      pazienteSearch:data.nomeCompleto,
      uiCapAltroRecapito:data.capReperibilita || '',
      uiComuneAltroRecapito:data.comuneReperibilitaDescr || '',
      uiIndirizzoAltroRecapito:data.indirizzoReperibilita || '',
      uiAreaReperibilitaId: data.areaReperibilitaId || null,

    });
    this.form.markAsTouched();
  }

  private populateFormFromPaziente(p: Paziente): void {
    this.form.patchValue({
      pazienteId: p.id,
      pazienteCognome: p.cognome,
      pazienteNome: p.nome,
      pazienteCodiceFiscale: p.codiceFiscale || '',
      pazienteDataNascita: p.dataNascita || '',
      uiSesso: (p.sesso as 'M' | 'F') || 'M',
      uiEmail: p.email || '',
      uiTelefono1: p.telefono1 || '',
      uiTelefono2: p.telefono2 || '',
      uiIndirizzoResidenza: p.indirizzoResidenza || '',
      uiCapResidenza: p.capResidenza || '',
      uiAreaResidenzaId: null,
      uiIndirizzoDomicilio: p.indirizzoDomicilio || '',
      uiCapDomicilio: p.capDomicilio || '',
      uiAreaDomicilioId: null,
      uiAreaReperibilitaId: null
    });
    this.form.markAsTouched();
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.msg.add({ severity: 'warn', summary: 'Validazione', detail: 'Compila i campi obbligatori.' });
      return;
    }

    const payload = this.toPayload();
    if (!payload) {
      this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Dati non validi per il salvataggio.' });
      return;
    }

    this.saving.set(true);

    if (this.editMode() && this.puaId()) {
      this.puaService.update(this.puaId()!, payload as UpdatePuaRequest).subscribe({
        next: (updated) => {
          this.saving.set(false);
          this.patchForm(updated);
          this.msg.add({ severity: 'success', summary: 'Salvato', detail: 'Pratica PUA aggiornata.' });
        },
        error: () => {
          this.saving.set(false);
          this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Aggiornamento non riuscito.' });
        }
      });
      return;
    }

    this.puaService.create(payload as CreatePuaRequest).subscribe({
      next: (created) => {
        this.saving.set(false);
        this.msg.add({ severity: 'success', summary: 'Creata', detail: 'Pratica PUA creata correttamente.' });
        this.router.navigate(['/app/pua', created.id]);
      },
      error: () => {
        this.saving.set(false);
        this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Creazione non riuscita.' });
      }
    });
  }

  reset(): void {
    if (this.editMode() && this.puaId()) {
      this.load(this.puaId()!);
      return;
    }

    this.prepareDefaults();
  }

  goBack(): void {
    this.router.navigate(['/app/pua']);
  }

  duplicate(): void {
    if (!this.editMode() || !this.puaId()) {
      return;
    }

    this.puaService.duplicate(this.puaId()!, {}).subscribe({
      next: (created) => {
        this.msg.add({ severity: 'success', summary: 'Duplicata', detail: 'Pratica PUA duplicata.' });
        this.router.navigate(['/app/pua', created.id]);
      },
      error: () => {
        this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Duplicazione non riuscita.' });
      }
    });
  }

  private load(id: number): void {
    this.loading.set(true);
    this.puaService.getById(id).subscribe({
      next: (item) => {
        this.patchForm(item);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Impossibile caricare la pratica PUA.' });
        this.goBack();
      }
    });
  }

  private patchForm(item: Pua): void {
    this.form.patchValue({
      numeroPuaId: item.numeroPuaId,
      numero: item.numero,
      data: this.toInputDate(item.data),
      pazienteId: item.pazienteId,
      pazienteCognome: item.pazienteCognome,
      pazienteNome: item.pazienteNome,
      pazienteCodiceFiscale: item.pazienteCodiceFiscale ?? '',
      pazienteDataNascita: item.pazienteDataNascita ?? '',
      areaInterventoId: item.areaInterventoId,
      origineId: item.origineId,
      dataAvvio: this.toInputDate(item.dataAvvio),
      accessoId: item.accessoId,
      accessoNote: item.accessoNote ?? '',
      motivoId: item.motivoId ?? 0,
      motivoNote: item.motivoNote ?? '',
      richiestaId: item.richiestaId,
      richiestaAltro: item.richiestaAltro ?? '',
      esitoId: item.esitoId,
      esitoNote: item.esitoNote ?? '',
      urgente: item.urgente,
      attivo: item.attivo,
      dataChiusura: this.toInputDate(item.dataChiusura),
      motivoChiusuraId: item.motivoChiusuraId ?? null,
    
    });
  }

  private prepareDefaults(): void {
    const today = this.toInputDate(new Date().toISOString());
    this.form.reset({
      numeroPuaId: null,
      numero: null,
      data: today,
      pazienteId: 0,
      pazienteCognome: '',
      pazienteNome: '',
      pazienteCodiceFiscale: '',
      pazienteDataNascita: '',
      pazienteSearch: '',
      areaInterventoId: 0,
      origineId: null,
      dataAvvio: today,
      accessoId: null,
      accessoNote: '',
      motivoId: null,
      motivoNote: '',
      richiestaId: null,
      richiestaAltro: '',
      esitoId: null,
      esitoNote: '',
      urgente: false,
      attivo: true,
      dataChiusura: '',
      motivoChiusuraId: null,
      uiSesso: '',
      uiEmail: '',
      uiTelefono1: '',
      uiTelefono2: '',
      uiComuneResidenza: '',
      uiIndirizzoResidenza: '',
      uiCapResidenza: '',
      uiAreaResidenzaId: null,
      uiComuneDomicilio: '',
      uiIndirizzoDomicilio: '',
      uiCapDomicilio: '',
      uiAreaDomicilioId: null,
      uiComuneAltroRecapito: '',
      uiIndirizzoAltroRecapito: '',
      uiCapAltroRecapito: '',
      uiAreaReperibilitaId: null,
      uiNoteAltroRecapito: '',
      uiMedicoCodice: '',
      uiMedicoNominativo: '',
      uiMedicoEmail: '',
      uiMedicoTelefono1: '',
      uiMedicoTelefono2: '',
      uiMedicoNote: '',
      uiTipoSegnalazione: '',
      uiSegnalanteCognome: '',
      uiSegnalanteNome: '',
      uiSegnalanteDataNascita: '',
      uiSegnalanteComuneNascita: '',
      uiSegnalanteComuneResidenza: '',
      uiSegnalanteIndirizzo: '',
      uiSegnalanteRelazione: '',
      uiSegnalanteTelefono1: '',
      uiSegnalanteTelefono2: '',
      uiReferenteCognome: '',
      uiReferenteNome: '',
      uiReferenteDataNascita: '',
      uiReferenteComuneNascita: '',
      uiReferenteComuneResidenza: '',
      uiReferenteIndirizzo: '',
      uiReferenteRelazione: '',
      uiReferenteTelefono1: '',
      uiReferenteTelefono2: '',
      uiViveSolo: false
    });
  }

  private loadAreeOptions(): void {
    this.puaService.getAree().subscribe({
      next: (rows: AreaDto[]) => {
        const options: AreaOption[] = rows.map(x => ({
          label: `${x.codice} - ${x.descrizione}`,
          value: x.id
        }));

        this.areeResidenzaOptions.set(options);
        this.areeDomicilioOptions.set(options);
        this.areeReperibilitaOptions.set(options);
      },
      error: () => {
        this.msg.add({
          severity: 'warn',
          summary: 'Attenzione',
          detail: 'Impossibile caricare il lookup delle aree.'
        });
      }
    });
  }

  private toPayload(): CreatePuaRequest | null {
    const v = this.form.getRawValue();

    const data = this.toApiDate(v.data);
    const dataAvvio = this.toApiDate(v.dataAvvio);

    if (!data || !dataAvvio) {
      return null;
    }

    return {
      numeroPuaId: Number(v.numeroPuaId),
      data,
      areaInterventoId: Number(v.areaInterventoId),
      pazienteId: Number(v.pazienteId),
      pazienteCognome: v.pazienteCognome.trim(),
      pazienteNome: v.pazienteNome.trim(),
      pazienteCodiceFiscale: this.nullIfBlank(v.pazienteCodiceFiscale),
      accessoId: Number(v.accessoId),
      accessoNote: this.nullIfBlank(v.accessoNote),
      motivoId: Number(v.motivoId) || null,
      motivoNote: this.nullIfBlank(v.motivoNote),
      richiestaId: Number(v.richiestaId),
      richiestaAltro: this.nullIfBlank(v.richiestaAltro),
      esitoId: Number(v.esitoId),
      esitoNote: this.nullIfBlank(v.esitoNote),
      urgente: !!v.urgente,
      origineId: Number(v.origineId),
      dataAvvio,
      dataChiusura: this.toApiDate(v.dataChiusura),
      motivoChiusuraId: Number(v.motivoChiusuraId) || null,
      attivo: !!v.attivo
    };
  }

  private nullIfBlank(value: string | null | undefined): string | null {
    const trimmed = (value ?? '').trim();
    return trimmed ? trimmed : null;
  }

  private toInputDate(value?: string | null): string {
    if (!value) {
      return '';
    }

    const d = new Date(value);
    if (Number.isNaN(d.getTime())) {
      return '';
    }

    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  private toApiDate(value?: string | null): string | null {
    if (!value) {
      return null;
    }

    const d = new Date(value);
    if (Number.isNaN(d.getTime())) {
      return null;
    }

    return d.toISOString();
  }
}
