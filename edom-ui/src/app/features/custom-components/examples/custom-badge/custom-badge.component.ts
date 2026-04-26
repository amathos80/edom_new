import { Component } from '@angular/core';
import { TagModule } from 'primeng/tag';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-custom-badge',
  standalone: true,
  imports: [TagModule, CardModule],
  templateUrl: './custom-badge.component.html',
  styleUrl: './custom-badge.component.scss'
})
export class CustomBadgeComponent {
  readonly states = [
    { label: 'Success', severity: 'success' as const },
    { label: 'Warning', severity: 'warn' as const },
    { label: 'Error', severity: 'danger' as const },
    { label: 'Info', severity: 'info' as const }
  ];
}
