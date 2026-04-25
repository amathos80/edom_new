import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, JwtPayload } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly TOKEN_KEY = 'access_token';

  private _user = signal<JwtPayload | null>(this.parseStoredToken());

  readonly isAuthenticated = computed(() => {
    const user = this._user();
    if (!user) return false;
    return user.exp * 1000 > Date.now();
  });

  readonly currentUser = this._user.asReadonly();

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, credentials).pipe(
      tap(res => {
        localStorage.setItem(this.TOKEN_KEY, res.token);
        this._user.set(this.decodeToken(res.token));
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this._user.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  private parseStoredToken(): JwtPayload | null {
    const token = localStorage.getItem(this.TOKEN_KEY);
    return token ? this.decodeToken(token) : null;
  }

  private decodeToken(token: string): JwtPayload | null {
    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload)) as JwtPayload;
    } catch {
      return null;
    }
  }
}
