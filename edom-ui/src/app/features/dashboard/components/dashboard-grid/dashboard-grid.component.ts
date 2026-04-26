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

  @ViewChild('gridHost', { static: true })
  private readonly gridHost?: ElementRef<HTMLDivElement>;

  gridFailed = false;
  private grid?: GridStack;

  ngAfterViewInit(): void {
    this.initGrid();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['widgets'] && this.grid) {
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
          minRow: 1
        },
        this.gridHost.nativeElement
      );

      this.grid.on('change', () => this.emitLayout());
      this.syncWidgetsWithGrid();
      this.fallbackMode.emit(false);
    } catch {
      this.gridFailed = true;
      this.fallbackMode.emit(true);
    }
  }

  private syncWidgetsWithGrid(): void {
    if (!this.grid || this.gridFailed) {
      return;
    }

    this.grid.removeAll(false);

    for (const element of this.gridHost?.nativeElement.querySelectorAll('.grid-stack-item') ?? []) {
      this.grid.makeWidget(element as HTMLElement);
    }

    this.emitLayout();
  }

  private emitLayout(): void {
    if (!this.grid || this.gridFailed) {
      return;
    }

    const nodes: GridStackNode[] = this.grid.engine.nodes;
    const nodesById = new Map(nodes.map((node: GridStackNode) => [String(node.id), node]));

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
