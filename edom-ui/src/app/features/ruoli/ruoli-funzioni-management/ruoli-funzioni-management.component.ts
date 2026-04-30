import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CheckboxChangeEvent, CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { Table, TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';

import { Ruolo, RuoloFunzioneNodo } from '../../../core/models/ruolo.model';
import { RuoliService } from '../../../core/services/ruoli.service';

type NodoFunzioneView = {
  id: number;
  codice: string;
  descrizione: string;
  parentId?: number | null;
  selezionata: boolean;
  parziale: boolean;
  espanso: boolean;
  figlie: NodoFunzioneView[];
};

@Component({
  selector: 'app-ruoli-funzioni-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    CardModule,
    CheckboxModule,
    DialogModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    TableModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './ruoli-funzioni-management.component.html',
  styleUrl: './ruoli-funzioni-management.component.scss'
})
export class RuoliFunzioniManagementComponent implements OnInit {
  private readonly ruoliService = inject(RuoliService);
  private readonly messageService = inject(MessageService);

  readonly loadingRuoli = signal(false);
  readonly loadingFunzioni = signal(false);
  readonly saving = signal(false);
  readonly dialogVisibile = signal(false);

  readonly ruoli = signal<Ruolo[]>([]);
  readonly alberoFunzioni = signal<NodoFunzioneView[]>([]);
  readonly ruoloSelezionato = signal<Ruolo | null>(null);

  ngOnInit(): void {
    this.caricaRuoli();
  }

  caricaRuoli(): void {
    this.loadingRuoli.set(true);
    this.ruoliService.cerca({}).subscribe({
      next: (data) => {
        this.ruoli.set(data);
        this.loadingRuoli.set(false);
      },
      error: () => {
        this.loadingRuoli.set(false);
        this.messageService.add({ severity: 'error', summary: 'Errore', detail: 'Impossibile caricare i ruoli.' });
      }
    });
  }

  apriModificaFunzioni(ruolo: Ruolo): void {
    this.ruoloSelezionato.set(ruolo);
    this.loadingFunzioni.set(true);
    this.dialogVisibile.set(true);
    this.ruoliService.ottieniFunzioniRuolo(ruolo.id).subscribe({
      next: (nodi) => {
        const albero = nodi.map((n) => this.toViewNode(n));
        this.aggiornaStatiDerivati(albero);
        this.alberoFunzioni.set(albero);
        this.loadingFunzioni.set(false);
      },
      error: () => {
        this.loadingFunzioni.set(false);
        this.dialogVisibile.set(false);
        this.messageService.add({ severity: 'error', summary: 'Errore', detail: 'Impossibile caricare le funzioni del ruolo selezionato.' });
      }
    });
  }

  chiudiDialog(): void {
    this.dialogVisibile.set(false);
    this.ruoloSelezionato.set(null);
    this.alberoFunzioni.set([]);
  }

  toggleNodo(nodo: NodoFunzioneView, event: CheckboxChangeEvent): void {
    const checked = !!event.checked;
    this.setNodoRicorsivo(nodo, checked);
    this.aggiornaStatiDerivati(this.alberoFunzioni());
  }

  espandiComprimiTutto(expand: boolean): void {
    const visita = (nodi: NodoFunzioneView[]): void => {
      for (const nodo of nodi) {
        if (nodo.figlie.length > 0) {
          nodo.espanso = expand;
          visita(nodo.figlie);
        }
      }
    };

    visita(this.alberoFunzioni());
    this.alberoFunzioni.set([...this.alberoFunzioni()]);
  }

  salva(): void {
    const ruolo = this.ruoloSelezionato();
    if (!ruolo) {
      return;
    }

    const funzioneIds = this.collectSelectedIds(this.alberoFunzioni());
    this.saving.set(true);
    this.ruoliService.aggiornaFunzioniRuolo(ruolo.id, funzioneIds).subscribe({
      next: () => {
        this.saving.set(false);
        this.dialogVisibile.set(false);
        this.messageService.add({ severity: 'success', summary: 'Salvato', detail: 'Abilitazioni ruolo-funzioni aggiornate.' });
      },
      error: () => {
        this.saving.set(false);
        this.messageService.add({ severity: 'error', summary: 'Errore', detail: 'Salvataggio abilitazioni non riuscito.' });
      }
    });
  }

  filtraGlobale(tabella: Table, event: Event): void {
    const valore = (event.target as HTMLInputElement).value;
    tabella.filterGlobal(valore, 'contains');
  }

  private toViewNode(nodo: RuoloFunzioneNodo): NodoFunzioneView {
    return {
      ...nodo,
      parziale: false,
      espanso: true,
      figlie: (nodo.figlie ?? []).map((f) => this.toViewNode(f))
    };
  }

  private setNodoRicorsivo(nodo: NodoFunzioneView, checked: boolean): void {
    nodo.selezionata = checked;
    nodo.parziale = false;
    for (const figlia of nodo.figlie) {
      this.setNodoRicorsivo(figlia, checked);
    }
  }

  private aggiornaStatiDerivati(nodi: NodoFunzioneView[]): void {
    const visita = (nodo: NodoFunzioneView): { all: boolean; any: boolean } => {
      if (nodo.figlie.length === 0) {
        nodo.parziale = false;
        return { all: nodo.selezionata, any: nodo.selezionata };
      }

      const risultati = nodo.figlie.map((f) => visita(f));
      const all = risultati.every((r) => r.all);
      const any = risultati.some((r) => r.any);

      if (all) {
        nodo.selezionata = true;
        nodo.parziale = false;
      } else if (any) {
        nodo.selezionata = false;
        nodo.parziale = true;
      } else {
        nodo.parziale = false;
      }

      return { all: nodo.selezionata && !nodo.parziale, any: nodo.selezionata || nodo.parziale };
    };

    for (const nodo of nodi) {
      visita(nodo);
    }
    this.alberoFunzioni.set([...nodi]);
  }

  private collectSelectedIds(nodi: NodoFunzioneView[]): number[] {
    const result: number[] = [];

    const visita = (nodo: NodoFunzioneView): void => {
      if (nodo.selezionata) {
        result.push(nodo.id);
      }
      for (const figlia of nodo.figlie) {
        visita(figlia);
      }
    };

    for (const nodo of nodi) {
      visita(nodo);
    }

    return result;
  }
}
