import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Assistito, AssistitoSearchRequest } from '../models/assistito.model';
import { PagedResult } from '../models/commons.model';
import { Paziente } from '../models/paziente.model';

@Injectable({ providedIn: 'root' })
export class AssistitiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/assistiti`;

  search(request: AssistitoSearchRequest): Observable<PagedResult<Assistito>> {
    let params = new HttpParams();
    if (request.cognome) params = params.set('cognome', request.cognome);
    if (request.nome) params = params.set('nome', request.nome);
    if (request.codiceFiscale) params = params.set('codiceFiscale', request.codiceFiscale);
    if (request.dataNascita) params = params.set('dataNascita', request.dataNascita);
    if (request.attivo !== undefined) params = params.set('attivo', String(request.attivo));
    params = params.set('page', String(request.page ?? 1));
    params = params.set('pageSize', String(request.pageSize ?? 20));
    return this.http.get<PagedResult<Assistito>>(this.baseUrl, { params });
  }

  getById(id: number): Observable<Assistito> {
    return this.http.get<Assistito>(`${this.baseUrl}/${id}`);
  }

  create(data: Partial<Assistito>): Observable<Assistito> {
    return this.http.post<Assistito>(this.baseUrl, data);
  }

  update(id: number, data: Partial<Assistito>): Observable<Assistito> {
    return this.http.put<Assistito>(`${this.baseUrl}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
