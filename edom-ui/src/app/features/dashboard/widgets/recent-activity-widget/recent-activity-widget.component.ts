import { Component, Input } from '@angular/core';
import { DashboardWidgetRuntime } from '../../models/dashboard.models';

@Component({
  selector: 'app-recent-activity-widget',
  standalone: true,
  template: `
    <ol class="activity-list">
      @for (item of items; track item.id) {
        <li>
          <span class="time">{{ item.time }}</span>
          <p>{{ item.text }}</p>
        </li>
      }
    </ol>
  `,
  styles: [
    `
      .activity-list {
        margin: 0;
        padding: 0 0 0 1.1rem;
        display: grid;
        gap: 0.5rem;
      }

      li {
        color: var(--p-surface-700);
      }

      .time {
        font-size: 0.7rem;
        color: var(--p-surface-500);
      }

      p {
        margin: 0.1rem 0 0;
        font-size: 0.8rem;
      }
    `
  ]
})
export class RecentActivityWidgetComponent {
  @Input() config: Record<string, unknown> = {};
  @Input() datasource: Record<string, unknown> = {};
  @Input() runtime: DashboardWidgetRuntime = {};
  @Input() depth = 0;

  readonly items = [
    { id: 1, time: '10:24', text: 'Referto aggiornato per Anna Bianchi.' },
    { id: 2, time: '10:03', text: 'Nuovo paziente registrato da segreteria.' },
    { id: 3, time: '09:47', text: 'Firma digitale completata per 5 documenti.' },
    { id: 4, time: '09:21', text: 'Sincronizzazione agenda completata.' }
  ];
}
