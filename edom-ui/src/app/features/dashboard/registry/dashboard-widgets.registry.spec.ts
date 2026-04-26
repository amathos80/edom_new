import { TestBed } from '@angular/core/testing';
import { DashboardWidgetsRegistryService } from './dashboard-widgets.registry';

describe('DashboardWidgetsRegistryService', () => {
  let service: DashboardWidgetsRegistryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DashboardWidgetsRegistryService);
  });

  it('should expose MVP catalog widgets', () => {
    const all = service.getAll();
    expect(all.length).toBeGreaterThanOrEqual(6);
    expect(all.some(widget => widget.type === 'kpi-summary')).toBeTrue();
  });

  it('should create default instance with metadata defaults', () => {
    const instance = service.createDefaultInstance('trend-chart');

    expect(instance.id).toBeTruthy();
    expect(instance.type).toBe('trend-chart');
    expect(instance.state).toBe('active');
    expect(instance.w).toBeGreaterThan(0);
    expect(instance.h).toBeGreaterThan(0);
  });
});
