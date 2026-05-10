import { Directive, TemplateRef } from '@angular/core';

@Directive({
  selector: 'ng-template[appLookupDialogContent]',
  standalone: true
})
export class LookupDialogContentDirective {
  constructor(public readonly templateRef: TemplateRef<unknown>) {}
}
