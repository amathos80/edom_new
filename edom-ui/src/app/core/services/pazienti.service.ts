import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Paziente, PazientePuaData, PazienteSearchRequest } from '../models/paziente.model';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/commons.model';

@Injectable({ providedIn: 'root' })
export class PazientiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/pazienti`;

  search(request: PazienteSearchRequest): Observable<PagedResult<Paziente>> {
    let params = new HttpParams();
    if (request.cognome) params = params.set('cognome', request.cognome);
    if (request.nome) params = params.set('nome', request.nome);
    if (request.codiceFiscale) params = params.set('codiceFiscale', request.codiceFiscale);
    if (request.dataNascita) params = params.set('dataNascita', request.dataNascita);
    if (request.attivo !== undefined) params = params.set('attivo', String(request.attivo));
    params = params.set('page', String(request.page ?? 1));
    params = params.set('pageSize', String(request.pageSize ?? 20));
    return this.http.get<PagedResult<Paziente>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<Paziente> {
    return this.http.get<Paziente>(`${this.baseUrl}/${id}`);
  }

  getPuaData(id: number): Observable<PazientePuaData> {
    return this.http.get<PazientePuaData>(`${this.baseUrl}/${id}/pua-data`);
  }

  create(data: Partial<Paziente>): Observable<Paziente> {
    return this.http.post<Paziente>(this.baseUrl, data);
  }

  createFromAssistito(assistitoId: number): Observable<Paziente> {
    return this.http.post<Paziente>(`${this.baseUrl}/from-assistito/${assistitoId}`, {});
  }

  update(id: number, data: Partial<Paziente>): Observable<Paziente> {
    return this.http.put<Paziente>(`${this.baseUrl}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
