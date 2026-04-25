import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';
import { AvatarModule } from 'primeng/avatar';
import { RippleModule } from 'primeng/ripple';
import { TooltipModule } from 'primeng/tooltip';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ButtonModule, ToolbarModule, AvatarModule, RippleModule, TooltipModule],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent {
  readonly auth = inject(AuthService);
  readonly router = inject(Router);
  sidebarCollapsed = signal(false);

  readonly menuItems = [
    { label: 'Home', icon: 'pi pi-home', route: '/app/home' },
    { label: 'Pazienti', icon: 'pi pi-users', route: '/app/pazienti' }
  ];

  toggleSidebar(): void {
    this.sidebarCollapsed.update(v => !v);
  }

  logout(): void {
    this.auth.logout();
  }
}
