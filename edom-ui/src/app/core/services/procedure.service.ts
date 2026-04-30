import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Procedura, RicercaProceduraRequest } from '../models/procedura.model';

@Injectable({ providedIn: 'root' })
export class ProcedureService {
  private readonly http = inject(HttpClient);
  private readonly urlBase = `${environment.apiUrl}/procedure`;

  cerca(request: RicercaProceduraRequest = {}): Observable<Procedura[]> {
    let params = new HttpParams();

    if (request.codice) {
      params = params.set('codice', request.codice);
    }

    if (request.descrizione) {
      params = params.set('descrizione', request.descrizione);
    }

    if (request.soloVisibili !== undefined) {
      params = params.set('soloVisibili', String(request.soloVisibili));
    }

    return this.http.get<Procedura[]>(this.urlBase, { params });
  }

  ottieniPerCodice(codice: string): Observable<Procedura> {
    return this.http.get<Procedura>(`${this.urlBase}/codice/${encodeURIComponent(codice)}`);
  }
}
