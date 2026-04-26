import { Component, computed, inject, signal } from '@angular/core';
import { take } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DashboardGridComponent } from '../../components/dashboard-grid/dashboard-grid.component';
import { DashboardWidgetInstance } from '../../models/dashboard.models';
import { DashboardWidgetsRegistryService } from '../../registry/dashboard-widgets.registry';
import { DashboardLayoutService } from '../../services/dashboard-layout.service';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [ButtonModule, CardModule, TagModule, DashboardGridComponent],
  templateUrl: './dashboard-home.component.html',
  styleUrl: './dashboard-home.component.scss'
})
export class DashboardHomeComponent {
  private readonly registry = inject(DashboardWidgetsRegistryService);
  private readonly layoutService = inject(DashboardLayoutService);

  readonly catalog = this.registry.getAll();
  readonly widgets = this.layoutService.activeWidgets;
  readonly loading = this.layoutService.loading;
  readonly saving = this.layoutService.saving;

  readonly fallbackEnabled = signal(false);
  readonly layoutVersion = computed(() => this.layoutService.layout().schemaVersion);

  constructor() {
    this.layoutService
      .loadLayout()
      .pipe(take(1))
      .subscribe();
  }

  addWidget(type: string): void {
    this.layoutService.addWidget(type);
  }

  removeWidget(id: string): void {
    this.layoutService.removeWidget(id);
  }

  onWidgetsChange(widgets: DashboardWidgetInstance[]): void {
    this.layoutService.updateWidgets(widgets);
  }

  onFallbackMode(enabled: boolean): void {
    this.fallbackEnabled.set(enabled);
  }

  resetLayout(): void {
    this.layoutService.restoreDefaults();
  }
}
