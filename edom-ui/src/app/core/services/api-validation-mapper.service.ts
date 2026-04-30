import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AbstractControl, FormArray, FormGroup } from '@angular/forms';

@Injectable({ providedIn: 'root' })
export class ApiValidationMapperService {
  applyToForm(form: FormGroup, error: unknown): boolean {
    const payload = this.readValidationPayload(error);
    if (!payload) {
      return false;
    }

    let mapped = 0;
    for (const [field, messages] of Object.entries(payload)) {
      const control = this.findControl(form, field);
      if (!control) {
        continue;
      }

      const previous = control.errors ?? {};
      control.setErrors({
        ...previous,
        api: messages.join(' '),
      });
      control.markAsTouched();
      control.markAsDirty();
      mapped++;
    }

    if (mapped === 0) {
      const allMessages = Object.values(payload).flat();
      const previous = form.errors ?? {};
      form.setErrors({
        ...previous,
        api: allMessages.join(' '),
      });
      form.markAsTouched();
    }

    return mapped > 0;
  }

  private readValidationPayload(error: unknown): Record<string, string[]> | null {
    const httpError = error as HttpErrorResponse | undefined;
    if (!httpError || httpError.status !== 400) {
      return null;
    }

    const maybeErrors = (httpError.error as { errors?: unknown } | undefined)?.errors;
    if (!maybeErrors || typeof maybeErrors !== 'object' || Array.isArray(maybeErrors)) {
      return null;
    }

    const parsed: Record<string, string[]> = {};
    for (const [key, value] of Object.entries(maybeErrors as Record<string, unknown>)) {
      if (Array.isArray(value)) {
        parsed[key] = value.map(v => String(v));
        continue;
      }

      if (typeof value === 'string') {
        parsed[key] = [value];
      }
    }

    return Object.keys(parsed).length > 0 ? parsed : null;
  }

  private findControl(form: FormGroup, field: string): AbstractControl | null {
    const byExact = form.get(field);
    if (byExact) {
      return byExact;
    }

    const byCamel = form.get(this.toCamelCase(field));
    if (byCamel) {
      return byCamel;
    }

    const normalizedField = this.normalize(field);
    for (const [path, control] of this.flattenControls(form)) {
      const normalizedPath = this.normalize(path);
      const lastSegment = path.split('.').at(-1) ?? path;
      const normalizedLeaf = this.normalize(lastSegment);

      if (normalizedField === normalizedPath || normalizedField === normalizedLeaf) {
        return control;
      }
    }

    return null;
  }

  private flattenControls(control: AbstractControl, prefix = ''): Array<[string, AbstractControl]> {
    if (control instanceof FormGroup) {
      return Object.entries(control.controls).flatMap(([key, child]) => {
        const nextPrefix = prefix ? `${prefix}.${key}` : key;
        return this.flattenControls(child, nextPrefix);
      });
    }

    if (control instanceof FormArray) {
      return control.controls.flatMap((child, index) => {
        const nextPrefix = prefix ? `${prefix}.${index}` : String(index);
        return this.flattenControls(child, nextPrefix);
      });
    }

    return [[prefix, control]];
  }

  private normalize(value: string): string {
    return value.replace(/[^a-zA-Z0-9]/g, '').toLowerCase();
  }

  private toCamelCase(value: string): string {
    const normalized = value.trim();
    if (!normalized) {
      return normalized;
    }

    return normalized.charAt(0).toLowerCase() + normalized.slice(1);
  }
}
