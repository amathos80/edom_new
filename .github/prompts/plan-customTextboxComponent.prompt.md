# Plan: Estendere CustomTextboxComponent con tutte le proprietà pInputText

## TL;DR
Trasformare CustomTextboxInputComponent in un wrapper completo di pInputText che supporti tutte le proprietà (variant, size, fluid, invalid, type, etc.) e che consenta di definire form bindings direttamente nel tag, mantenendo ControlValueAccessor per reactive forms.

**Strategia:** aggiungere @Input per ogni proprietà di pInputText, estendere il template per passare gli attributi dinamici, e supportare direttamente `formControlName` senza dover wrappare in `<input>`.

---

## Steps

### Fase 1: Estensione Componente TypeScript
1. Aggiungere @Input per proprietà pInputText:
   - `variant: 'filled' | 'outlined' = 'outlined'`
   - `size: 'small' | 'large' | undefined`
   - `fluid: boolean = false` (span 100% width)
   - `invalid: boolean = false` (styling per validazione fallita)
   - `type: 'text' | 'email' | 'password' | 'number' | etc. = 'text'`
   - `readonly: boolean = false`
   - `name: string = ''` (accessibility, form naming)
   - `styleClass: string = ''` (aggiungere classe CSS custom)

2. Ampliare le proprietà già presenti:
   - `placeholder` (già presente, mantenere)
   - `uppercase` (già presente, mantenere)
   - `maxLength` (già presente, mantenere)

3. Aggiungere logica per applicare invalid styling:
   - Legare `invalid` a una variabile computed che consideri sia l'@Input che lo stato del control (touched + errors)

### Fase 1bis: Supporto Validazioni Dichiarative (Self-Contained)
1. Aggiungere @Input per regole di validazione:
   - `required: boolean = false`
   - `minLength: number | null = null`
   - `maxLength: number | null = null` (già presente come proprietà, estendere a validator)
   - `pattern: string | null = null`
   - `email: boolean = false`
   - `number: boolean = false`

2. Iniettare `NgControl` opzionalmente:
   ```typescript
   private ngControl = inject(NgControl, { optional: true });
   ```

3. Utilizzare `effect()` per applicare i validator automaticamente al FormControl:
   - Quando gli @Input cambiano → ricostruire array di ValidatorFn
   - Applicare via `setValidators()` + `updateValueAndValidity()`
   - Il componente si auto-configura senza intervento del padre

4. Benefici:
   - Nel template: `<app-custom-textbox-input formControlName="email" email required placeholder="...">`
   - Zero configurazione nel reactive form
   - Validazioni dichiarate accanto al controllo

### Fase 1ter: Approccio C (Ibrido: Built-in + Custom + Remote)
1. Esporre input ibridi per validazione:
   - Built-in: `required`, `minLength`, `maxLength`, `pattern`, `email`, `number`
   - Custom diretti: `customValidators: (ValidatorFn | AsyncValidatorFn)[] = []`
   - Remote dichiarativi: `customValidationType: string | null = null`

2. Comporre i validator in un unico pipeline:
   - `buildBuiltInValidators()` per i validator standard
   - Merge con `customValidators`
   - Se presente `customValidationType`, risoluzione tramite registry/service centralizzato

3. Applicazione automatica al control:
   - `setValidators(syncValidators)`
   - `setAsyncValidators(asyncValidators)`
   - `updateValueAndValidity({ emitEvent: false })` per evitare loop inutili

4. Supporto concreto alle remote validation:
   - Esempi: `emailUnique`, `codiceFiscaleUnique`
   - Debounce lato validator async e short-circuit su valore vuoto/non valido
   - Gestione error key specifiche (`{ emailExists: true }`, `{ cfExists: true }`)

5. Esempio template con approccio C:
   - `<app-custom-textbox-input formControlName="uiEmail" type="email" [required]="true" [email]="true" [customValidators]="[emailUniqueValidator]" customValidationType="emailUnique" />`
   - `<app-custom-textbox-input formControlName="pazienteCodiceFiscale" [required]="true" [pattern]="'^[A-Z0-9]{16}$'" [uppercase]="true" [customValidators]="[codiceFiscaleRemoteValidator]" customValidationType="codiceFiscaleUnique" />`

### Fase 2: Estensione Template HTML
1. Passare tutte le nuove @Input come binding all'`<input>`:
   - `[type]="type"`
   - `[readonly]="readonly"`
   - `[ngClass]="{'p-invalid': invalid || hasValidationError()}"` (per styling)
   - `[class]="styleClass"`

2. Mantenere il binding di `pInputText` (direttiva PrimeNG)

3. Gestire la visualizzazione dello stato invalid via CSS class

### Fase 3: Supporto Validazione Reactive Forms
1. Aggiungere metodo per determinare se il control ha errori:
   - `hasValidationError()`: restituisce true se control is touched AND ha errori

2. Riflettere questo nello state di rendering (CSS class `p-invalid`)

3. Opzionalmente: aggiungere @Output per esporre validation messages

### Fase 4: Backward Compatibility & Testing
1. Verificare che componente funzioni ancora con uso attuale:
   - `<app-custom-textbox-input formControlName="cognome" placeholder="..." uppercase="true" maxLength="50"></app-custom-textbox-input>`

2. Verificare nuove proprietà:
   - `<app-custom-textbox-input formControlName="email" type="email" variant="filled" size="large" placeholder="..."></app-custom-textbox-input>`

3. Verificare styling invalid:
   - Control non touched: nessun styling
   - Control touched + errors: colore rosso
   - Control valid: colore normale

---

## Relevant files
- `edom-ui/src/app/features/custom-components/components/custom-textbox/custom-textbox.component.ts` — aggiungere @Input e logica validazione
- `edom-ui/src/app/features/custom-components/components/custom-textbox/custom-textbox.component.html` — estendere template con binding dinamici
- `edom-ui/src/app/features/pua/pua-edit/pua-edit.component.html` — sostituire `<input pInputText ...>` con `<app-custom-textbox-input ...>`

---

## Verification
1. Build Angular: `ng build` (no errors)
2. Visual test in browser:
   - Selezionare campo in pua-edit form
   - Verificare styling variant (outlined vs filled)
   - Verificare size (small/large se usati)
   - Verificare invalid styling dopo touch + errore validazione
   - Verificare onChange/onBlur funzionano con reactive forms
3. Unit test (opzionale):
   - ControlValueAccessor writeValue/registerOnChange comportamenti
   - Uppercase transformation
   - Invalid state computation

---

## Decisions
- **Diretta integrazione pInputText**: componente rimane basato sulla direttiva PrimeNG (no reimplementazione), solo aggiunge layer di properties
- **ControlValueAccessor mantenuto**: nessun cambio alla logica di reactive forms binding
- **CSS invalid-state**: usare classe `p-invalid` di PrimeNG (già supportata)
- **Graduale rollout**: supportare sia forma vecchia (raw input) che nuova (custom component) in pua-edit

---

## Further Considerations
1. **Quando implementare il rollout di sostituzione nei form?** 
   - Opzione A: Sostituire solo pua-edit come POC, lasciare altri form con `<input pInputText>`
   - Opzione B: Creare wrapper uniforme per tutto il progetto
   - **Raccomandazione**: Opzione A (focus su pua-edit per ora)

2. **Aggiungere messaggi di errore visivi inline?**
   - Opzione A: No — il componente rimane un input puro, validazione gestita dal form
   - Opzione B: Sì — aggiungere @Input `showErrorMessage` e display errore sotto input
   - **Raccomandazione**: Opzione A (mantenere simplicità, il form già mostra errori)

3. **Support per aria-label e accessibility?**
   - Opzione A: Semplice @Input `label` per `aria-label`
   - Opzione B: No — gestire a livello di label HTML esterna
   - **Raccomandazione**: Opzione A (aggiungere più tardi se necessario)
