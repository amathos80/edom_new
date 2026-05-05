export type SmartTableFilterType = 'text' | 'numeric' | 'date' | 'boolean';

export interface SmartTableColumn {
  key: string;
  field: string;
  header: string;
  fullHeader?: string;
  sortable?: boolean;
  filterType?: SmartTableFilterType;
  minWidthRem?: number;
  emptyValue?: string;
  dateFormat?: string;
}
