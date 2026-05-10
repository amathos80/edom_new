import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { ToolbarModule } from 'primeng/toolbar';

import { SmartTableComponent } from '../../../core/components/smart-table/smart-table.component';
import { SmartTableColumn } from '../../../core/components/smart-table/smart-table.model';
import { SmartTableActionsTemplateDirective } from '../../../core/components/smart-table/smart-table.templates';
import { Pua } from '../../../core/models/pua.model';
import { PuaService } from '../../../core/services/pua.service';

type PuaRow = Pua & {
  nominativo: string;
  dataDate: Date | null;
  dataAvvioDate: Date | null;
  dataChiusuraDate: Date | null;
  stato: string;
};

@Component({
  selector: 'app-pua-management',
  standalone: true,
  imports: [
    ButtonModule,
    CardModule,
    SmartTableComponent,
    SmartTableActionsTemplateDirective,
    ToastModule,
    ToolbarModule
  ],
  providers: [MessageService],
  templateUrl: './pua-management.component.html',
  styleUrl: './pua-management.component.scss'
})
export class PuaManagementComponent implements OnInit {
  private readonly puaService = inject(PuaService);
  private readonly router = inject(Router);
  private readonly msg = inject(MessageService);

  readonly loading = signal(false);
  readonly rows = signal<PuaRow[]>([]);

  readonly columns: SmartTableColumn[] = [
    { key: 'numeroPuaId', field: 'numeroPuaId', header: 'Tipo/Anno', filterType: 'numeric' },
    { key: 'numero', field: 'numero', header: 'Numero', filterType: 'numeric' },
    { key: 'dataDate', field: 'dataDate', header: 'Data', filterType: 'date' },
    { key: 'nominativo', field: 'nominativo', header: 'Utente', filterType: 'text' },
    { key: 'pazienteCodiceFiscale', field: 'pazienteCodiceFiscale', header: 'Cod. fisc.', filterType: 'text' },
    { key: 'accessoId', field: 'accessoId', header: 'Accesso', filterType: 'numeric' },
    { key: 'esitoId', field: 'esitoId', header: 'Esito', filterType: 'numeric' },
    { key: 'dataAvvioDate', field: 'dataAvvioDate', header: 'Data avvio', filterType: 'date' },
    { key: 'stato', field: 'stato', header: 'Stato', filterType: 'text' }
  ];

  readonly visibleColumnKeys = [
    'numeroPuaId',
    'numero',
    'dataDate',
    'nominativo',
    'pazienteCodiceFiscale',
    'esitoId',
    'stato'
  ];

  readonly globalFilterFields = this.columns.map(c => c.field);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);

    this.puaService.search({ take: 300 }).subscribe({
      next: (items) => {
        this.rows.set(items.map((p) => ({
          ...p,
          nominativo: [p.pazienteCognome, p.pazienteNome].filter(Boolean).join(' '),
          dataDate: this.toDateOnly(p.data),
          dataAvvioDate: this.toDateOnly(p.dataAvvio),
          dataChiusuraDate: this.toDateOnly(p.dataChiusura),
          stato: p.attivo ? 'Attivo' : 'Disattivato'
        })));
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Impossibile caricare i record PUA.' });
      }
    });
  }

  createNew(): void {
    this.router.navigate(['/app/pua/new']);
  }

  openEdit(row: unknown): void {
    const id = (row as { id?: number } | null)?.id;
    if (!id) {
      return;
    }

    this.router.navigate(['/app/pua', id]);
  }

  deleteRow(row: { id: number; numero: number }): void {
    if (!confirm(`Eliminare la pratica PUA n. ${row.numero}?`)) {
      return;
    }

    this.puaService.delete(row.id).subscribe({
      next: () => {
        this.msg.add({ severity: 'success', summary: 'Eliminato', detail: 'Record PUA eliminato.' });
        this.load();
      },
      error: () => {
        this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Eliminazione non riuscita.' });
      }
    });
  }

  private toDateOnly(value?: string | null): Date | null {
    if (!value) {
      return null;
    }

    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) {
      return null;
    }

    return new Date(parsed.getFullYear(), parsed.getMonth(), parsed.getDate());
  }
}
