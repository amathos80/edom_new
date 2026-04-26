import { Component, Input } from '@angular/core';
import { DashboardWidgetRuntime } from '../../models/dashboard.models';

@Component({
  selector: 'app-system-status-widget',
  standalone: true,
  template: `
    <ul class="status-list">
      @for (service of services; track service.name) {
        <li>
          <span>{{ service.name }}</span>
          <strong [class]="service.ok ? 'ok' : 'ko'">{{ service.ok ? 'OK' : 'DOWN' }}</strong>
        </li>
      }
    </ul>
  `,
  styles: [
    `
      .status-list {
        list-style: none;
        margin: 0;
        padding: 0;
        display: grid;
        gap: 0.35rem;
      }

      li {
        display: flex;
        justify-content: space-between;
        border: 1px solid var(--p-surface-200);
        border-radius: 0.5rem;
        padding: 0.4rem 0.55rem;
      }

      .ok {
        color: var(--p-green-600);
      }

      .ko {
        color: var(--p-red-600);
      }
    `
  ]
})
export class SystemStatusWidgetComponent {
  @Input() config: Record<string, unknown> = {};
  @Input() datasource: Record<string, unknown> = {};
  @Input() runtime: DashboardWidgetRuntime = {};
  @Input() depth = 0;

  readonly services = [
    { name: 'API Gateway', ok: true },
    { name: 'Anagrafe Pazienti', ok: true },
    { name: 'Notifiche', ok: true },
    { name: 'Storage Referti', ok: true }
  ];
}
