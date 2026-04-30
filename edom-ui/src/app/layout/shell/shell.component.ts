import { DOCUMENT } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';
import { AvatarModule } from 'primeng/avatar';
import { RippleModule } from 'primeng/ripple';
import { TooltipModule } from 'primeng/tooltip';
import { PrimeNG } from 'primeng/config';
import { SelectModule } from 'primeng/select';
import Aura from '@primeuix/themes/aura';
import Lara from '@primeuix/themes/lara';
import Nora from '@primeuix/themes/nora';
import Material from '@primeuix/themes/material';
import { AuthService } from '../../core/services/auth.service';
import { TopMenuComponent } from '../top-menu/top-menu.component';

type ThemePresetKey = 'aura' | 'lara' | 'nora' | 'material';
type ColorScheme = 'light' | 'dark';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, FormsModule, ButtonModule, ToolbarModule, AvatarModule, RippleModule, TooltipModule, SelectModule, TopMenuComponent],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent {
  readonly auth = inject(AuthService);
  readonly router = inject(Router);
  private readonly primeng = inject(PrimeNG);
  private readonly document = inject(DOCUMENT);

  private readonly themeStorageKey = 'edom.themePreset';
  private readonly modeStorageKey = 'edom.colorScheme';

  private readonly themePresets: Record<ThemePresetKey, object> = {
    aura: Aura,
    lara: Lara,
    nora: Nora,
    material: Material
  };

  sidebarCollapsed = signal(false);
  selectedTheme = signal<ThemePresetKey>('aura');
  selectedMode = signal<ColorScheme>('light');

  readonly themeOptions: Array<{ label: string; value: ThemePresetKey }> = [
    { label: 'Aura', value: 'aura' },
    { label: 'Lara', value: 'lara' },
    { label: 'Nora', value: 'nora' },
    { label: 'Material', value: 'material' }
  ];

  readonly modeOptions: Array<{ label: string; value: ColorScheme }> = [
    { label: 'Light', value: 'light' },
    { label: 'Dark', value: 'dark' }
  ];

  readonly menuItems = [
    { label: 'Home', icon: 'pi pi-home', route: '/app/home' },
    { label: 'Dashboard', icon: 'pi pi-th-large', route: '/app/dashboard' },
    { label: 'Pazienti', icon: 'pi pi-users', route: '/app/pazienti' },
    { label: 'Custom Components', icon: 'pi pi-box', route: '/app/custom-components' }
  ];

  constructor() {
    this.restoreThemePreferences();
    this.applyTheme();
    this.applyColorScheme();
  }

  toggleSidebar(): void {
    this.sidebarCollapsed.update(v => !v);
  }

  onThemeChange(value: ThemePresetKey): void {
    this.selectedTheme.set(value);
    this.applyTheme();
    this.persistThemePreferences();
  }

  onModeChange(value: ColorScheme): void {
    this.selectedMode.set(value);
    this.applyColorScheme();
    this.persistThemePreferences();
  }

  logout(): void {
    this.auth.logout();
  }

  private restoreThemePreferences(): void {
    const storedTheme = localStorage.getItem(this.themeStorageKey);
    const storedMode = localStorage.getItem(this.modeStorageKey);

    if (storedTheme && this.isThemePresetKey(storedTheme)) {
      this.selectedTheme.set(storedTheme);
    }

    if (storedMode && this.isColorScheme(storedMode)) {
      this.selectedMode.set(storedMode);
    }
  }

  private persistThemePreferences(): void {
    localStorage.setItem(this.themeStorageKey, this.selectedTheme());
    localStorage.setItem(this.modeStorageKey, this.selectedMode());
  }

  private applyTheme(): void {
    this.primeng.setThemeConfig({
      theme: {
        preset: this.themePresets[this.selectedTheme()],
        options: { darkModeSelector: '.dark-mode' }
      }
    });
  }

  private applyColorScheme(): void {
    this.document.documentElement.classList.toggle('dark-mode', this.selectedMode() === 'dark');
  }

  private isThemePresetKey(value: string): value is ThemePresetKey {
    return value === 'aura' || value === 'lara' || value === 'nora' || value === 'material';
  }

  private isColorScheme(value: string): value is ColorScheme {
    return value === 'light' || value === 'dark';
  }
}
