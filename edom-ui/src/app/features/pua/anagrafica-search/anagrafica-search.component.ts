import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';

import { CustomTextboxInputComponent } from '../../custom-components/components/custom-textbox/custom-textbox.component';
import { DateInputComponent } from '../../../core/components/date-input/date-input.component';
import { AssistitiService } from '../../../core/services/assistiti.service';

export interface AnagraficaSearchFilters {
  cognome?: string;
  nome?: string;
  codiceFiscale?: string;
  dataNascita?: string;
}

export interface AnagraficaSearchResult {
  id: number;
  codice?: string | null;
  cognome: string;
  nome: string;
  codiceFiscale: string;
  dataNascita?: string | null;
  sesso?: string | null;
}

const PAGE_SIZE = 20;

@Component({
  selector: 'app-anagrafica-search',
  standalone: true,
  imports: [FormsModule, DatePipe, ButtonModule, InputTextModule, TableModule, TagModule, CustomTextboxInputComponent, DateInputComponent],
  templateUrl: './anagrafica-search.component.html',
  styleUrl: './anagrafica-search.component.scss'
})
export class AnagraficaSearchComponent {
  private readonly assistitiService = inject(AssistitiService);

  @Output() readonly selectedAssistito = new EventEmitter<AnagraficaSearchResult>();

  readonly loading = signal(false);
  readonly pageSize = signal(PAGE_SIZE);

  readonly filters = signal<AnagraficaSearchFilters>({
    cognome: '',
    nome: '',
    codiceFiscale: '',
    dataNascita: ''
  });

  readonly rows = signal<AnagraficaSearchResult[]>([]);
  readonly totalCount = signal(0);

  /** Chiamato dal bottone Cerca: resetta alla pagina 1 e carica */
  runSearch(): void {
    this.loadPage(1);
  }

  /** Chiamato da p-table (lazy) al cambio pagina o page size */
  onLazyLoad(event: TableLazyLoadEvent): void {
    const currentPageSize = event.rows ?? PAGE_SIZE;
    this.pageSize.set(currentPageSize);
    const page = event.first != null ? Math.floor(event.first / currentPageSize) + 1 : 1;
    this.loadPage(page, currentPageSize);
  }

  reset(): void {
    this.filters.set({ cognome: '', nome: '', codiceFiscale: '', dataNascita: '' });
    this.rows.set([]);
    this.totalCount.set(0);
  }

  choose(row: AnagraficaSearchResult): void {
    this.selectedAssistito.emit(row);
  }

  private loadPage(page: number, pageSize: number = PAGE_SIZE): void {
    const req = this.filters();
    this.loading.set(true);

    this.assistitiService.search({
      cognome: req.cognome,
      nome: req.nome,
      codiceFiscale: req.codiceFiscale,
      dataNascita: req.dataNascita,
      page,
      pageSize
    }).subscribe({
      next: (result) => {
        this.rows.set(result.items.map((p) => ({
          id: p.id,
          codice: p.codice ?? null,
          cognome: p.cognome,
          nome: p.nome,
          codiceFiscale: p.codiceFiscale,
          dataNascita: p.dataNascita ?? null,
          sesso: p.sesso ?? null
        })));
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }
}
