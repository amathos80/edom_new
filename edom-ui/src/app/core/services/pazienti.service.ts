import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Paziente, PazienteSearchRequest } from '../models/paziente.model';

@Injectable({ providedIn: 'root' })
export class PazientiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/pazienti';

  search(request: PazienteSearchRequest): Observable<Paziente[]> {
    let params = new HttpParams();
    if (request.cognome) params = params.set('cognome', request.cognome);
    if (request.codiceFiscale) params = params.set('codiceFiscale', request.codiceFiscale);
    if (request.attivo !== undefined) params = params.set('attivo', String(request.attivo));
    return this.http.get<Paziente[]>(this.baseUrl, { params });
  }

  getById(id: number): Observable<Paziente> {
    return this.http.get<Paziente>(`${this.baseUrl}/${id}`);
  }

  create(data: Partial<Paziente>): Observable<Paziente> {
    return this.http.post<Paziente>(this.baseUrl, data);
  }

  update(id: number, data: Partial<Paziente>): Observable<Paziente> {
    return this.http.put<Paziente>(`${this.baseUrl}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
