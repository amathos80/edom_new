import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { DashboardLayoutService } from './dashboard-layout.service';
import { AuthService } from '../../../core/services/auth.service';
import { DashboardWidgetsRegistryService } from '../registry/dashboard-widgets.registry';

describe('DashboardLayoutService', () => {
  let service: DashboardLayoutService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        DashboardWidgetsRegistryService,
        {
          provide: AuthService,
          useValue: {
            currentUser: () => ({ sub: 'u-test', unique_name: 'u-test' })
          }
        }
      ]
    });

    service = TestBed.inject(DashboardLayoutService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should fallback to local storage when backend fails', () => {
    const key = 'edom.dashboard.layout.u-test';
    localStorage.setItem(
      key,
      JSON.stringify({
        schemaVersion: 1,
        updatedAt: '2026-04-26T10:00:00.000Z',
        widgets: [
          {
            id: 'w1',
            type: 'kpi-summary',
            x: 0,
            y: 0,
            w: 3,
            h: 2,
            state: 'active',
            config: {},
            datasource: {}
          }
        ]
      })
    );

    service.loadLayout().subscribe(layout => {
      expect(layout.widgets.length).toBe(1);
      expect(layout.widgets[0].id).toBe('w1');
    });

    const req = httpMock.expectOne('/api/dashboard/layout');
    req.flush({}, { status: 500, statusText: 'Server error' });
  });

  it('should migrate old schema layout from backend', () => {
    service.loadLayout().subscribe(layout => {
      expect(layout.schemaVersion).toBe(1);
      expect(layout.widgets[0].state).toBe('active');
    });

    const req = httpMock.expectOne('/api/dashboard/layout');
    req.flush({
      schemaVersion: 0,
      updatedAt: '2026-04-26T10:00:00.000Z',
      widgets: [
        {
          id: 'legacy',
          type: 'kpi-summary',
          x: 1,
          y: 1,
          w: 3,
          h: 2,
          config: {},
          datasource: {}
        }
      ]
    });
  });
});
