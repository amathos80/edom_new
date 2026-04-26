import { Component, Input } from '@angular/core';
import { DashboardWidgetRuntime } from '../../models/dashboard.models';

@Component({
  selector: 'app-patients-table-widget',
  standalone: true,
  template: `
    <table class="patients-table">
      <thead>
        <tr>
          <th>Paziente</th>
          <th>Stato</th>
          <th>Ultimo accesso</th>
        </tr>
      </thead>
      <tbody>
        @for (row of rows; track row.id) {
          <tr>
            <td>{{ row.name }}</td>
            <td><span class="status" [class.active]="row.active">{{ row.active ? 'Attivo' : 'Sospeso' }}</span></td>
            <td>{{ row.lastAccess }}</td>
          </tr>
        }
      </tbody>
    </table>
  `,
  styles: [
    `
      .patients-table {
        width: 100%;
        border-collapse: collapse;
        font-size: 0.83rem;
      }

      th,
      td {
        text-align: left;
        border-bottom: 1px solid var(--p-surface-200);
        padding: 0.35rem;
      }

      th {
        color: var(--p-surface-500);
        font-weight: 600;
      }

      .status {
        border-radius: 999px;
        padding: 0.1rem 0.45rem;
        background: var(--p-orange-100);
        color: var(--p-orange-700);
      }

      .status.active {
        background: var(--p-green-100);
        color: var(--p-green-700);
      }
    `
  ]
})
export class PatientsTableWidgetComponent {
  @Input() config: Record<string, unknown> = {};
  @Input() datasource: Record<string, unknown> = {};
  @Input() runtime: DashboardWidgetRuntime = {};
  @Input() depth = 0;

  readonly rows = [
    { id: 1, name: 'Anna Bianchi', active: true, lastAccess: '26/04 10:22' },
    { id: 2, name: 'Luca Rossi', active: true, lastAccess: '26/04 10:08' },
    { id: 3, name: 'Marta Ferri', active: false, lastAccess: '25/04 18:47' },
    { id: 4, name: 'Paolo Neri', active: true, lastAccess: '25/04 14:12' }
  ];
}
