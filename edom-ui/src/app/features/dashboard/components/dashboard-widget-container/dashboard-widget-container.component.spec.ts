import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DashboardWidgetContainerComponent } from './dashboard-widget-container.component';
import { DashboardWidgetsRegistryService } from '../../registry/dashboard-widgets.registry';

describe('DashboardWidgetContainerComponent', () => {
  let fixture: ComponentFixture<DashboardWidgetContainerComponent>;
  let component: DashboardWidgetContainerComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardWidgetContainerComponent],
      providers: [DashboardWidgetsRegistryService]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardWidgetContainerComponent);
    component = fixture.componentInstance;
  });

  it('should emit remove event', () => {
    component.widget = {
      id: 'to-remove',
      type: 'kpi-summary',
      x: 0,
      y: 0,
      w: 3,
      h: 2,
      state: 'active',
      config: {},
      datasource: {}
    };

    spyOn(component.removeRequested, 'emit');
    component.requestRemove();

    expect(component.removeRequested.emit).toHaveBeenCalledWith('to-remove');
  });

  it('should set error when widget type is missing from registry', async () => {
    component.widget = {
      id: 'unknown',
      type: 'missing-widget',
      x: 0,
      y: 0,
      w: 2,
      h: 2,
      state: 'active',
      config: {},
      datasource: {}
    };

    component.ngOnChanges();
    await fixture.whenStable();

    expect(component.error()).toContain('missing-widget');
  });
});
