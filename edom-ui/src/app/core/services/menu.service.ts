import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, switchMap } from 'rxjs';
import { MenuItem } from 'primeng/api';
import { AuthService } from './auth.service';
import { MenuConfig, MenuNode } from '../models/menu.models';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private readonly auth = inject(AuthService);
  private readonly http = inject(HttpClient);

  /**
   * Carica il file JSON dall'URL indicato e restituisce un Observable
   * con i MenuItem PrimeNG già filtrati per permessi.
   * L'URL di default punta all'asset statico `public/config/top-menu.json`.
   */
  loadAndBuild(url = 'config/top-menu.json'): Observable<MenuItem[]> {
    return this.auth.assicuraPermessiCaricati().pipe(
      switchMap(() => this.http.get<MenuConfig>(url)),
      map(config => this.buildMenuItems(config))
    );
  }

  /**
   * Converte la configurazione menu in un array PrimeNG MenuItem[],
   * filtrando le voci per cui l'utente corrente non ha il permesso richiesto.
   * I gruppi vuoti (senza figli visibili) vengono omessi automaticamente.
   */
  buildMenuItems(config: MenuConfig): MenuItem[] {
    const permessiUtente = this.risolviPermessiUtente();
    return config.menu
      .map(node => this.toMenuItem(node, permessiUtente))
      .filter((item): item is MenuItem => item !== null);
  }

  // ── Private ────────────────────────────────────────────────────────────────

  /**
   * Ricava il Set di codici permesso dell'utente corrente.
   * Il campo `role` del JWT può essere una stringa singola o un array.
   */
  private risolviPermessiUtente(): Set<string> {
    const permessi = this.auth.permessiCorrenti();
    if (permessi?.funzioni?.length) {
      return new Set(permessi.funzioni);
    }

    const utente = this.auth.utenteCorrente();
    if (!utente) return new Set();
    const ruoli = Array.isArray(utente.role) ? utente.role : [utente.role];
    return new Set(ruoli);
  }

  private hasPermission(permissions: string | undefined, userPermissions: Set<string>): boolean {
    if (!permissions) return true;
    return userPermissions.has(permissions);
  }

  /**
   * Converte un MenuNode in MenuItem PrimeNG.
   * Restituisce null se l'utente non ha il permesso oppure se
   * il nodo è un gruppo senza figli visibili.
   */
  private toMenuItem(node: MenuNode, userPermissions: Set<string>): MenuItem | null {
    if (!this.hasPermission(node.permissions, userPermissions)) return null;

    const item: MenuItem = { label: node.text };
    if (node.icon) item.icon = node.icon;

    if (node.children?.length) {
      const childItems = node.children
        .map(c => this.toMenuItem(c, userPermissions))
        .filter((i): i is MenuItem => i !== null);

      // Gruppo senza figli visibili e senza navigazione propria → nasconde
      if (childItems.length === 0 && !node.routerLink && !node.externalUrl) {
        return null;
      }
      if (childItems.length > 0) {
        item.items = childItems;
      }
    }

    this.applyNavigation(item, node);
    return item;
  }

  /**
   * Imposta `routerLink`, `url` o `command` sul MenuItem
   * in base al tipo di navigazione del nodo.
   */
  private applyNavigation(item: MenuItem, node: MenuNode): void {
    if (node.routerLink) {
      item.routerLink = node.routerLink;
      return;
    }

    if (node.externalUrl) {
      const resolvedUrl = MenuService.interpolateParams(
        node.externalUrl,
        node.externalParams ?? {},
      );

      if (node.method === 'POST') {
        item.command = () => MenuService.postNavigate(resolvedUrl, node.target);
      } else {
        item.url = resolvedUrl;
        item.target = node.target ?? '_self';
      }
    }
  }

  /**
   * Sostituisce i segnaposto {key} nell'URL con i valori forniti.
   * I segnaposto non presenti in params vengono lasciati invariati.
   */
  private static interpolateParams(url: string, params: Record<string, string>): string {
    return url.replace(/\{(\w+)\}/g, (match, key) => params[key] ?? match);
  }

  /**
   * Naviga via HTTP POST creando un form temporaneo e sottomettendolo.
   * Il form viene rimosso dal DOM immediatamente dopo l'invio.
   */
  private static postNavigate(
    url: string,
    target: string = '_blank',
    params: Record<string, string> = {},
  ): void {
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = url;
    form.target = target;

    for (const [key, value] of Object.entries(params)) {
      const input = document.createElement('input');
      input.type = 'hidden';
      input.name = key;
      input.value = value;
      form.appendChild(input);
    }

    document.body.appendChild(form);
    form.submit();
    document.body.removeChild(form);
  }
}
