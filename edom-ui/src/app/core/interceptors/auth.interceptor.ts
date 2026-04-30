import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, finalize, map, Observable, shareReplay, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

let refreshInFlight$: Observable<string> | null = null;

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  const isAuthEndpoint =
    req.url.includes('/auth/login') ||
    req.url.includes('/auth/refresh') ||
    req.url.includes('/auth/logout') ||
    req.url.includes('/auth/logout-all');

  const withAuth = (token: string | null) => {
    if (!token) {
      return req;
    }

    return req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  };

  return next(withAuth(auth.getToken())).pipe(
    catchError((error: unknown) => {
      const httpError = error as HttpErrorResponse;
      if (httpError.status !== 401 || isAuthEndpoint || !auth.getRefreshToken()) {
        return throwError(() => error);
      }

      if (!refreshInFlight$) {
        refreshInFlight$ = auth.refreshSession().pipe(
          map(res => res.token),
          shareReplay(1),
          finalize(() => {
            refreshInFlight$ = null;
          })
        );
      }

      return refreshInFlight$.pipe(
        switchMap((token) => next(withAuth(token))),
        catchError((refreshError) => {
          auth.logout();
          return throwError(() => refreshError);
        })
      );
    })
  );
};
