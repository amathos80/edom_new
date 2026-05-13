# PUA Lookup Control - Skeleton Specification

## Goal
Provide a reusable lookup input control based on PrimeNG textbox behavior, with:
- inline search button
- inline clear button
- modal dialog host for pluggable search components

A first concrete search component is included for anagrafica lookup (cognome, nome, codice fiscale, data nascita).

## Implemented Skeleton

### Reusable Control
- Component: `app-lookup-input`
- Path: `src/app/core/components/lookup-input/lookup-input.component.ts`
- CVA support: yes (`ControlValueAccessor`)
- UI:
  - text input (PrimeNG `pInputText`)
  - search button (`pi pi-search`) opens modal
  - clear button (`pi pi-times`) clears value
- Dialog host:
  - accepts projected template through `ng-template[appLookupDialogContent]`
  - template context includes:
    - `close`: function to close dialog
    - `value`: current input text value

### Dialog Content Directive
- Directive: `appLookupDialogContent`
- Path: `src/app/core/components/lookup-input/lookup-dialog-content.directive.ts`
- Purpose: typed anchor for projected dialog content template.

### Concrete Search Component (Anagrafica)
- Component: `app-anagrafica-search`
- Path: `src/app/features/pua/anagrafica-search/anagrafica-search.component.ts`
- Filters:
  - cognome
  - nome
  - codice fiscale
  - data nascita
- Result table columns:
  - cognome
  - nome
  - codice fiscale
  - data nascita
  - sesso
  - action "Seleziona"
- Current data source: mocked in component (`runSearch`) as placeholder.

## Contracts

### `app-lookup-input` inputs
- `placeholder: string`
- `label: string`
- `dialogTitle: string`
- `disabled: boolean`
- `readonly: boolean`
- `clearButtonAriaLabel: string`
- `searchButtonAriaLabel: string`

### `app-lookup-input` outputs
- `opened`: emitted when dialog opens
- `cleared`: emitted after clear action
- `searchTextChanged`: emitted on textbox changes

### `app-anagrafica-search` outputs
- `search: EventEmitter<AnagraficaSearchFilters>`
- `selected: EventEmitter<AnagraficaSearchResult>`

## Example Integration Pattern

```html
<app-lookup-input
  label="Assistito"
  placeholder="Cerca assistito"
  dialogTitle="Ricerca assistito"
  [(ngModel)]="assistitoDisplay"
>
  <ng-template appLookupDialogContent let-close="close">
    <app-anagrafica-search
      (selected)="onAssistitoSelected($event); close()"
    ></app-anagrafica-search>
  </ng-template>
</app-lookup-input>
```

## Integration Roadmap: PUA Management Form

### Step 1: Add Lookup Field to PUA Edit Form (Tab 1: Dati Generali)
Replace the placeholder paziente field in `PuaEditComponent` with `LookupInputComponent`:

```html
<!-- In pua-edit.component.html, Tab 1 -->
<app-lookup-input
  formControlName="pazienteSearch"
  label="Assistito"
  placeholder="Cerca assistito..."
  dialogTitle="Ricerca Assistito"
  [disabled]="form.get('pazienteSearch')?.disabled"
  (search)="onPazienteSearchClick()"
>
  <ng-template appLookupDialogContent let-close="close">
    <app-anagrafica-search
      (selectedPaziente)="onAssistitoSelected($event, close)"
    ></app-anagrafica-search>
  </ng-template>
</app-lookup-input>
```

### Step 2: Wire Selection Handler in Component
In `PuaEditComponent.ts`, implement the selection handler that auto-fills form fields:

```typescript
onAssistitoSelected(paziente: Paziente, close: () => void): void {
  // Populate form fields with selected patient data
  this.form.patchValue({
    pazienteId: paziente.id,
    pazienteCognome: paziente.cognome,
    pazienteNome: paziente.nome,
    pazienteCodiceFiscale: paziente.codiceFiscale,
    pazienteDataNascita: paziente.dataNascita,
    pazienteDisplay: `${paziente.cognome} ${paziente.nome}` // for read-only display
  });
  
  // Close dialog after successful selection
  close();
  
  // Mark form as touched to show any validation errors
  this.form.markAsTouched();
}

onPazienteSearchClick(): void {
  // Emit event to signal search initiated (for analytics/tracking if needed)
  console.log('User initiated paziente search');
}
```

### Step 3: Update Form Control Bindings
Ensure form has the required hidden fields for ID storage:

```typescript
// In PuaEditComponent initialization
this.form = this.fb.group({
  // ... existing fields
  pazienteId: [null, Validators.required],           // hidden ID
  pazienteCognome: [{value: '', disabled: true}],    // read-only display
  pazienteNome: [{value: '', disabled: true}],       // read-only display
  pazienteCodiceFiscale: [{value: '', disabled: true}],
  pazienteDataNascita: [{value: '', disabled: true}],
  pazienteSearch: [{value: '', disabled: false}],    // search control (not saved)
});
```

### Step 4: Update AnagraficaSearchComponent Output
Modify the component's `selected` event to emit paziente object:

```typescript
// In anagrafica-search.component.ts
@Output() selectedPaziente = output<Paziente>();

selectPaziente(paziente: Paziente): void {
  this.selectedPaziente.emit(paziente);
}
```

### Data Flow Diagram
```
User UI
├─ 1. Click search button on paziente field
│  └─ Dialog opens with AnagraficaSearchComponent
├─ 2. Enter filters (cognome, nome, codice fiscale, data nascita)
├─ 3. System queries backend (search results table)
├─ 4. Click "Seleziona" on chosen paziente
│  └─ selectedPaziente event fires with full Paziente object
├─ 5. onAssistitoSelected() handler receives event
│  ├─ Form.patchValue() fills pazienteId, pazienteCognome, etc.
│  ├─ Dialog closes automatically
│  └─ Form validation re-runs
└─ 6. User continues filling remaining PUA form fields
   └─ Save persists paziente selection to database
```

### Form Field Mapping Reference
| UI Display Field | Form Control (Saved) | Data Source | Type |
|---|---|---|---|
| "Assistito" textbox | `pazienteId` | Paziente.id | number (hidden) |
| Display: Cognome Nome | `pazienteCognome`, `pazienteNome` | Paziente (disabled) | string |
| Display: Cod. Fiscale | `pazienteCodiceFiscale` | Paziente (disabled) | string |
| Display: Data Nascita | `pazienteDataNascita` | Paziente (disabled) | date |
| Search textbox | `pazienteSearch` | User input (not saved) | string |

### Testing Checklist
- [ ] Search button opens dialog with AnagraficaSearchComponent
- [ ] Filters display correctly (cognome, nome, codice fiscale, data nascita)
- [ ] Search results table loads after filter submission
- [ ] "Seleziona" button emits selectedPaziente with full object
- [ ] Form fields populate on selection (not saved if user cancels)
- [ ] Dialog closes after selection
- [ ] Form validation shows pazienteId as required
- [ ] Can clear selection via clear button (sets pazienteId to null)
- [ ] Disabled state works (can't search when form is disabled)

## Planned Next Steps (Phase 2)
1. Introduce `AnagraficaSearchService` with HTTP backend call to replace mock data.
2. Add debounce + server-side paging/sorting for large result sets.
3. Add keyboard support (`Enter` to search, `Esc` to close dialog).
4. Add validation state styling in `app-lookup-input` (error borders, hints).
5. Add unit tests for CVA contracts and selection event flow.
6. Add Analytics: track search queries, selection frequency, field population rate.

## Open Decisions
- Return payload for selection:
  - full row object (**chosen: simpler integration**), or
  - compact DTO (`id`, `displayText`) + optional lazy load details.
- Dialog sizing strategy for mobile vs desktop.
- Shared lookup result table component vs feature-specific tables.
- Should pazienteSearch field be visible or hidden post-selection (for UX clarity)?
