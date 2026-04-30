/**
 * Modello per i parametri di ricerca paginata con filtri
 */
export type OperatoreFiltro =
  | 'eq'
  | 'ne'
  | 'contains'
  | 'startsWith'
  | 'endsWith'
  | 'gt'
  | 'gte'
  | 'lt'
  | 'lte'
  | 'between'
  | 'in';

export interface CondizioneFiltro {
  field: string;
  op: OperatoreFiltro;
  value?: unknown;
  values?: unknown[];
  type?: 'string' | 'number' | 'boolean' | 'date';
}

export interface GruppoFiltri {
  logic: 'and' | 'or';
  conditions: CondizioneFiltro[];
  groups?: GruppoFiltri[];
}

export interface ParametriRicercaPaginata {
  skip: number;
  take: number;
  sort?: string;
  filter?: GruppoFiltri;
}

/**
 * Modello per la risposta paginata generica
 */
export interface RispostaPaginata<T> {
  items: T[];
  totale: number;
  skip: number;
  take: number;
}

/**
 * Tipo per event lazy loading di PrimeNG
 */
export interface LazyLoadEvent {
  first?: number;
  rows?: number;
  sortField?: string | string[];
  sortOrder?: number; // 1 = asc, -1 = desc
  filters?: Record<string, any>;
  globalFilter?: string | string[];
}
