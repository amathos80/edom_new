import { Injectable } from '@angular/core';
import { DashboardWidgetInstance, DashboardWidgetMetadata } from '../models/dashboard.models';

const DASHBOARD_WIDGETS: DashboardWidgetMetadata[] = [
  {
    type: 'kpi-summary',
    title: 'KPI Summary',
    icon: 'pi pi-chart-bar',
    description: 'Riepilogo indicatori principali della giornata.',
    minW: 2,
    minH: 2,
    defaultW: 3,
    defaultH: 2,
    loadComponent: () =>
      import('../widgets/kpi-summary-widget/kpi-summary-widget.component').then(
        m => m.KpiSummaryWidgetComponent
      )
  },
  {
    type: 'patients-table',
    title: 'Tabella Pazienti',
    icon: 'pi pi-table',
    description: 'Elenco rapido pazienti con stato e ultimo accesso.',
    minW: 3,
    minH: 2,
    defaultW: 5,
    defaultH: 3,
    loadComponent: () =>
      import('../widgets/patients-table-widget/patients-table-widget.component').then(
        m => m.PatientsTableWidgetComponent
      )
  },
  {
    type: 'trend-chart',
    title: 'Trend Visite',
    icon: 'pi pi-chart-line',
    description: 'Andamento visite settimanali in forma compatta.',
    minW: 3,
    minH: 2,
    defaultW: 4,
    defaultH: 3,
    loadComponent: () =>
      import('../widgets/trend-chart-widget/trend-chart-widget.component').then(
        m => m.TrendChartWidgetComponent
      )
  },
  {
    type: 'alerts-list',
    title: 'Alert Clinici',
    icon: 'pi pi-bell',
    description: 'Segnalazioni prioritarie da monitorare.',
    minW: 2,
    minH: 2,
    defaultW: 3,
    defaultH: 3,
    supportsChildren: true,
    loadComponent: () =>
      import('../widgets/alerts-list-widget/alerts-list-widget.component').then(
        m => m.AlertsListWidgetComponent
      )
  },
  {
    type: 'recent-activity',
    title: 'Attivita Recenti',
    icon: 'pi pi-history',
    description: 'Timeline azioni utente e aggiornamenti sistema.',
    minW: 2,
    minH: 2,
    defaultW: 4,
    defaultH: 2,
    loadComponent: () =>
      import('../widgets/recent-activity-widget/recent-activity-widget.component').then(
        m => m.RecentActivityWidgetComponent
      )
  },
  {
    type: 'system-status',
    title: 'Stato Sistema',
    icon: 'pi pi-server',
    description: 'Disponibilita servizi e code in tempo reale.',
    minW: 2,
    minH: 2,
    defaultW: 3,
    defaultH: 2,
    supportsChildren: true,
    loadComponent: () =>
      import('../widgets/system-status-widget/system-status-widget.component').then(
        m => m.SystemStatusWidgetComponent
      )
  }
];

@Injectable({ providedIn: 'root' })
export class DashboardWidgetsRegistryService {
  private readonly widgets = DASHBOARD_WIDGETS;

  getAll(): DashboardWidgetMetadata[] {
    return this.widgets;
  }

  getByType(type: string): DashboardWidgetMetadata | undefined {
    return this.widgets.find(widget => widget.type === type);
  }

  async resolveComponent(type: string) {
    const metadata = this.getByType(type);
    if (!metadata) {
      throw new Error(`Widget non registrato: ${type}`);
    }

    return metadata.loadComponent();
  }

  createDefaultInstance(type: string, overrides?: Partial<DashboardWidgetInstance>): DashboardWidgetInstance {
    const metadata = this.getByType(type);
    if (!metadata) {
      throw new Error(`Widget non registrato: ${type}`);
    }

    return {
      id: crypto.randomUUID(),
      type,
      title: metadata.title,
      x: 0,
      y: 0,
      w: metadata.defaultW,
      h: metadata.defaultH,
      minW: metadata.minW,
      minH: metadata.minH,
      maxW: metadata.maxW,
      maxH: metadata.maxH,
      state: 'active',
      config: {},
      datasource: {},
      children: [],
      ...overrides
    };
  }
}
