# Plan: Conversione eDom/HICT → Angular 18 + .NET 8

**TL;DR** — L'app è un gestionale healthcare italiano (ASP.NET WebForms + MVP + DevExpress + Oracle). La conversione richiede: un backend .NET 8 con EF Core multi-database (Oracle/SQL Server/PostgreSQL), un'API REST JWT-protected, e un frontend Angular 18 con PrimeNG (sostituto diretto di DevExpress). Il focus iniziale è la struttura UI Angular, poi i servizi backend.

**Root progetto nuovo:** `c:\repo_infonext\Edom_new`

---

## Fase 1 — Struttura Soluzione (fondamenta)

**Cartella root:** `c:\repo_infonext\Edom_new`

1. Creare soluzione .NET 8 con 4 progetti:
   - `eDom.Core` — domain models + interfacce repository (migra da HCTModel + HCTCommon)
   - `eDom.Infrastructure` — EF Core 8 + provider multi-database + repository concreti
   - `eDom.Application` — Services + DTOs + AutoMapper 12 profiles (migra da HICT.Services)
   - `eDom.Api` — .NET 8 Web API, JWT, Swagger, CORS, global exception handler
2. Creare progetto Angular 18 standalone (`ng new edom-ui --standalone --routing --style=scss`) con PrimeNG 17, `@auth0/angular-jwt`, Angular Material per layout

**Struttura cartelle:**
```
c:\repo_infonext\Edom_new\
├── src\
│   ├── eDom.Core\
│   ├── eDom.Infrastructure\
│   ├── eDom.Application\
│   └── eDom.Api\
├── edom-ui\          ← Angular 18
└── eDom.sln
```

---

## Fase 2 — Angular UI *(focus principale)*

### Struttura Feature Modules — mappa 1:1 con le view esistenti

| Modulo Angular | Route | Sorgente WebForms | Componenti chiave |
|---|---|---|---|
| `AuthModule` | `/login` | Login.aspx | `LoginComponent`, `ChangePasswordComponent` |
| `DashboardModule` | `/` | Default.aspx + `ucDefaultPage/` | 7 widget card (PUA, UVM, Casi, Sospensioni, Rivalutazioni, SIAD, MMG) |
| `PazientiModule` | `/pazienti` | Pazienti.aspx + ModificaPaziente.aspx | `PazientiListComponent` (griglia), `PazienteFormComponent` (tab multi-sezione) |
| `CartellaModule` | `/cartella/:id` | Paziente.master + Views/Cartella/ | Shell con sidenav, poi: Riepilogo, Diagnosi, SVAMA, PAI, Interventi, Scale (KPS/PAPS), SIAD, Chiusura caso |
| `CasiModule` | `/casi` | Casi.aspx + CasiSemplici.aspx | `CasiListComponent`, `CasoDetailComponent` |
| `PuaModule` | `/pua` | Pua/NuovoPua/ModificaPua/VisualizzaPua.aspx | `PuaListComponent`, `PuaFormComponent` |
| `UvmModule` | `/uvm` | Segnalazioni, AccettazioneUVM, NuovaAccettazioneUvm | `SegnalazioniComponent`, `AccettazioneFormComponent` |
| `PaiModule` | `/pai` | NuovoCPPai, NuovoAdiPai, NuovoCsPai, NuovoRSPai (e varianti) | Form separati per ADI/CP/CS/RS |
| `InterventiModule` | `/interventi` | Interventi, NuovoIntervento | `InterventiListComponent`, `InterventoFormComponent` |
| `SiadModule` | `/siad` | Siad, CreaFlussiSiad, ElencoSiad | `SiadListComponent`, `CreaFlussiComponent` |
| `RichiesteModule` | `/richieste` | RichiesteMMG, MMGCP, RichiesteCP, SchedeValCP | Sottomoduli per AD e CP |
| `SistemaModule` | `/sistema` | Ruoli, Utenti, Gruppi, StoricoLog | CRUD completo roles/users/groups + audit log |
| `TabelleModule` | `/tabelle` | TabelleGeneriche + tutte le CO_* | Presidi, Medici, Personale, Consultori, Commissioni |
| `StampeModule` | `/stampe` | Views/Stampe/ + Views/Pivots/ | Report con `p-table` + export Excel/PDF |
| `SharedModule` | *(globale)* | `uc/` user controls + Search popups | Dialoghi riutilizzabili (vedi sotto) |

### Componenti Shared critici (dalla folder `uc/` + popup di ricerca)

| Componente Angular | Origine |
|---|---|
| `PatientSearchDialogComponent` | PazientiSearch + PazientiHandler.ashx |
| `MediciSearchDialogComponent` | MediciSearch |
| `ComuniSearchDialogComponent` | ComuniSearch |
| `DiagnosiIcdSearchDialogComponent` | DiagnosiICDSearch |
| `PersonaleSearchDialogComponent` | PersonaleSearch |
| `CasiSearchDialogComponent` | CasiSearch |
| `ConfirmDialogComponent` | ucMessageBoxConfirm.ascx |
| `AlertDialogComponent` | ucMessageBoxAlert.ascx |
| `PatientDatiGeneraliComponent` | ucDatiGenerali.ascx |
| `PatientAddressComponent` | ucResidenza.ascx + ucDomicilio.ascx |
| `AssessmentScaleComponents` (KPS, PAPS, SVAMA) | ucKPS.ascx, ucPAPS.ascx, ucSVAMA.ascx |

### Mapping DevExpress → PrimeNG

| DevExpress | PrimeNG |
|---|---|
| `ASPxGridView` | `p-table` |
| `ASPxNavBar` (side menu) | `p-panelMenu` |
| `ASPxMenu` (top menu) | `p-menubar` |
| `ASPxPopupControl` | `p-dialog` |
| `ASPxDateEdit` | `p-calendar` |
| `ASPxComboBox` | `p-dropdown` |
| `ASPxSpinEdit` | `p-inputnumber` |
| Pivot Grid | `p-table` + groupBy |

### Navigation XML → Angular

- TopMenu.xml → `p-menubar` con permission filtering via Angular `PermissionDirective`
- SideMenu.xml → `p-panelMenu` nella shell del `CartellaModule`
- `PermissionDirective` (`*hasPermission="'CODICE_FUNZIONE'"`) — nasconde elementi non autorizzati (porta logica da `ControlAuthorizationAdapter` in SecurityAdapter.cs)

---

## Fase 3 — Backend .NET Core API

### EF Core 8 — Strategia Multi-Database

Supporto contemporaneo per **Oracle**, **SQL Server** e **PostgreSQL** tramite astrazione del provider EF Core.

**Approccio: provider selezionabile da configurazione**
- `appsettings.json` → `"DatabaseProvider": "Oracle" | "SqlServer" | "Postgres"`
- `Program.cs` legge il provider e registra il `DbContext` con il driver corretto
- Un unico `HctDbContext : DbContext` con Fluent API provider-agnostica
- Mappings specifici (sequence vs identity, tipi colonna, sysdate vs GETDATE/NOW()) centralizzati in classi `IEntityTypeConfiguration<T>` con `if (database.IsOracle())` guards

**Provider NuGet da includere:**
| DB | Package |
|---|---|
| Oracle | `Oracle.EntityFrameworkCore` 8.x |
| SQL Server | `Microsoft.EntityFrameworkCore.SqlServer` 8.x |
| PostgreSQL | `Npgsql.EntityFrameworkCore.PostgreSQL` 8.x |

**Punti critici di adattamento:**
- **Sequenze Oracle** (`Dbutils.GetSequence()`) → `UseHiLo()` su SQL Server / PostgreSQL oppure `IDENTITY` columns
- **`sysdate`** (Oracle) → `GETDATE()` (SQL Server) / `NOW()` (PostgreSQL) — usare `DateTime.UtcNow` in C# e lasciare EF Core tradurre
- **Tipi colonna**: `VARCHAR2` → `nvarchar`/`text`, `NUMBER` → `decimal`/`int` — gestiti con `.HasColumnType()` condizionale
- **Schema `HICT`** (Oracle) → schema separabile tramite `modelBuilder.HasDefaultSchema(config["DbSchema"])`
- **Audit `SaveChanges()`** — reimplementa con `ICurrentUserService`, indipendente dal provider
- Lifetime context: `Scoped` via DI (sostituisce `WebContextModule` + `HttpContext.Items`)

**Scaffolding iniziale su Oracle** (DB di partenza):
```
dotnet ef dbcontext scaffold "..." Oracle.EntityFrameworkCore \
  --schema HICT --output-dir ../eDom.Core/Entities --no-onconfiguring
```
Poi adattare le entity configurations per renderle provider-agnostiche.

### Repository Layer

- Mantieni le ~85 interfacce `I*Repository`
- Reimplementa `BaseRepository<T>` su `DbSet<T>` EF Core (era `IDbSet<T>`)
- `ApplicationContext` → rimpiazzato da DI diretta nei controller

### Authentication JWT (sostituisce sessione custom da LoginPresenter)

- `POST /api/auth/login` — logica da `LoginPresenter.VerifyLogin()`, ritorna JWT + refresh token
- Claims JWT: `userId`, `ruoloId`, `functions[]` (codici funzione), `presidi[]`, `aree[]`
- `POST /api/auth/change-password`, `POST /api/auth/refresh`

### API Controllers

| Controller | Endpoints principali |
|---|---|
| `AuthController` | login, change-password, refresh |
| `PazientiController` | CRUD + storico (`CO_PAZIENTI_ST`) |
| `CasiController` | CRUD + sub-entities |
| `PuaController` | CRUD + numerazione |
| `UvmController` | CRUD + verbali PDF |
| `PaiController` | CRUD ADI/CP/CS/RS + calcolo costi (`ADI_PAI_TIPOCALCOLO`) |
| `InterventiController` | CRUD |
| `SiadController` | CRUD + genera flussi XML (logica da `FlussiSiadService`) |
| `RichiesteController` | MMG-AD, MMG-CP, CP |
| `SistemaController` | Utenti, Ruoli, Funzioni, Gruppi, Log |
| `TabelleController` | Lookup CO_* + SI_TABCONF |
| `StampeController` | Report PDF/Excel |
| `ValidationController` | Micro-validazioni (porta da ValidationsController) |

### Cross-cutting

- `MaintenanceMiddleware` — controlla `SI_TABCONF.MANUTENZIONE=1`, ritorna 503
- `[RequirePermission("CODICE")]` attribute custom su action controller

---

## File chiave di riferimento

- HICT/App_Data/TopMenu.xml + SideMenu.xml — struttura menu
- HCTCommon/Presenters/ — logica business da spostare nei Services
- HCTCommon/ViewModels/ — proprietà form da trasformare in Angular Reactive Forms
- HICT.Services/DTO/ — contratto DTO da adattare come response API
- HCTCommon/SecurityCheck.cs + HICT/SecurityAdapter.cs — logica permission da portare in JWT claims + PermissionDirective

---

## Verification

1. Fase 1: Angular compila senza errori, `GET /health` ritorna 200, EF Core si connette a Oracle
2. Fase 2: ogni route Angular carica correttamente, griglie mostrano dati mock, dialog di ricerca funzionano, permessi nascondono elementi non autorizzati
3. Fase 3: Swagger documenta tutti gli endpoint, dati reali da Oracle, JWT rilasciato al login
4. End-to-end: login → dashboard → lista pazienti → cartella assistito → PAI completo, tutto con permessi applicati

---

## Decisioni

| # | Decisione | Scelta |
|---|---|---|
| 1 | **Libreria UI Angular** | **PrimeNG 17** ✅ |
| 2 | **Database** | **Multi-DB**: Oracle + SQL Server + PostgreSQL — provider selezionabile da config ✅ |
| 3 | **Ordine implementazione UI** | Auth → Dashboard → PazientiModule → CartellaModule ✅ |
| 4 | **Root progetto** | `c:\repo_infonext\Edom_new` ✅ |
| 5 | **Architettura** | **Clean Architecture + Vertical Slice** — i layer `Core / Infrastructure / Application / Api` rispettano Clean Architecture; all'interno di `Application` e `Api` il codice è organizzato per **feature slice verticale** (`Features/<FeatureName>/`) contenente Commands, Queries, Handlers (CQRS via MediatR), Validators (FluentValidation) e DTOs, evitando cartelle orizzontali monolitiche ✅ |

---

## Architettura — Clean Architecture + Vertical Slice (decisione 5)

### Struttura cartelle `eDom.Application`

```
eDom.Application/
└── Features/
    ├── Patients/
    │   ├── Commands/
    │   │   ├── CreatePatient/
    │   │   │   ├── CreatePatientCommand.cs
    │   │   │   ├── CreatePatientCommandHandler.cs
    │   │   │   └── CreatePatientCommandValidator.cs
    │   │   └── UpdatePatient/
    │   │       ├── UpdatePatientCommand.cs
    │   │       ├── UpdatePatientCommandHandler.cs
    │   │       └── UpdatePatientCommandValidator.cs
    │   ├── Queries/
    │   │   ├── GetPatientById/
    │   │   │   ├── GetPatientByIdQuery.cs
    │   │   │   ├── GetPatientByIdQueryHandler.cs
    │   │   │   └── PatientDetailDto.cs
    │   │   └── GetPatientsList/
    │   │       ├── GetPatientsListQuery.cs
    │   │       ├── GetPatientsListQueryHandler.cs
    │   │       └── PatientSummaryDto.cs
    │   └── Common/
    │       └── PatientMappingProfile.cs
    ├── Auth/
    ├── Cases/
    ├── Pua/
    ├── Uvm/
    └── ...
```

### Struttura cartelle `eDom.Api`

```
eDom.Api/
└── Features/
    ├── Patients/
    │   └── PatientsController.cs   ← thin controller: riceve HTTP, dispatcha via MediatR
    ├── Auth/
    │   └── AuthController.cs
    └── ...
```

### Regole architetturali

- **Dipendenze mono-direzionali:** `Api` → `Application` → `Core`; `Infrastructure` implementa interfacce di `Core`; nessun layer riferisce il layer superiore.
- **CQRS con MediatR:** ogni operazione è un `IRequest<T>` separato (Command o Query); niente logica nei controller.
- **FluentValidation:** ogni Command/Query ha il proprio `AbstractValidator<T>`, registrato via `AddValidatorsFromAssembly`.
- **No cartelle orizzontali in Application:** vietate cartelle `/Services/`, `/DTOs/`, `/Mappings/` a radice — tutto dentro la slice.
- **Angular — mirroring verticale:** la struttura `edom-ui/src/app/features/<feature>/` rispecchia le slice backend; ogni feature contiene `components/`, `services/`, `models/`, `store/` (NgRx slice se necessario).
- **Core puro:** `eDom.Core` contiene solo entità di dominio, value objects, interfacce repository e domain events — zero dipendenze da NuGet applicativi.
