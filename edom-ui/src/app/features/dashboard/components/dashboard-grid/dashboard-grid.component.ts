import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  Output,
  SimpleChanges,
  ViewChild
} from '@angular/core';
import { GridStack, GridStackNode } from 'gridstack';
import 'gridstack/dist/dd-gridstack';
import { DashboardWidgetInstance } from '../../models/dashboard.models';
import { DashboardWidgetContainerComponent } from '../dashboard-widget-container/dashboard-widget-container.component';

@Component({
  selector: 'app-dashboard-grid',
  standalone: true,
  imports: [DashboardWidgetContainerComponent],
  templateUrl: './dashboard-grid.component.html',
  styleUrl: './dashboard-grid.component.scss'
})
export class DashboardGridComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input({ required: true }) widgets: DashboardWidgetInstance[] = [];
  @Output() readonly widgetsChange = new EventEmitter<DashboardWidgetInstance[]>();
  @Output() readonly removeWidget = new EventEmitter<string>();
  @Output() readonly fallbackMode = new EventEmitter<boolean>();

  @ViewChild('gridHost', { static: false })
  private readonly gridHost?: ElementRef<HTMLDivElement>;

  gridFailed = false;
  private grid?: GridStack;

  ngAfterViewInit(): void {
    this.initGrid();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['widgets'] || !this.grid) return;

    const prev: DashboardWidgetInstance[] = changes['widgets'].previousValue ?? [];
    const curr: DashboardWidgetInstance[] = changes['widgets'].currentValue ?? [];

    // Only react to structural changes (widget added/removed).
    // Position-only changes come FROM GridStack via emitLayout — reacting to those causes an infinite loop.
    const prevIds = new Set(prev.map(w => w.id));
    const currIds = new Set(curr.map(w => w.id));
    const hasStructuralChange =
      curr.some(w => !prevIds.has(w.id)) || prev.some(w => !currIds.has(w.id));

    if (hasStructuralChange) {
      queueMicrotask(() => this.syncWidgetsWithGrid());
    }
  }

  ngOnDestroy(): void {
    this.grid?.destroy(false);
  }

  onRemoveWidget(id: string): void {
    this.removeWidget.emit(id);
  }

  private initGrid(): void {
    try {
      if (!this.gridHost?.nativeElement) {
        throw new Error('Grid host non disponibile');
      }

      this.grid = GridStack.init(
        {
          column: 12,
          cellHeight: 95,
          margin: 10,
          float: true,
          minRow: 1,
          staticGrid: false,
          alwaysShowResizeHandle: true,
          disableDrag: false,
          disableResize: false
        },
        this.gridHost.nativeElement
      );

      this.grid.on('dragstop', () => this.emitLayout());
      this.grid.on('resizestop', () => this.emitLayout());
      this.fallbackMode.emit(false);
    } catch (err) {
      console.error('GridStack init failed:', err);
      this.gridFailed = true;
      this.fallbackMode.emit(true);
    }
  }

  private syncWidgetsWithGrid(): void {
    if (!this.grid || this.gridFailed || !this.gridHost?.nativeElement) {
      return;
    }

    // Register any DOM nodes Angular just added that GridStack doesn't track yet.
    // makeWidget() is safe to call on already-tracked nodes (GridStack engine deduplicates by _id).
    const knownIds = new Set(
      this.grid.engine.nodes
        .map(n => n.id ?? n.el?.getAttribute('gs-id'))
        .filter(Boolean)
    );

    for (const el of Array.from(
      this.gridHost.nativeElement.querySelectorAll<HTMLElement>('.grid-stack-item')
    )) {
      const id = el.getAttribute('gs-id');
      if (id && !knownIds.has(id)) {
        this.grid.makeWidget(el);
      }
    }
  }

  private emitLayout(): void {
    if (!this.grid || this.gridFailed) {
      return;
    }

    const nodes: GridStackNode[] = this.grid.engine.nodes;
    const nodesById = new Map(
      nodes
        .map((node: GridStackNode) => {
          const id =
            node.id ??
            node.el?.getAttribute('gs-id') ??
            node.el?.id ??
            node.el?.getAttribute('id');
          return id ? [String(id), node] : null;
        })
        .filter((entry): entry is [string, GridStackNode] => entry !== null)
    );

    const nextWidgets = this.widgets.map(widget => {
      const node = nodesById.get(widget.id);
      if (!node) {
        return widget;
      }

      return {
        ...widget,
        x: node.x ?? widget.x,
        y: node.y ?? widget.y,
        w: node.w ?? widget.w,
        h: node.h ?? widget.h
      };
    });

    this.widgetsChange.emit(nextWidgets);
  }
}
