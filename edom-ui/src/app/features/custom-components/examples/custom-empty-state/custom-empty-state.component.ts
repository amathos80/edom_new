import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-custom-empty-state',
  standalone: true,
  imports: [ButtonModule, CardModule],
  templateUrl: './custom-empty-state.component.html',
  styleUrl: './custom-empty-state.component.scss'
})
export class CustomEmptyStateComponent {}
