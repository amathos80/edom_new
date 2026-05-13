# Lookup Input Component - Generalized Implementation

## Panoramica

Il componente `LookupInputComponent` è stato generalizzato per supportare **oggetti di risultato generici** anziché solo stringhe, con capacità di **mappatura automatica dei campi** e **auto-popolazione della form**.

### Caratteristiche principali

- ✅ **Generici**: Supporta qualunque tipo di oggetto risultato (`<T = Record<string, any>>`)
- ✅ **Field Mapping**: Mappa dichiarativa i campi del risultato ai controlli della form
- ✅ **Auto-Popolazione**: Riempie automaticamente i campi della form padre
- ✅ **Display Field**: Specifica quale campo del risultato mostrare nella textbox
- ✅ **Nested Paths**: Supporta notazione dot per campi annidati (`"paziente.id"`)
- ✅ **Backward Compatible**: Le lookup basate su stringhe continuano a funzionare senza cambiamenti

---

## Nuove Proprietà

### Input Properties

#### `fieldMappings?: LookupFieldMapping`
**Tipo**: `Record<string, string>` (opzionale)

**Descrizione**: Configura il mapping tra i campi dell'oggetto risultato e i nomi dei controlli della form nel componente padre.

**Formato**: 
```typescript
{
  "nomeFieldRisultato": "nomeControlForm",
  "nomeFieldRisultato2": "nomeControlForm2"
}
```

**Esempio**:
```typescript
fieldMappings = {
  id: 'pazienteId',
  cognome: 'pazienteCognome',
  nome: 'pazienteNome',
  codiceFiscale: 'pazienteCodiceFiscale'
}
```

**Come funziona**: Quando il componente riceve un risultato dalla modale, estrae i campi specificati dall'oggetto e li trasforma in un oggetto flat che può essere passato a `form.patchValue()`.

**Supporta nested paths**: 
```typescript
fieldMappings = {
  'paziente.id': 'pazienteId',           // Estrae result.paziente.id
  'paziente.cognome': 'pazienteCognome'  // Estrae result.paziente.cognome
}
```

---

#### `autoPopulateForm: boolean`
**Tipo**: `boolean` (default: `false`)

**Descrizione**: Quando `true`, il componente **auto-popola automaticamente i campi della form** usando il mapping definito in `fieldMappings`.

**Comportamento**:
- Se `true` + `fieldMappings` definito: auto-patcha i valori nella FormGroup padre
- Se `true` + NO `fieldMappings`: nessun effetto (niente da mappare)
- Se `false`: emette solo l'evento `selectedResult`; il componente padre gestisce manualmente il form

**Esempio**:
```typescript
// Con autoPopulateForm = true, non serve handler:
form.patchValue({...}) // Fatto automaticamente!

// Con autoPopulateForm = false (default), il padre gestisce:
form.patchValue({...}) // Nel handler onAssistitoSelected()
```

---

#### `resultDisplayField?: string`
**Tipo**: `string` (opzionale)

**Descrizione**: Specifica quale campo dell'oggetto risultato deve essere **visualizzato nella textbox** di lookup.

**Comportamento**:
- Se definito: estrae il valore del campo e lo visualizza
- Se non definito: mostra stringa vuota (usato per lookup generic con oggetti complessi)
- Se il risultato è string: ignora `resultDisplayField` e mostra la stringa direttamente

**Supporta nested paths**: Come `fieldMappings`, usa notazione dot per campi annidati.

**Esempio**:
```typescript
// Mostra il cognome nella textbox
resultDisplayField = 'cognome'

// Mostra il nome del paziente da oggetto annidato
resultDisplayField = 'paziente.cognome'
```

**Risultato nel template**:
```
Textbox: [Rossi                    ]  ← Valore di result.cognome
                ^
         riempita da resultDisplayField
```

---

### Output Properties

#### `selectedResult: EventEmitter<LookupSelectionEvent<T>>`
**Tipo**: `EventEmitter<LookupSelectionEvent<T>>`

**Descrizione**: Emette un evento quando la modale chiude con una selezione.

**Payload** (`LookupSelectionEvent<T>`):
```typescript
{
  raw: T,                          // Oggetto risultato originale non modificato
  mapped?: Record<string, unknown>, // Oggetto con i campi mappati (undefined se no fieldMappings)
  closed: boolean                   // true = selezione; false = cancellazione
}
```

**Quando viene emesso**:
- Quando l'utente seleziona un risultato dalla modale
- Emesso prima dell'auto-popolazione (se abilitata)

**Esempio evento ricevuto**:
```typescript
{
  raw: {
    id: 123,
    cognome: 'Rossi',
    nome: 'Mario',
    codiceFiscale: 'RSSMRA80A01H501T'
  },
  mapped: {
    pazienteId: 123,
    pazienteCognome: 'Rossi',
    pazienteNome: 'Mario',
    pazienteCodiceFiscale: 'RSSMRA80A01H501T'
  },
  closed: true
}
```

**Usi**:
- Logging/analytics
- Validazioni aggiuntive
- Elaborazioni custom prima dell'auto-popolazione
- Fallback manuale se `autoPopulateForm = false`

---

## Nuovi Metodi Pubblici

### `emitResult(result: T): void`

**Descrizione**: Metodo pubblico chiamato dal componente della modale per segnalare la selezione di un risultato.

**Cosa fa**:
1. Calcola il mapping (se `fieldMappings` definito)
2. Estrae il display field (se `resultDisplayField` definito)
3. Emette l'evento `selectedResult`
4. Auto-popola la form (se `autoPopulateForm = true` e `fieldMappings` definito)

**Firma**:
```typescript
emitResult(result: T): void
```

**Come usarlo nel template della modale**:
```html
<ng-template appLookupDialogContent let-close="close" let-emitResult="emitResult">
  <app-search-component
    (selected)="emitResult($event); close()"
  ></app-search-component>
</ng-template>
```

---

## Pattern di Utilizzo

### Pattern 1: Backward Compatible (Stringhe)

**Quando usarlo**: Lookup semplici basate su stringhe (es. campo di ricerca testuale).

```typescript
// Component
export class MyComponent {
  form = this.fb.group({
    searchText: ['']
  });
}
```

```html
<!-- Template -->
<app-lookup-input
  formControlName="searchText"
  label="Cerca"
  placeholder="Digita qualcosa..."
>
  <ng-template appLookupDialogContent let-close="close">
    <app-text-search (selected)="close()"></app-text-search>
  </ng-template>
</app-lookup-input>
```

**Risultato**: Funziona esattamente come prima. Nessun mapping, solo stringhe.

---

### Pattern 2: Selezione Manual (Oggetti Complessi)

**Quando usarlo**: Quando vuoi ricevere l'evento e gestire manualmente il form.

```typescript
// Component
export class PuaEditComponent {
  form = this.fb.group({
    pazienteId: [0],
    pazienteCognome: [''],
    pazienteNome: [''],
    pazienteCodiceFiscale: [''],
    pazienteSearch: ['']
  });

  onResultSelected(event: LookupSelectionEvent<AnagraficaSearchResult>): void {
    // Puoi accedere sia al risultato grezzo che ai campi mappati
    console.log('Raw:', event.raw);
    console.log('Mapped:', event.mapped);

    // Logica custom
    if (event.raw.codice === null) {
      this.showConfirmation('Paziente da anagrafe...');
    }

    // Popola manualmente
    this.form.patchValue({
      pazienteId: event.raw.id,
      pazienteCognome: event.raw.cognome,
      pazienteNome: event.raw.nome,
      pazienteCodiceFiscale: event.raw.codiceFiscale
    });
  }
}
```

```html
<!-- Template -->
<app-lookup-input
  formControlName="pazienteSearch"
  label="Assistito"
  placeholder="Cerca assistito..."
  resultDisplayField="cognome"
  (selectedResult)="onResultSelected($event)"
>
  <ng-template appLookupDialogContent let-close="close" let-emitResult="emitResult">
    <app-anagrafica-search
      (selectedAssistito)="emitResult($event); close()"
    ></app-anagrafica-search>
  </ng-template>
</app-lookup-input>
```

**Vantaggi**:
- Controllo totale su cosa fare con il risultato
- Possibilità di validazioni custom
- Logiche complesse prima dell'auto-popolazione

---

### Pattern 3: Auto-Popolazione Dichiarativa (CONSIGLIATO)

**Quando usarlo**: La maggior parte dei casi. Mapping semplici e auto-popolazione.

```typescript
// Component
export class PuaEditComponent {
  form = this.fb.group({
    pazienteId: [0],
    pazienteCognome: [''],
    pazienteNome: [''],
    pazienteCodiceFiscale: [''],
    pazienteDataNascita: [''],
    pazienteSearch: ['']
  });

  // Configurazione dichiarativa
  pazienteFieldMappings = {
    id: 'pazienteId',
    cognome: 'pazienteCognome',
    nome: 'pazienteNome',
    codiceFiscale: 'pazienteCodiceFiscale',
    dataNascita: 'pazienteDataNascita'
  };

  onPazienteSelected(event: LookupSelectionEvent<AnagraficaSearchResult>): void {
    // Log solo per tracking
    console.log('Paziente selezionato:', event.raw.cognome);
  }
}
```

```html
<!-- Template -->
<app-lookup-input
  formControlName="pazienteSearch"
  label="Assistito"
  placeholder="Cerca assistito..."
  dialogTitle="Ricerca Assistito"
  resultDisplayField="cognome"
  [fieldMappings]="pazienteFieldMappings"
  [autoPopulateForm]="true"
  (selectedResult)="onPazienteSelected($event)"
>
  <ng-template appLookupDialogContent let-close="close" let-emitResult="emitResult">
    <app-anagrafica-search
      (selectedAssistito)="emitResult($event); close()"
    ></app-anagrafica-search>
  </ng-template>
</app-lookup-input>
```

**Vantaggi**:
- ✅ Configurazione dichiarativa e chiara
- ✅ Auto-popolazione automatica
- ✅ Meno boilerplate nel componente
- ✅ Facile da manutenere
- ✅ Riusabile (copia il mapping per altri campi simili)

---

### Pattern 4: Nested Objects con Dot Notation

**Quando usarlo**: Il risultato ha oggetti annidati.

```typescript
// Risultato dalla API
interface PazienteSearchResult {
  id: number;
  personaData: {
    cognome: string;
    nome: string;
    dataNascita: string;
  };
  anagrafe: {
    codiceFiscale: string;
    comuneResidenza: string;
  };
}

// Component
pazienteFieldMappings = {
  'id': 'pazienteId',
  'personaData.cognome': 'pazienteCognome',
  'personaData.nome': 'pazienteNome',
  'personaData.dataNascita': 'pazienteDataNascita',
  'anagrafe.codiceFiscale': 'pazienteCodiceFiscale',
  'anagrafe.comuneResidenza': 'pazienteComuneResidenza'
};
```

```html
<!-- Template -->
<app-lookup-input
  resultDisplayField="personaData.cognome"
  [fieldMappings]="pazienteFieldMappings"
  [autoPopulateForm]="true"
  ...
></app-lookup-input>
```

**Come funziona**: Il componente usa `getNestedValue(obj, 'personaData.cognome')` per navigare l'oggetto.

---

### Pattern 5: Auto-Popolazione con Operazioni Custom (IMPORTANTE)

**Quando usarlo**: Vuoi auto-popolare MA prima devi fare controlli, mostrare dialog di conferma, o eseguire logiche custom.

```typescript
// Component
private readonly confirm = inject(ConfirmationService);

pazienteFieldMappings = {
  id: 'pazienteId',
  cognome: 'pazienteCognome',
  nome: 'pazienteNome'
};

onPazienteSelected(event: LookupSelectionEvent<AnagraficaSearchResult>): void {
  // Check if paziente comes from V_ANAGRAFE_ASSISTITI (codice è empty)
  if (!event.raw.codice || event.raw.codice.trim() === '') {
    // Show confirmation dialog FIRST
    this.confirm.confirm({
      message: "Paziente da anagrafe assistiti. Verrà creata voce in pazienti.",
      header: 'Conferma',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        // Utente confermato: ora auto-popola
        // (component lo ha già fatto! Non fare nulla, o aggiungi logica custom)
        console.log('Paziente creato da anagrafe');
      },
      reject: () => {
        // Utente annulla: SVUOTA i campi già auto-popolati
        this.form.patchValue({
          pazienteId: 0,
          pazienteCognome: '',
          pazienteNome: ''
        });
        this.clearLookupValue();
      }
    });
  }
}

private clearLookupValue(): void {
  // Se serve, svuota il lookup input
  this.form.patchValue({ pazienteSearch: '' });
}
```

```html
<!-- Template -->
<app-lookup-input
  formControlName="pazienteSearch"
  resultDisplayField="cognome"
  [fieldMappings]="pazienteFieldMappings"
  [autoPopulateForm]="true"
  (selectedResult)="onPazienteSelected($event)"
>
  <ng-template appLookupDialogContent let-close="close" let-emitResult="emitResult">
    <app-anagrafica-search
      (selectedAssistito)="emitResult($event); close()"
    ></app-anagrafica-search>
  </ng-template>
</app-lookup-input>
```

**Flusso di esecuzione**:

```
1. Utente seleziona risultato
   ↓
2. emitResult() EMETTE selectedResult event + AUTO-POPOLA form
   ↓
3. onPazienteSelected() viene chiamato (riceve event)
   ↓
4. Puoi fare controlli su event.raw
   ↓
5. Se confermi: form rimane popolato ✅
   Se rifiuti: SVUOTA manualmente i campi ❌
```

**Vantaggi**:
- ✅ Auto-popolazione riduce boilerplate
- ✅ Puoi intercettare e fare controlli
- ✅ Mostra dialog di conferma all'utente
- ✅ Se rifiuta, svuoti i campi

**Alternativa se vuoi controllo totale**:

Se le operazioni sono **async** (es. API call per validare), usa `autoPopulateForm = false`:

```typescript
async onPazienteSelected(event: LookupSelectionEvent<AnagraficaSearchResult>): Promise<void> {
  // Validazione async
  const isValid = await this.validatePaziente(event.raw.id);
  
  if (!isValid) {
    this.msg.add({ severity: 'error', detail: 'Paziente non valido' });
    return;
  }

  // Solo se valido, popola manualmente
  this.form.patchValue(event.mapped!);
}
```

```html
<!-- Template con autoPopulateForm = FALSE -->
<app-lookup-input
  [fieldMappings]="pazienteFieldMappings"
  [autoPopulateForm]="false"
  (selectedResult)="onPazienteSelected($event)"
  ...
></app-lookup-input>
```

---

## Flusso Completo di Esecuzione

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Utente clicca bottone "Cerca" (openDialog)               │
│    - Modale si apre                                          │
└────────────────────────┬────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. Utente seleziona risultato nella modale                  │
│    - (selectedAssistito)="emitResult($event); close()"      │
│    - Chiama emitResult(result)                              │
└────────────────────────┬────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. emitResult() esegue SINCRONO:                            │
│    a) Estrae displayValue da result (resultDisplayField)    │
│    b) Calcola mapped usando fieldMappings                   │
│    c) Aggiorna this.value e this.displayValue               │
│    d) Chiama onChange(result) per form control              │
│    e) Emette selectedResult event (parent riceve)           │
│    f) [SE autoPopulateForm=true] Auto-popola form padre     │
└────────────────────────┬────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Parent riceve selectedResult event                       │
│    - onPazienteSelected(event) viene chiamato               │
│    - Puoi accedere: event.raw, event.mapped, event.closed   │
│    - Form è GIÀ STATA auto-popolata (se enabled)            │
│    - Puoi fare controlli/validazioni su event.raw           │
└────────────────────────┬────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Parent può reagire:                                      │
│    - Se ok: lascia form com'è (auto-populate ha fatto)      │
│    - Se fail: svuota manualmente i campi                    │
│    - Mostra dialog di conferma                              │
└─────────────────────────────────────────────────────────────┘
```

### Timing: Quando succede cosa?

| Momento | Evento | Cosa succede | autoPopulateForm=true | autoPopulateForm=false |
|---------|--------|------------|---|---|
| T0 | `emitResult()` chiamato | Estrae mapping | ✅ Auto-popola form | ❌ NO auto-populate |
| T1 | `selectedResult` emesso | Parent riceve event | ✅ Form già popolato | ❌ Form vuoto |
| T2 | Handler `onSelected()` eseguito | Parent fa controlli | ✅ Controlla form già pieno | ❌ Parent popola se ok |
| T3 | Dialogs/confirmations | Parent mostra UI | ✅ User vede dati pre-riempiti | ❌ User vede form vuoto |

### Key point: L'auto-popolazione è sincrona!

```typescript
emitResult(result: T): void {
  // ... mapping e validazione ...
  this.selectedResult.emit(event);  // ← Emette qui
  
  // ↓ Form si auto-popola IMMEDIATAMENTE
  if (this.autoPopulateForm && mapped) {
    control.parent.patchValue(mapped, { emitEvent: false });
  }
}
```

Quindi quando `onSelected()` riceve l'evento, **il form è già stato auto-popolato**.

---

## Flusso Completo di Esecuzione

---

## Tipi Esportati

### `LookupSelectionEvent<T>`

```typescript
interface LookupSelectionEvent<T = Record<string, any>> {
  raw: T;                          // Oggetto originale
  mapped?: Record<string, unknown>; // Campi mappati (se fieldMappings definito)
  closed: boolean;                  // true = selezione, false = cancellazione
}
```

### `LookupFieldMapping`

```typescript
type LookupFieldMapping = Record<string, string>;
// Esempio: { id: 'pazienteId', cognome: 'pazienteCognome' }
```

---

## Compatibilità

### Backward Compatibility ✅

Il componente mantiene **100% backward compatibility** con l'utilizzo precedente:

```html
<!-- Questo continua a funzionare esattamente come prima -->
<app-lookup-input
  formControlName="searchText"
  label="Cercare"
>
  <ng-template appLookupDialogContent let-close="close">
    <app-search></app-search>
  </ng-template>
</app-lookup-input>
```

Le nuove proprietà (`fieldMappings`, `autoPopulateForm`, `resultDisplayField`) sono tutte **opzionali**.

---

## Best Practices

### ✅ DO

1. **Definisci fieldMappings come property di classe** per riusabilità
   ```typescript
   pazienteFieldMappings = { id: 'pazienteId', ... };
   ```

2. **Usa resultDisplayField** per fornire feedback visivo all'utente
   ```html
   resultDisplayField="cognome"
   ```

3. **Usa autoPopulateForm = true** per semplificare il codice
   ```html
   [autoPopulateForm]="true"
   ```

4. **Passa emitResult nel template** per segnalare la selezione
   ```html
   <ng-template appLookupDialogContent let-emitResult="emitResult">
     (selected)="emitResult($event); close()"
   </ng-template>
   ```

5. **Usa nested paths** se il risultato ha oggetti annidati
   ```typescript
   'paziente.id': 'pazienteId'
   ```

### ❌ DON'T

1. Non dimenticare `emitResult()` nel template della modale
   ```html
   <!-- ❌ Sbagliato - modale non sa come inviare il risultato -->
   <app-search (selected)="close()"></app-search>

   <!-- ✅ Giusto -->
   <app-search (selected)="emitResult($event); close()"></app-search>
   ```

2. Non usare `autoPopulateForm = true` senza `fieldMappings`
   ```typescript
   // ❌ Non fa nulla
   [autoPopulateForm]="true"
   <!-- fieldMappings non definito -->

   // ✅ Sempre definisci il mapping
   [fieldMappings]="mapping"
   [autoPopulateForm]="true"
   ```

3. Non chiamare `emitResult()` manualmente dal componente padre
   ```typescript
   // ❌ Sbagliato - è responsabilità della modale
   this.emitResult(result);

   // ✅ La modale chiama emitResult, il padre riceve selectedResult
   (selectedResult)="onSelected($event)"
   ```

---

## Migrare un Lookup Esistente

### Prima (Pattern Manuale)

```typescript
onAssistitoSelected(paziente: AnagraficaSearchResult, close: () => void): void {
  close();
  this.form.patchValue({
    pazienteId: paziente.id,
    pazienteCognome: paziente.cognome,
    pazienteNome: paziente.nome,
    pazienteCodiceFiscale: paziente.codiceFiscale,
    pazienteDataNascita: paziente.dataNascita ?? ''
  });
}
```

```html
<ng-template appLookupDialogContent let-close="close">
  <app-anagrafica-search
    (selectedAssistito)="onAssistitoSelected($event, close)"
  ></app-anagrafica-search>
</ng-template>
```

### Dopo (Pattern Dichiarativo)

```typescript
pazienteFieldMappings = {
  id: 'pazienteId',
  cognome: 'pazienteCognome',
  nome: 'pazienteNome',
  codiceFiscale: 'pazienteCodiceFiscale',
  dataNascita: 'pazienteDataNascita'
};

onAssistitoSelected(event: LookupSelectionEvent<AnagraficaSearchResult>): void {
  // Log only if needed
  console.log('Assistito:', event.raw.cognome);
}
```

```html
<app-lookup-input
  resultDisplayField="cognome"
  [fieldMappings]="pazienteFieldMappings"
  [autoPopulateForm]="true"
  (selectedResult)="onAssistitoSelected($event)"
>
  <ng-template appLookupDialogContent let-close="close" let-emitResult="emitResult">
    <app-anagrafica-search
      (selectedAssistito)="emitResult($event); close()"
    ></app-anagrafica-search>
  </ng-template>
</app-lookup-input>
```

**Benefici della migrazione**:
- 🎯 Codice più pulito e dichiarativo
- 📉 Meno logica nel componente
- 🔄 Riutilizzabile (copy-paste il mapping per altri campi)
- 🐛 Meno errori di mappatura manuale
- ✨ Auto-popolazione automatica

---

## Troubleshooting

### "Il campo non si popola"

**Causa**: `autoPopulateForm` è `false` (default) e non hai un handler per `selectedResult`.

**Soluzione**:
```html
<!-- Opzione 1: Abilita auto-popolazione -->
[autoPopulateForm]="true"

<!-- Opzione 2: Gestisci manualmente -->
(selectedResult)="onSelected($event)"
```

### "displayValue è vuoto"

**Causa**: `resultDisplayField` non è definito.

**Soluzione**:
```html
<!-- Specifica quale campo mostrare -->
resultDisplayField="cognome"
resultDisplayField="personaData.nome"
```

### "Nested field non trovato"

**Causa**: Il path nella notazione dot non esiste nell'oggetto.

**Soluzione**: Verifica che il path esista e sia corretto:
```typescript
// Oggetto
{ paziente: { id: 123 } }

// ✅ Giusto
'paziente.id'

// ❌ Sbagliato
'person.id'
'patient.id'
```

### "La form non si popola anche con autoPopulateForm=true"

**Causa**: Il mapping ha nomi di controlli sbagliati.

**Soluzione**: Verifica che i nomi nel mapping corrispondono ai nomi nel form group:
```typescript
// Form group
form = this.fb.group({
  pazienteId: [...],      // ← Nome corretto
  pazienteCognome: [...]  // ← Nome corretto
});

// Mapping
fieldMappings = {
  id: 'pazienteId',       // ✅ Deve corrispondere
  cognome: 'pazienteCognome' // ✅ Deve corrispondere
}
```

---

## API Reference

| Proprietà | Tipo | Default | Descrizione |
|-----------|------|---------|-------------|
| `fieldMappings` | `Record<string, string>` | `undefined` | Mappa risultato → form |
| `autoPopulateForm` | `boolean` | `false` | Auto-popola form con mapping |
| `resultDisplayField` | `string` | `undefined` | Campo da mostrare in textbox |
| `selectedResult` | `EventEmitter<LookupSelectionEvent<T>>` | - | Evento selezione risultato |

| Metodo | Parametri | Ritorno | Descrizione |
|--------|-----------|---------|-------------|
| `emitResult(result)` | `T` | `void` | Segnala risultato selezionato |

---

## Vedi anche

- [lookup-input.component.ts](lookup-input.component.ts)
- [lookup.types.ts](lookup.types.ts)
- [pua-edit.component.ts](../../../features/pua/pua-edit/pua-edit.component.ts) — Esempio di utilizzo
