import { Component, Input } from '@angular/core';
import { DashboardWidgetRuntime } from '../../models/dashboard.models';

@Component({
  selector: 'app-alerts-list-widget',
  standalone: true,
  template: `
    <ul class="alerts-list">
      @for (alert of alerts; track alert.id) {
        <li [class]="'sev-' + alert.severity">
          <strong>{{ alert.title }}</strong>
          <span>{{ alert.message }}</span>
        </li>
      }
    </ul>
  `,
  styles: [
    `
      .alerts-list {
        list-style: none;
        margin: 0;
        padding: 0;
        display: grid;
        gap: 0.45rem;
      }

      li {
        border-radius: 0.5rem;
        padding: 0.45rem 0.55rem;
        border-left: 4px solid transparent;
        background: var(--p-surface-100);
        display: grid;
      }

      .sev-high {
        border-left-color: var(--p-red-500);
      }

      .sev-medium {
        border-left-color: var(--p-orange-500);
      }

      .sev-low {
        border-left-color: var(--p-blue-500);
      }

      strong {
        font-size: 0.8rem;
      }

      span {
        font-size: 0.75rem;
        color: var(--p-surface-600);
      }
    `
  ]
})
export class AlertsListWidgetComponent {
  @Input() config: Record<string, unknown> = {};
  @Input() datasource: Record<string, unknown> = {};
  @Input() runtime: DashboardWidgetRuntime = {};
  @Input() depth = 0;

  readonly alerts = [
    { id: 1, severity: 'high', title: 'Pressione fuori soglia', message: '3 pazienti con valori critici nelle ultime 2h.' },
    { id: 2, severity: 'medium', title: 'Terapia in scadenza', message: '12 piani terapeutici da rinnovare entro 7 giorni.' },
    { id: 3, severity: 'low', title: 'Reminder follow-up', message: '8 richiami programmati per domani mattina.' }
  ];
}
