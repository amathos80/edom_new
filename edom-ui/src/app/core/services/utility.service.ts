import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay } from 'rxjs';

import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UtilityService {
  private readonly http = inject(HttpClient);
  private readonly urlBase = `${environment.apiUrl}/utility`;

  private readonly _dataCorrente$ = this.http
    .get<string>(`${this.urlBase}/data-corrente`, { responseType: 'json' })
    .pipe(shareReplay(1));

  /** Returns the current server date as ISO string (yyyy-MM-dd). Cached for the lifetime of the service. */
  getDataCorrente(): Observable<string> {
    return this._dataCorrente$;
  }
}
