import { Component, Input } from '@angular/core';
import { DashboardWidgetRuntime } from '../../models/dashboard.models';

@Component({
  selector: 'app-trend-chart-widget',
  standalone: true,
  template: `
    <div class="trend-bars" aria-label="Trend visite settimanali">
      @for (point of trend; track point.day) {
        <div class="bar-wrap">
          <span class="bar" [style.height.%]="point.value"></span>
          <small>{{ point.day }}</small>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .trend-bars {
        height: 120px;
        display: grid;
        grid-template-columns: repeat(7, minmax(0, 1fr));
        align-items: end;
        gap: 0.35rem;
      }

      .bar-wrap {
        display: grid;
        justify-items: center;
        gap: 0.3rem;
      }

      .bar {
        width: 100%;
        border-radius: 0.45rem;
        background: linear-gradient(180deg, #22c55e 0%, #0ea5e9 100%);
        min-height: 8px;
      }

      small {
        color: var(--p-surface-500);
        font-size: 0.65rem;
      }
    `
  ]
})
export class TrendChartWidgetComponent {
  @Input() config: Record<string, unknown> = {};
  @Input() datasource: Record<string, unknown> = {};
  @Input() runtime: DashboardWidgetRuntime = {};
  @Input() depth = 0;

  readonly trend = [
    { day: 'Lun', value: 62 },
    { day: 'Mar', value: 84 },
    { day: 'Mer', value: 53 },
    { day: 'Gio', value: 76 },
    { day: 'Ven', value: 69 },
    { day: 'Sab', value: 34 },
    { day: 'Dom', value: 25 }
  ];
}
