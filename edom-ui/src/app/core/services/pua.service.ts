import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  AreaDto,
  CreatePuaRequest,
  DuplicatePuaRequest,
  NumeroPuaDto,
  Pua,
  PuaSearchRequest,
  UpdatePuaRequest
} from '../models/pua.model';

@Injectable({ providedIn: 'root' })
export class PuaService {
  private readonly http = inject(HttpClient);
  private readonly urlBase = `${environment.apiUrl}/pua`;

  search(request: PuaSearchRequest): Observable<Pua[]> {
    let params = new HttpParams();

    if (request.pazienteId != null) {
      params = params.set('pazienteId', String(request.pazienteId));
    }

    if (request.numeroPuaId != null) {
      params = params.set('numeroPuaId', String(request.numeroPuaId));
    }

    if (request.attivo != null) {
      params = params.set('attivo', String(request.attivo));
    }

    if (request.dataDa) {
      params = params.set('dataDa', request.dataDa);
    }

    if (request.dataA) {
      params = params.set('dataA', request.dataA);
    }

    if (request.take != null) {
      params = params.set('take', String(request.take));
    }

    return this.http.get<Pua[]>(this.urlBase, { params });
  }

  getById(id: number): Observable<Pua> {
    return this.http.get<Pua>(`${this.urlBase}/${id}`);
  }

  create(request: CreatePuaRequest): Observable<Pua> {
    return this.http.post<Pua>(this.urlBase, request);
  }

  update(id: number, request: UpdatePuaRequest): Observable<Pua> {
    return this.http.put<Pua>(`${this.urlBase}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.urlBase}/${id}`);
  }

  duplicate(id: number, request: DuplicatePuaRequest = {}): Observable<Pua> {
    return this.http.post<Pua>(`${this.urlBase}/${id}/duplica`, request);
  }

  getNumeriPua(): Observable<NumeroPuaDto[]> {
    return this.http.get<NumeroPuaDto[]>(`${this.urlBase}/numeri-pua`);
  }

  getAree(): Observable<AreaDto[]> {
    return this.http.get<AreaDto[]>(`${this.urlBase}/aree`);
  }
}
