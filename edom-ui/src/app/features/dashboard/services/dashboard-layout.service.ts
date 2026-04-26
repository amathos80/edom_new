import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, debounceTime, finalize, of, retry, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { AuthService } from '../../../core/services/auth.service';
import { DashboardLayout, DashboardWidgetInstance } from '../models/dashboard.models';
import { DashboardWidgetsRegistryService } from '../registry/dashboard-widgets.registry';

const DASHBOARD_SCHEMA_VERSION = 1;
const SAVE_DEBOUNCE_MS = 450;

@Injectable({ providedIn: 'root' })
export class DashboardLayoutService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly registry = inject(DashboardWidgetsRegistryService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly baseUrl = '/api/dashboard/layout';
  private readonly saveQueue = signal<DashboardLayout | null>(null);

  private readonly _layout = signal<DashboardLayout>(this.createDefaultLayout());
  private readonly _loading = signal(false);
  private readonly _saving = signal(false);

  readonly layout = this._layout.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly saving = this._saving.asReadonly();
  readonly activeWidgets = computed(() =>
    this._layout()
      .widgets
      .filter(widget => widget.state === 'active')
  );

  constructor() {
    this.bindSavePipeline();
  }

  loadLayout() {
    this._loading.set(true);

    return this.http.get<DashboardLayout>(this.baseUrl).pipe(
      switchMap(remoteLayout => {
        const normalized = this.normalizeLayout(remoteLayout);
        this._layout.set(normalized);
        this.saveLocal(normalized);
        return of(normalized);
      }),
      catchError(() => {
        const fallback = this.loadLocal() ?? this.createSeedLayout();
        this._layout.set(fallback);
        return of(fallback);
      }),
      finalize(() => this._loading.set(false))
    );
  }

  updateWidgets(widgets: DashboardWidgetInstance[]): void {
    const nextLayout: DashboardLayout = {
      ...this._layout(),
      updatedAt: new Date().toISOString(),
      widgets
    };

    this._layout.set(nextLayout);
    this.enqueueSave(nextLayout);
  }

  addWidget(type: string): void {
    const activeCount = this.activeWidgets().length;
    const base = this.registry.createDefaultInstance(type, {
      x: (activeCount * 2) % 12,
      y: Math.floor(activeCount / 4) * 2
    });

    this.updateWidgets([...this._layout().widgets, base]);
  }

  removeWidget(id: string): void {
    const widgets = this._layout().widgets.map(widget =>
      widget.id === id ? { ...widget, state: 'removed' as const } : widget
    );
    this.updateWidgets(widgets);
  }

  restoreDefaults(): void {
    const seeded = this.createSeedLayout();
    this._layout.set(seeded);
    this.enqueueSave(seeded);
  }

  private enqueueSave(layout: DashboardLayout): void {
    this.saveLocal(layout);
    this.saveQueue.set(layout);
  }

  private bindSavePipeline(): void {
    toObservable(this.saveQueue)
      .pipe(
        debounceTime(SAVE_DEBOUNCE_MS),
        switchMap(layout => {
          if (!layout) {
            return of(null);
          }

          this._saving.set(true);
          return this.http.put<DashboardLayout>(this.baseUrl, layout).pipe(
            retry({ count: 1, delay: 350 }),
            tap(remote => {
              const normalized = this.normalizeLayout(remote);
              this._layout.set(normalized);
              this.saveLocal(normalized);
            }),
            catchError(() => {
              this.saveLocal(layout);
              return of(layout);
            }),
            finalize(() => {
              this._saving.set(false);
              this.saveQueue.set(null);
            })
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();
  }

  private normalizeLayout(layout: DashboardLayout | null | undefined): DashboardLayout {
    const candidate = layout ?? this.createDefaultLayout();
    const migrated = this.migrateIfNeeded(candidate);

    return {
      schemaVersion: DASHBOARD_SCHEMA_VERSION,
      updatedAt: migrated.updatedAt ?? new Date().toISOString(),
      widgets: migrated.widgets
        .filter(widget => widget.state !== 'removed')
        .map(widget => this.normalizeWidget(widget))
    };
  }

  private normalizeWidget(widget: DashboardWidgetInstance): DashboardWidgetInstance {
    const metadata = this.registry.getByType(widget.type);

    return {
      ...widget,
      title: widget.title ?? metadata?.title ?? widget.type,
      state: widget.state ?? 'active',
      x: Number.isFinite(widget.x) ? widget.x : 0,
      y: Number.isFinite(widget.y) ? widget.y : 0,
      w: Number.isFinite(widget.w) ? widget.w : metadata?.defaultW ?? 3,
      h: Number.isFinite(widget.h) ? widget.h : metadata?.defaultH ?? 2,
      config: widget.config ?? {},
      datasource: widget.datasource ?? {},
      children: (widget.children ?? []).slice(0, 3)
    };
  }

  private migrateIfNeeded(layout: DashboardLayout): DashboardLayout {
    if ((layout.schemaVersion ?? 0) >= DASHBOARD_SCHEMA_VERSION) {
      return layout;
    }

    return {
      ...layout,
      schemaVersion: DASHBOARD_SCHEMA_VERSION,
      widgets: layout.widgets?.map(widget => ({ ...widget, state: widget.state ?? 'active' })) ?? []
    };
  }

  private createDefaultLayout(): DashboardLayout {
    return {
      schemaVersion: DASHBOARD_SCHEMA_VERSION,
      updatedAt: new Date().toISOString(),
      widgets: []
    };
  }

  private createSeedLayout(): DashboardLayout {
    const initialTypes = ['kpi-summary', 'patients-table', 'trend-chart', 'alerts-list', 'recent-activity', 'system-status'];

    return {
      schemaVersion: DASHBOARD_SCHEMA_VERSION,
      updatedAt: new Date().toISOString(),
      widgets: initialTypes.map((type, index) =>
        this.registry.createDefaultInstance(type, {
          x: (index % 3) * 4,
          y: Math.floor(index / 3) * 3
        })
      )
    };
  }

  private localKey(): string {
    const user = this.auth.currentUser();
    const id = user?.sub ?? user?.unique_name ?? 'anonymous';
    return `edom.dashboard.layout.${id}`;
  }

  private loadLocal(): DashboardLayout | null {
    try {
      const raw = localStorage.getItem(this.localKey());
      if (!raw) {
        return null;
      }

      return this.normalizeLayout(JSON.parse(raw) as DashboardLayout);
    } catch {
      return null;
    }
  }

  private saveLocal(layout: DashboardLayout): void {
    localStorage.setItem(this.localKey(), JSON.stringify(layout));
  }
}
