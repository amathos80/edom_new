import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { CUSTOM_COMPONENT_ENTRIES } from '../../custom-components.registry';

@Component({
  selector: 'app-custom-components-home',
  standalone: true,
  imports: [RouterLink, CardModule, TagModule],
  templateUrl: './custom-components-home.component.html',
  styleUrl: './custom-components-home.component.scss'
})
export class CustomComponentsHomeComponent {
  readonly entries = CUSTOM_COMPONENT_ENTRIES;
}
