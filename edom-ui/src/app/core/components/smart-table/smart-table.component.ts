import {
  AfterContentInit,
  Component,
  ContentChild,
  ContentChildren,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  Output,
  QueryList,
  SimpleChanges,
  computed,
  signal
} from '@angular/core';
import { NgTemplateOutlet, formatDate } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { Table, TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';

import { SmartTableColumn } from './smart-table.model';
import { SmartTableActionsTemplateDirective, SmartTableCellTemplateDirective } from './smart-table.templates';

@Component({
  selector: 'app-smart-table',
  standalone: true,
  imports: [
    NgTemplateOutlet,
    FormsModule,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
    MultiSelectModule,
    TableModule,
    TooltipModule
  ],
  templateUrl: './smart-table.component.html',
  styleUrl: './smart-table.component.scss'
})
export class SmartTableComponent implements OnChanges, AfterContentInit, OnDestroy {
  @Input() value: unknown[] = [];
  @Input() loading = false;
  @Input() columns: SmartTableColumn[] = [];
  @Input() dataKey?: string;

  @Input() rows = 15;
  @Input() rowsPerPageOptions: number[] = [15, 30, 50];

  @Input() emptyMessage = 'Nessun dato disponibile.';

  @Input() showColumnSelector = true;
  @Input() columnSelectorLabel = 'Colonne';
  @Input() requiredColumnKeys: string[] = [];
  @Input() defaultVisibleColumnKeys: string[] = [];
  @Input() visibleColumnKeys: string[] | null = null;
  @Output() readonly visibleColumnKeysChange = new EventEmitter<string[]>();

  @Input() showGlobalSearch = false;
  @Input() globalSearchPlaceholder = 'Cerca';
  @Input() globalFilterFields: string[] = [];

  @Input() showActionsColumn = true;
  @Input() actionsHeader = 'Azioni';
  @Input() actionsMinWidthRem = 8;
  @Input() defaultColumnMinWidthRem = 10;
  @Input() enableRowDoubleClick = false;
  @Output() readonly rowDoubleClick = new EventEmitter<unknown>();

  @ContentChildren(SmartTableCellTemplateDirective)
  private cellTemplates?: QueryList<SmartTableCellTemplateDirective>;

  @ContentChild(SmartTableActionsTemplateDirective)
  actionsTemplate?: SmartTableActionsTemplateDirective;

  readonly internalVisibleKeys = signal<string[]>([]);
  private readonly columnTemplateMap = new Map<string, SmartTableCellTemplateDirective>();
  private templatesSub?: { unsubscribe: () => void };

  readonly effectiveVisibleColumns = computed(() => {
    const keys = this.internalVisibleKeys();
    return this.columns.filter((col) => keys.includes(col.key));
  });

  readonly selectorOptions = computed(() =>
    this.columns.map((col) => ({ label: col.header, value: col.key }))
  );

  readonly tableStyle = computed(() => {
    const columnsWidthRem = this.effectiveVisibleColumns()
      .reduce((sum, col) => sum + (col.minWidthRem ?? this.defaultColumnMinWidthRem), 0);

    const actionsWidthRem = this.shouldRenderActionsColumn() ? this.actionsMinWidthRem : 0;
    return { 'min-width': `${columnsWidthRem + actionsWidthRem}rem` };
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['columns'] || changes['visibleColumnKeys'] || changes['defaultVisibleColumnKeys'] || changes['requiredColumnKeys']) {
      this.syncVisibleColumns();
    }
  }

  ngAfterContentInit(): void {
    this.rebuildTemplateMap();
    this.templatesSub = this.cellTemplates?.changes.subscribe(() => this.rebuildTemplateMap());
  }

  ngOnDestroy(): void {
    this.templatesSub?.unsubscribe();
  }

  updateVisibleColumns(keys: string[] | null | undefined): void {
    const normalized = this.normalizeVisibleColumns(keys ?? []);
    this.internalVisibleKeys.set(normalized);
    this.visibleColumnKeysChange.emit(normalized);
  }

  getCellTemplate(key: string): SmartTableCellTemplateDirective | undefined {
    return this.columnTemplateMap.get(key);
  }

  shouldRenderActionsColumn(): boolean {
    return this.showActionsColumn && !!this.actionsTemplate;
  }

  emptyStateColspan(): number {
    return this.effectiveVisibleColumns().length + (this.shouldRenderActionsColumn() ? 1 : 0);
  }

  formatCellValue(row: unknown, column: SmartTableColumn): string {
    const fallback = column.emptyValue ?? '-';
    const rawValue = this.readNestedValue(row, column.field);

    if (rawValue === null || rawValue === undefined || rawValue === '') {
      return fallback;
    }

    if (column.filterType === 'date') {
      const date = rawValue instanceof Date ? rawValue : new Date(rawValue as string);
      if (isNaN(date.getTime())) {
        return fallback;
      }

      return formatDate(date, column.dateFormat ?? 'dd/MM/yyyy', 'it-IT');
    }

    return String(rawValue);
  }

  filterGlobale(tabella: Table, event: Event): void {
    const valore = (event.target as HTMLInputElement).value;
    tabella.filterGlobal(valore, 'contains');
  }

  onRowDoubleClick(row: unknown): void {
    if (!this.enableRowDoubleClick) {
      return;
    }

    this.rowDoubleClick.emit(row);
  }

  private syncVisibleColumns(): void {
    if (!this.columns.length) {
      this.internalVisibleKeys.set([]);
      return;
    }

    const current = this.internalVisibleKeys();
    const next = this.visibleColumnKeys ?? (current.length ? current : this.defaultVisibleColumnKeys);
    this.internalVisibleKeys.set(this.normalizeVisibleColumns(next));
  }

  private normalizeVisibleColumns(keys: string[]): string[] {
    const available = this.columns.map((col) => col.key);
    const selected = available.filter((key) => keys.includes(key));

    const fallback = selected.length ? selected : available;

    for (const required of this.requiredColumnKeys) {
      if (available.includes(required) && !fallback.includes(required)) {
        fallback.push(required);
      }
    }

    return fallback;
  }

  private rebuildTemplateMap(): void {
    this.columnTemplateMap.clear();
    for (const tpl of this.cellTemplates ?? []) {
      if (tpl.key) {
        this.columnTemplateMap.set(tpl.key, tpl);
      }
    }
  }

  private readNestedValue(row: unknown, path: string): unknown {
    if (!row || !path) {
      return null;
    }

    const parts = path.split('.');
    let current: any = row;

    for (const part of parts) {
      current = current?.[part];
      if (current === undefined || current === null) {
        break;
      }
    }

    return current;
  }
}
