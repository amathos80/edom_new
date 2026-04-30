import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AggiornaUtenteRequest, CreaUtenteRequest, RicercaUtenteRequest, Utente } from '../models/utente.model';

@Injectable({ providedIn: 'root' })
export class UtentiService {
  private readonly http = inject(HttpClient);
  private readonly urlBase = `${environment.apiUrl}/utenti`;

  cerca(request: RicercaUtenteRequest): Observable<Utente[]> {
    let params = new HttpParams();
    if (request.codice) params = params.set('codice', request.codice);
    if (request.cognome) params = params.set('cognome', request.cognome);
    if (request.nome) params = params.set('nome', request.nome);
    if (request.soloAttivi !== undefined) params = params.set('soloAttivi', String(request.soloAttivi));
    return this.http.get<Utente[]>(this.urlBase, { params });
  }

  crea(data: CreaUtenteRequest): Observable<Utente> {
    return this.http.post<Utente>(this.urlBase, data);
  }

  aggiorna(id: number, data: AggiornaUtenteRequest): Observable<Utente> {
    return this.http.put<Utente>(`${this.urlBase}/${id}`, data);
  }

  elimina(id: number): Observable<void> {
    return this.http.delete<void>(`${this.urlBase}/${id}`);
  }

  resetPassword(id: number): Observable<void> {
    return this.http.post<void>(`${this.urlBase}/${id}/reset-password`, {});
  }

  riattiva(id: number): Observable<void> {
    return this.http.post<void>(`${this.urlBase}/${id}/riattiva`, {});
  }
}
