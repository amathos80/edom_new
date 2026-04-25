import { Component, inject, signal, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { Paziente, PazienteSearchRequest } from '../../../core/models/paziente.model';
import { PazientiService } from '../../../core/services/pazienti.service';

@Component({
  selector: 'app-pazienti-list',
  standalone: true,
  imports: [
    FormsModule, DatePipe,
    CardModule, TableModule, InputTextModule, ButtonModule,
    TagModule, IconFieldModule, InputIconModule, ToastModule
  ],
  providers: [MessageService],
  templateUrl: './pazienti-list.component.html',
  styleUrl: './pazienti-list.component.scss'
})
export class PazientiListComponent implements OnInit {
  private readonly svc = inject(PazientiService);
  private readonly msg = inject(MessageService);

  pazienti = signal<Paziente[]>([]);
  loading = signal(false);

  filter: PazienteSearchRequest = { cognome: '', codiceFiscale: '', attivo: undefined };

  ngOnInit(): void {
    this.search();
  }

  search(): void {
    this.loading.set(true);
    const req: PazienteSearchRequest = {
      cognome: this.filter.cognome || undefined,
      codiceFiscale: this.filter.codiceFiscale || undefined,
      attivo: this.filter.attivo
    };
    this.svc.search(req).subscribe({
      next: data => { this.pazienti.set(data); this.loading.set(false); },
      error: () => {
        this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Impossibile caricare i pazienti' });
        this.loading.set(false);
      }
    });
  }

  reset(): void {
    this.filter = { cognome: '', codiceFiscale: '', attivo: undefined };
    this.search();
  }

  delete(paziente: Paziente): void {
    if (!confirm(`Eliminare ${paziente.nomeCompleto}?`)) return;
    this.svc.delete(paziente.id).subscribe({
      next: () => {
        this.pazienti.update(list => list.filter(p => p.id !== paziente.id));
        this.msg.add({ severity: 'success', summary: 'Eliminato', detail: `${paziente.nomeCompleto} eliminato` });
      },
      error: () => this.msg.add({ severity: 'error', summary: 'Errore', detail: 'Impossibile eliminare il paziente' })
    });
  }
}
