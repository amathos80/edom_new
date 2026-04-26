import { Type } from '@angular/core';

export type CustomComponentLoader = () => Promise<Type<unknown>>;

export interface CustomComponentEntry {
  path: string;
  title: string;
  description: string;
  loadComponent: CustomComponentLoader;
}

// Each entry below is split into its own lazy chunk via dynamic import.
export const CUSTOM_COMPONENT_ENTRIES: CustomComponentEntry[] = [
  {
    path: 'badge',
    title: 'Custom Badge',
    description: 'Badge riutilizzabile con varianti di stato e dimensioni.',
    loadComponent: () =>
      import('./examples/custom-badge/custom-badge.component').then(
        m => m.CustomBadgeComponent
      )
  },
  {
    path: 'empty-state',
    title: 'Custom Empty State',
    description: 'Blocco vuoto con call-to-action e stili pronti per dashboard.',
    loadComponent: () =>
      import('./examples/custom-empty-state/custom-empty-state.component').then(
        m => m.CustomEmptyStateComponent
      )
  }
];
