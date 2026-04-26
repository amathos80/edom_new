import { CommonModule, DecimalPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { DashboardWidgetRuntime } from '../../models/dashboard.models';

@Component({
  selector: 'app-kpi-summary-widget',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  template: `
    <section class="kpi-grid">
      @for (item of kpis; track item.label) {
        <article class="kpi-tile">
          <span class="kpi-label">{{ item.label }}</span>
          <strong class="kpi-value">{{ item.value | number: '1.0-0' }}</strong>
          <small [class]="item.delta >= 0 ? 'delta-up' : 'delta-down'">
            {{ item.delta >= 0 ? '+' : '' }}{{ item.delta }}%
          </small>
        </article>
      }
    </section>
  `,
  styles: [
    `
      .kpi-grid {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 0.6rem;
      }

      .kpi-tile {
        border: 1px solid var(--p-surface-200);
        border-radius: 0.7rem;
        padding: 0.5rem 0.65rem;
        background: #fff;
      }

      .kpi-label {
        display: block;
        color: var(--p-surface-500);
        font-size: 0.75rem;
      }

      .kpi-value {
        display: block;
        font-size: 1.2rem;
      }

      .delta-up {
        color: var(--p-green-600);
      }

      .delta-down {
        color: var(--p-red-600);
      }
    `
  ]
})
export class KpiSummaryWidgetComponent {
  @Input() config: Record<string, unknown> = {};
  @Input() datasource: Record<string, unknown> = {};
  @Input() runtime: DashboardWidgetRuntime = {};
  @Input() depth = 0;

  readonly kpis = [
    { label: 'Pazienti attivi', value: 842, delta: 2.7 },
    { label: 'Visite oggi', value: 127, delta: 5.2 },
    { label: 'Referti in coda', value: 31, delta: -3.1 },
    { label: 'Tempo medio', value: 19, delta: -1.4 }
  ];
}
