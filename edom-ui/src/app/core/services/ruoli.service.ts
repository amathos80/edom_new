import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AggiornaRuoloRequest, CreaRuoloRequest, RicercaRuoloRequest, Ruolo, RuoloFunzioneNodo } from '../models/ruolo.model';
import { ParametriRicercaPaginata, RispostaPaginata } from '../models/pagination.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class RuoliService {
  private readonly http = inject(HttpClient);
  private readonly urlBase = `${environment.apiUrl}/ruoli`;

  cerca(request: RicercaRuoloRequest): Observable<Ruolo[]> {
    let params = new HttpParams();
    if (request.codice) params = params.set('codice', request.codice);
    if (request.descrizione) params = params.set('descrizione', request.descrizione);
    if (request.flagAmministratore !== undefined) params = params.set('flagAmministratore', String(request.flagAmministratore));
    if (request.proceduraId !== undefined) params = params.set('proceduraId', String(request.proceduraId));
    return this.http.get<Ruolo[]>(this.urlBase, { params });
  }

  /**
   * Ricerca paginata con supporto a filtri, ordinamento e paginazione lato server
   */
  cercaPaginata(parametri: ParametriRicercaPaginata): Observable<RispostaPaginata<Ruolo>> {
    let httpParams = new HttpParams()
      .set('skip', String(parametri.skip))
      .set('take', String(parametri.take));

    if (parametri.sort) {
      httpParams = httpParams.set('sort', parametri.sort);
    }

    if (parametri.filter) {
      httpParams = httpParams.set('filter', JSON.stringify(parametri.filter));
    }

    return this.http.get<RispostaPaginata<Ruolo>>(`${this.urlBase}/paginated`, { params: httpParams });
  }

  ottieniPerId(id: number): Observable<Ruolo> {
    return this.http.get<Ruolo>(`${this.urlBase}/${id}`);
  }

  crea(data: CreaRuoloRequest): Observable<Ruolo> {
    return this.http.post<Ruolo>(this.urlBase, data);
  }

  aggiorna(id: number, data: AggiornaRuoloRequest): Observable<Ruolo> {
    return this.http.put<Ruolo>(`${this.urlBase}/${id}`, data);
  }

  elimina(id: number): Observable<void> {
    return this.http.delete<void>(`${this.urlBase}/${id}`);
  }

  ottieniFunzioniRuolo(id: number): Observable<RuoloFunzioneNodo[]> {
    return this.http.get<RuoloFunzioneNodo[]>(`${this.urlBase}/${id}/funzioni`);
  }

  aggiornaFunzioniRuolo(id: number, funzioneIds: number[]): Observable<void> {
    return this.http.put<void>(`${this.urlBase}/${id}/funzioni`, { funzioneIds });
  }
}
