import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MenuItem } from 'primeng/api';
import { MenubarModule } from 'primeng/menubar';
import { MenuService } from '../../core/services/menu.service';

@Component({
  selector: 'app-top-menu',
  standalone: true,
  imports: [MenubarModule],
  templateUrl: './top-menu.component.html',
  styleUrl: './top-menu.component.scss',
})
export class TopMenuComponent {
  private readonly menuService = inject(MenuService);

  readonly items = toSignal(
    this.menuService.loadAndBuild(),
    { initialValue: [] as MenuItem[] },
  );
}

