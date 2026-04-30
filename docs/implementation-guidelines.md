# Linee Guida Implementative (Backend + Frontend)

## 1. Obiettivo del documento
Questo documento riassume le linee guida applicate finora nel progetto `edom_new` e serve come base estendibile per le prossime implementazioni.

La struttura e suddivisa in:
- linee guida generiche Backend
- linee guida specifiche per slice Backend gia implementate
- linee guida generiche Frontend
- linee guida specifiche per slice Frontend gia implementate
- checklist operativa per nuove slice

## 2. Backend

### 2.1 Linee guida generiche

#### Architettura e layering
- Usare il pattern verticale per feature in `src/eDom.Application/Features/<FeatureName>/...`.
- Esposizione endpoint in controller API in `src/eDom.Api/Controllers`.
- Accesso dati tramite `IRepository<T>` o repository specializzati (es. `IUtenteRepository`).
- Regola base: controller sottile, logica nel layer Application (handler).

#### Naming e organizzazione
- Query: `<Azione><Entita>Query`.
- Command: `<Azione><Entita>Command`.
- Handler nello stesso file o sottofolder dedicato della slice.
- DTO condiviso della slice in file dedicato (es. `RuoloDto`, `UtenteDto`).

#### Contratti API
- Usare route REST consistenti:
  - `GET /api/<entita>` ricerca/lista
  - `POST /api/<entita>` creazione
  - `PUT /api/<entita>/{id}` aggiornamento
  - `DELETE /api/<entita>/{id}` cancellazione
  - azioni specifiche su route annidate (es. `/{id}/reset-password`, `/{id}/riattiva`, `/{id}/funzioni`)
- Restituire:
  - `200 OK` per get/update con payload
  - `201 Created` per create
  - `204 NoContent` per azioni senza payload
  - `404 NotFound` se record inesistente

#### Regole implementative handler
- Recuperare entita con filtro esplicito su Id quando necessario (`GetAllAsync(... take:1).FirstOrDefault()`) per evitare mismatch di `FindAsync` in scenari gia osservati.
- Applicare `.Trim()` ai campi stringa in ingresso.
- Normalizzare stringhe opzionali a `null` quando vuote.
- Gestire audit su campi `UtenteInserimento`, `DataInserimento`, `UtenteModifica`, `DataModifica`.

#### Sicurezza e autenticazione
- Controller protetti con `[Authorize]` salvo casi espliciti (es. login).
- Operazioni sensibili (reset password, riattivazione) sempre lato backend con endpoint dedicato.

#### Password
- Coerenza con implementazione esistente Auth: SHA512 + Base64.
- Reset password: impostare hash del default e forzare cambio password al login (`FlagCambiaPwd = 1`).

#### Database e mapping
- Rispettare naming colonne legacy (`UTEN_*`, `RUOL_*`, `RUFU_*`, ...).
- Preferire identity PostgreSQL dove richiesto.
- Per reattivazione utente: regola funzionale principale = `DataDisattivazione = null`.

#### Qualita e verifica
- Build backend obbligatoria dopo modifiche (`dotnet build`).
- Non introdurre modifiche distruttive su dati senza endpoint/command dedicati.

### 2.2 Slice Backend specifiche implementate

#### Slice: Ruoli (CRUD + ricerca)
Riferimenti principali:
- `src/eDom.Api/Controllers/RuoliController.cs`
- `src/eDom.Application/Features/Ruoli/*`

Linee guida specifiche:
- Supportare ricerca semplice e paginata.
- Mappare `FlagAdmin` (smallint) in bool nel DTO.
- Arricchire risposta con `ProceduraCodice` quando utile in UI.

#### Slice: Ruoli-Funzioni
Riferimenti principali:
- `src/eDom.Application/Features/Ruoli/GetRuoloFunzioni/*`
- `src/eDom.Application/Features/Ruoli/UpdateRuoloFunzioni/*`
- `src/eDom.Api/Controllers/RuoliController.cs` (`/{id}/funzioni`)

Linee guida specifiche:
- Restituire struttura ad albero (nodi con `Figlie`).
- Constraining per procedura del ruolo (`Funzione.ProcedureId == Ruolo.ProcedureId`).
- Update in logica differenziale (add/remove) su associazioni esistenti.

#### Slice: Utenti
Riferimenti principali:
- `src/eDom.Api/Controllers/UtentiController.cs`
- `src/eDom.Application/Features/Utenti/*`

Linee guida specifiche:
- Esporre CRUD base (lista/create/update/delete).
- Esporre azioni operative:
  - reset password
  - riattivazione utente
- Campi funzionali chiave: `CodiceFiscale`, `DataScadenzaPassword`, `DataDisattivazione`, `DataRiattivazione`, `UltimoLogin`.

## 3. Frontend

### 3.1 Linee guida generiche

#### Struttura
- Feature component standalone in `edom-ui/src/app/features/<feature>/<component>`.
- Model in `edom-ui/src/app/core/models`.
- Service API in `edom-ui/src/app/core/services`.

#### Pattern UI principale
- Pagina gestione con:
  - tabella PrimeNG (`p-table`)
  - ricerca globale
  - filtri colonna (`p-columnFilter`)
  - popup di modifica/creazione (`p-dialog`)
- Operazioni record con pulsanti azione in tabella.

#### Generazione nuove tabelle
- In generale le tabelle devono sempre contenere creazione/aggiornamento/cancellazione entità a meno di differenti indicazioni
- In generale ogni colonna deve avere un filtro compatibile con il tipo di dato a meno che non sia specificato diversamente: 
    - Data
    - Numero
    - Lookup
    - Testo


#### Pattern stato e form
- Usare `signal()` per stato UI (`loading`, `saving`, `dialogVisibile`, selezioni).
- Usare Reactive Forms con validazioni in linea.
- Distinguere modalita `crea` e `modifica` nello stesso popup.

#### Chiamate API e feedback utente
- Service dedicato per entita (metodi `cerca`, `crea`, `aggiorna`, `elimina`, eventuali azioni extra).
- Mostrare toast di successo/errore su tutte le operazioni.
- Ricaricare tabella al termine delle operazioni mutative.

#### Routing e menu
- Aggiungere route in `edom-ui/src/app/app.routes.ts`.
- Aggiungere/aggiornare voce menu in `edom-ui/public/config/top-menu.json` con `routerLink`.

#### Qualita e verifica
- Build frontend obbligatoria (`npm run build`).
- Warning noti tollerati solo se preesistenti o fuori scope.

### 3.2 Slice Frontend specifiche implementate

#### Slice: Gestione Ruoli
Riferimenti:
- `edom-ui/src/app/features/ruoli/ruoli-management/*`
- `edom-ui/src/app/core/services/ruoli.service.ts`

Linee guida specifiche:
- Supporto lazy load/paginazione lato server.
- Toolbar con operazioni principali e reset filtri.

#### Slice: Gestione Ruoli-Funzioni
Riferimenti:
- `edom-ui/src/app/features/ruoli/ruoli-funzioni-management/*`

Linee guida specifiche:
- UX richiesta: tabella ruoli + modifica in popup.
- Popup con albero annidato checkbox e azioni `Espandi/Comprimi/Salva`.

#### Slice: Gestione Utenti
Riferimenti:
- `edom-ui/src/app/features/utenti/utenti-management/*`
- `edom-ui/src/app/core/services/utenti.service.ts`
- `edom-ui/src/app/core/models/utente.model.ts`

Linee guida specifiche:
- Tabella con colonne business richieste:
  - codice, cognome, nome, codice fiscale, email
  - data scadenza password, data disattivazione, data riattivazione
  - data ultimo accesso, stato
- Filtri per colonna su tutte le colonne esposte.
- Popup unico per creazione/modifica.
- Azioni popup: reset password e riattivazione.

## 4. Checklist per nuove implementazioni (estendibile)

### 4.1 Backend checklist
- Creata folder slice in `src/eDom.Application/Features/<NuovaFeature>`.
- Definiti Query/Command/DTO e relativi Handler.
- Aggiunte route controller in `src/eDom.Api/Controllers`.
- Gestiti codici HTTP coerenti (`200/201/204/404`).
- Validata logica business specifica della slice.
- Eseguita build backend.

### 4.2 Frontend checklist
- Creato model in `core/models`.
- Creato/esteso service in `core/services`.
- Creato componente feature standalone con tabella + popup.
- Aggiunti filtri e feedback toast.
- Collegata route e voce menu.
- Eseguita build frontend.

## 5. Convenzioni per estensione documento
- Per ogni nuova slice aggiungere una sottosezione in:
  - `2.2 Slice Backend specifiche implementate`
  - `3.2 Slice Frontend specifiche implementate`
- Mantenere sempre:
  - obiettivo funzionale della slice
  - file di riferimento
  - regole specifiche da rispettare
- Se una regola diventa trasversale, promuoverla da sezione specifica a linea guida generica.
