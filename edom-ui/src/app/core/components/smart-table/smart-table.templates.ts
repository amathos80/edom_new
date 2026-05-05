import { Directive, Input, TemplateRef } from '@angular/core';

@Directive({
  selector: 'ng-template[appSmartTableCell]',
  standalone: true
})
export class SmartTableCellTemplateDirective {
  @Input('appSmartTableCell') key = '';

  constructor(public readonly template: TemplateRef<unknown>) {}
}

@Directive({
  selector: 'ng-template[appSmartTableActions]',
  standalone: true
})
export class SmartTableActionsTemplateDirective {
  constructor(public readonly template: TemplateRef<unknown>) {}
}
