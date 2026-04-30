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
      },
      {
        path: 'ruoli',
        loadComponent: () => import('./features/ruoli/ruoli-management/ruoli-management.component').then(m => m.GestioneRuoliComponent)
      },
      {
        path: 'ruoli-funzioni',
        loadComponent: () => import('./features/ruoli/ruoli-funzioni-management/ruoli-funzioni-management.component').then(m => m.RuoliFunzioniManagementComponent)
      },
      {
        path: 'utenti',
        loadComponent: () => import('./features/utenti/utenti-management/utenti-management.component').then(m => m.UtentiManagementComponent)
      },
      {
        path: 'custom-components',
        loadChildren: () => import('./features/custom-components/custom-components.routes').then(m => m.CUSTOM_COMPONENTS_ROUTES)
      },
      {
        path: 'dashboard',
        loadChildren: () => import('./features/dashboard/dashboard.routes').then(m => m.DASHBOARD_ROUTES)
      }
    ]
  },
  { path: '**', redirectTo: 'app/home' }
];
