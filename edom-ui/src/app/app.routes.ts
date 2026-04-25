import { Routes } from '@angular/router';
import { authGuard } from './core/interceptors/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'app/home', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'app',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell/shell.component').then(m => m.ShellComponent),
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      {
        path: 'home',
        loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent)
      },
      {
        path: 'pazienti',
        loadComponent: () => import('./features/pazienti/pazienti-list/pazienti-list.component').then(m => m.PazientiListComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'app/home' }
];
