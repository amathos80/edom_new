import { Routes } from '@angular/router';
import { CUSTOM_COMPONENT_ENTRIES } from './custom-components.registry';

export const CUSTOM_COMPONENTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/custom-components-home/custom-components-home.component').then(
        m => m.CustomComponentsHomeComponent
      )
  },
  ...CUSTOM_COMPONENT_ENTRIES.map(entry => ({
    path: entry.path,
    loadComponent: entry.loadComponent,
    data: {
      title: entry.title,
      description: entry.description
    }
  })),
  {
    path: '**',
    redirectTo: ''
  }
];
