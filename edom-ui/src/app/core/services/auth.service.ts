import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { JwtPayload, LoginRequest, LoginResponse, RefreshTokenRequest, RispostaPermessi } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly CHIAVE_TOKEN = 'access_token';
  private readonly CHIAVE_REFRESH_TOKEN = 'refresh_token';
  private readonly CHIAVE_PERMESSI = 'auth_permissions_v1';
  private readonly CHIAVE_LOOKUP_PROCEDURE = 'lookup_procedure_v1';
  private readonly CHIAVE_LOOKUP_MESSAGGI = 'lookup_messaggi_v1';
  private readonly CHIAVE_LOOKUP_VERSIONE = 'lookup_version_v1';

  private _utente = signal<JwtPayload | null>(this.leggiTokenSalvato());
  private _permessi = signal<RispostaPermessi | null>(this.leggiPermessiSalvati());

  readonly isAuthenticated = computed(() => {
    const utente = this._utente();
    if (!utente) return false;
    return utente.exp * 1000 > Date.now();
  });

  readonly utenteCorrente = this._utente.asReadonly();
  readonly permessiCorrenti = this._permessi.asReadonly();

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, credentials).pipe(
      tap(res => {
        this.salvaSessione(res);
      }),
      tap(() => {
        this.aggiornaPermessi().subscribe({ error: () => this.svuotaPermessi() });
      })
    );
  }

  assicuraPermessiCaricati(): Observable<RispostaPermessi | null> {
    if (!this.isAuthenticated()) {
      this.svuotaPermessi();
      return of(null);
    }

    const cache = this._permessi();
    if (cache) return of(cache);

    return this.aggiornaPermessi();
  }

  aggiornaPermessi(): Observable<RispostaPermessi> {
    return this.http.get<RispostaPermessi>(`${environment.apiUrl}/auth/permessi`).pipe(
      tap(permessi => {
        localStorage.setItem(this.CHIAVE_PERMESSI, JSON.stringify(permessi));
        this._permessi.set(permessi);
      })
    );
  }

  logout(): void {
    const token = this.getToken();
    if (token) {
      this.http.post(`${environment.apiUrl}/auth/logout`, {}).subscribe({ error: () => {} });
    }

    localStorage.removeItem(this.CHIAVE_TOKEN);
    localStorage.removeItem(this.CHIAVE_REFRESH_TOKEN);
    this._utente.set(null);
    this.svuotaPermessi();
    this.svuotaLookupCache();
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.CHIAVE_TOKEN);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.CHIAVE_REFRESH_TOKEN);
  }

  refreshSession(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('Refresh token non disponibile.');
    }

    const payload: RefreshTokenRequest = { refreshToken };
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/refresh`, payload).pipe(
      tap(res => this.salvaSessione(res))
    );
  }

  private leggiTokenSalvato(): JwtPayload | null {
    const token = localStorage.getItem(this.CHIAVE_TOKEN);
    return token ? this.decodificaToken(token) : null;
  }

  private leggiPermessiSalvati(): RispostaPermessi | null {
    const raw = localStorage.getItem(this.CHIAVE_PERMESSI);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as RispostaPermessi;
    } catch {
      return null;
    }
  }

  private svuotaPermessi(): void {
    localStorage.removeItem(this.CHIAVE_PERMESSI);
    this._permessi.set(null);
  }

  private svuotaLookupCache(): void {
    localStorage.removeItem(this.CHIAVE_LOOKUP_PROCEDURE);
    localStorage.removeItem(this.CHIAVE_LOOKUP_MESSAGGI);
    localStorage.removeItem(this.CHIAVE_LOOKUP_VERSIONE);
  }

  private salvaSessione(res: LoginResponse): void {
    localStorage.setItem(this.CHIAVE_TOKEN, res.token);
    localStorage.setItem(this.CHIAVE_REFRESH_TOKEN, res.refreshToken);
    this._utente.set(this.decodificaToken(res.token));
  }

  private decodificaToken(token: string): JwtPayload | null {
    try {
      const payload = token.split('.')[1];
      if (!payload) return null;

      // JWT usa base64url: converte in base64 standard prima della decodifica.
      const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
      const padding = '='.repeat((4 - (base64.length % 4)) % 4);
      return JSON.parse(atob(base64 + padding)) as JwtPayload;
    } catch {
      return null;
    }
  }
}
