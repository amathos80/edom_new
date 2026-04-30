import { Injectable } from '@angular/core';
import { TableLazyLoadEvent } from 'primeng/table';
import { CondizioneFiltro, GruppoFiltri, ParametriRicercaPaginata } from '../models/pagination.model';

@Injectable({ providedIn: 'root' })
export class LazyLoadProvider {
  private readonly defaultGlobalFields = ['codice', 'descrizione', 'proceduraId'];

  estraiParametri(
    event: TableLazyLoadEvent,
    filtriAggiuntivi?: CondizioneFiltro[],
    globalFields?: string[]
  ): ParametriRicercaPaginata {
    const skip = event.first ?? 0;
    const take = event.rows ?? 10;

    const sortField = (Array.isArray(event.sortField) ? event.sortField[0] : event.sortField) ?? undefined;
    const sort = this.normalizzaSort(sortField, event.sortOrder);
    const filter = this.costruisciFiltro(event, filtriAggiuntivi, globalFields ?? this.defaultGlobalFields);

    return {
      skip,
      take,
      sort,
      filter
    };
  }

  normalizzaParametri(params: ParametriRicercaPaginata): ParametriRicercaPaginata {
    const filter = params.filter && (params.filter.conditions.length > 0 || (params.filter.groups?.length ?? 0) > 0)
      ? params.filter
      : undefined;

    return {
      skip: params.skip,
      take: params.take,
      sort: params.sort || undefined,
      filter
    };
  }

  private normalizzaSort(sortField: string | undefined, sortOrder: number | null | undefined): string | undefined {
    if (!sortField || !sortOrder || sortOrder === 0) {
      return undefined;
    }

    return sortOrder < 0 ? `-${sortField}` : sortField;
  }

  private costruisciFiltro(
    event: TableLazyLoadEvent,
    filtriAggiuntivi: CondizioneFiltro[] | undefined,
    globalFields: string[]
  ): GruppoFiltri | undefined {
    const conditions: CondizioneFiltro[] = [];

    const tableFilters = event.filters ?? {};
    for (const [field, value] of Object.entries(tableFilters)) {
      if (field === 'global') {
        continue;
      }

      const parsed = this.estraiCondizioniCampo(field, value);
      conditions.push(...parsed);
    }

    if (filtriAggiuntivi?.length) {
      conditions.push(...filtriAggiuntivi);
    }

    const globalMetaValue = this.estraiValoreFiltroGlobale(tableFilters['global']);
    const globalValueRaw = Array.isArray(event.globalFilter)
      ? event.globalFilter[0]
      : event.globalFilter ?? globalMetaValue;
    const globalValue = this.normalizzaValore(globalValueRaw);
    let groups: GruppoFiltri[] | undefined;

    if (typeof globalValue === 'string' && globalValue.trim().length > 0 && globalFields.length > 0) {
      groups = [{
        logic: 'or',
        conditions: globalFields.map(field => ({
          field,
          op: 'contains',
          value: globalValue,
          type: 'string'
        }))
      }];
    }

    const root: GruppoFiltri = {
      logic: 'and',
      conditions
    };

    if (groups?.length) {
      root.groups = groups;
    }

    if (root.conditions.length === 0 && !root.groups?.length) {
      return undefined;
    }

    return root;
  }

  private estraiCondizioniCampo(field: string, raw: unknown): CondizioneFiltro[] {
    if (!raw || typeof raw !== 'object') {
      return [];
    }

    const constraintsRaw = this.estraiConstraints(raw);

    return constraintsRaw
      .map(c => this.mappaCondizione(field, c))
      .filter((c): c is CondizioneFiltro => c !== null);
  }

  private estraiConstraints(raw: unknown): unknown[] {
    if (Array.isArray(raw)) {
      return raw;
    }

    const meta = raw as Record<string, unknown>;
    if (Array.isArray(meta['constraints'])) {
      return meta['constraints'] as unknown[];
    }

    return [meta];
  }

  private estraiValoreFiltroGlobale(raw: unknown): unknown {
    if (!raw) {
      return undefined;
    }

    if (Array.isArray(raw)) {
      const first = raw[0] as { value?: unknown } | undefined;
      return first?.value;
    }

    if (typeof raw === 'object') {
      return (raw as { value?: unknown }).value;
    }

    return undefined;
  }

  private mappaCondizione(field: string, raw: unknown): CondizioneFiltro | null {
    if (!raw || typeof raw !== 'object') {
      return null;
    }

    const meta = raw as Record<string, unknown>;
    const matchMode = String(meta['matchMode'] ?? 'contains');
    const normalizedOp = this.mappaOperatore(matchMode);

    if (!normalizedOp) {
      return null;
    }

    const value = this.normalizzaValore(meta['value']);
    if (value === undefined || value === null || value === '') {
      return null;
    }

    if (normalizedOp === 'between' && Array.isArray(value) && value.length === 2) {
      return {
        field,
        op: 'between',
        values: value,
        type: this.inferType(value[0])
      };
    }

    if (normalizedOp === 'in' && Array.isArray(value)) {
      return {
        field,
        op: 'in',
        values: value,
        type: this.inferType(value[0])
      };
    }

    return {
      field,
      op: normalizedOp,
      value,
      type: this.inferType(value)
    };
  }

  private mappaOperatore(matchMode: string): CondizioneFiltro['op'] | null {
    const mode = matchMode.toLowerCase();
    switch (mode) {
      case 'equals':
      case 'dateis':
        return 'eq';
      case 'notequals':
      case 'dateisnot':
        return 'ne';
      case 'datebefore':
        return 'lt';
      case 'dateafter':
        return 'gt';
      case 'startswith':
        return 'startsWith';
      case 'endswith':
        return 'endsWith';
      case 'contains':
      case 'lt':
      case 'lte':
      case 'gt':
      case 'gte':
      case 'in':
      case 'between':
        return mode as CondizioneFiltro['op'];
      default:
        return null;
    }
  }

  private normalizzaValore(value: unknown): unknown {
    if (Array.isArray(value)) {
      return value.map(v => this.normalizzaValore(v));
    }

    if (value instanceof Date) {
      return value.toISOString();
    }

    return value;
  }

  private inferType(value: unknown): CondizioneFiltro['type'] {
    if (typeof value === 'boolean') {
      return 'boolean';
    }

    if (typeof value === 'number') {
      return 'number';
    }

    if (typeof value === 'string') {
      const maybeDate = Date.parse(value);
      if (!Number.isNaN(maybeDate) && (value.includes('T') || value.includes('-'))) {
        return 'date';
      }

      return 'string';
    }

    return 'string';
  }
}
