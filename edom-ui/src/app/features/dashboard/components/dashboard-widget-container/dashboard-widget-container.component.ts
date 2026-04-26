import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, Type, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { DashboardWidgetComponentInputs, DashboardWidgetInstance } from '../../models/dashboard.models';
import { DashboardWidgetsRegistryService } from '../../registry/dashboard-widgets.registry';

@Component({
  selector: 'app-dashboard-widget-container',
  standalone: true,
  imports: [CommonModule, ButtonModule, ProgressSpinnerModule, TagModule, TooltipModule],
  templateUrl: './dashboard-widget-container.component.html',
  styleUrl: './dashboard-widget-container.component.scss'
})
export class DashboardWidgetContainerComponent implements OnChanges {
  @Input({ required: true }) widget!: DashboardWidgetInstance;
  @Input() depth = 0;
  @Input() maxDepth = 2;
  @Output() readonly removeRequested = new EventEmitter<string>();

  private readonly registry = inject(DashboardWidgetsRegistryService);

  componentType = signal<Type<unknown> | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  get canRenderChildren(): boolean {
    return this.depth < this.maxDepth;
  }

  get componentInputs(): DashboardWidgetComponentInputs {
    return {
      config: this.widget.config ?? {},
      datasource: this.widget.datasource ?? {},
      runtime: { loading: false, error: null, empty: false },
      depth: this.depth
    };
  }

  ngOnChanges(): void {
    this.loadComponent();
  }

  requestRemove(): void {
    this.removeRequested.emit(this.widget.id);
  }

  private async loadComponent(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const component = await this.registry.resolveComponent(this.widget.type);
      this.componentType.set(component);
    } catch {
      this.componentType.set(null);
      this.error.set(`Widget non disponibile: ${this.widget.type}`);
    } finally {
      this.loading.set(false);
    }
  }
}
