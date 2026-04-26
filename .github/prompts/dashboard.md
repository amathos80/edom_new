Plan: Dashboard Custom Reorderable
Status: Approved by user on 2026-04-26. Ready for implementation handoff.

Creare una nuova feature dashboard Angular basata su GridStack con widget contenitori dinamici, reorderable e resizable, mantenendo i pattern esistenti del progetto (standalone + lazy loading + registry). Il layout sarà persistito per utente con strategia backend-first e fallback localStorage. Nel MVP drag/resize sempre attivi e catalogo iniziale di 5-6 widget.

Steps

Fase 1 - Fondazioni feature dashboard
Definire il dominio dashboard: tipi per widget, layout, istanza widget, metadata e stato runtime (attivo/rimosso/configurazione). Blocca le fasi successive.
Aggiungere la dipendenza GridStack nel frontend e integrare i CSS necessari a livello feature (evitando side effects globali non necessari).
Creare la struttura feature dashboard con route lazy dedicate e pagina container principale. Parallelizzabile con step 3 dopo definizione tipi.
Fase 2 - Motore layout e persistenza
Implementare DashboardLayoutService con flusso backend-first: load da API, fallback localStorage, save con retry e sincronizzazione stato locale ottimistico.
Definire DashboardWidgetsRegistryService (o registry statico + service wrapper) per risolvere i widget caricabili dinamicamente e le dimensioni default.
Implementare DashboardGridComponent come adapter GridStack: init/destroy, bind eventi drag/resize/change, serializzazione layout e emissione eventi verso la pagina.
Implementare debounce/throttle dei salvataggi layout per evitare storm di richieste durante il drag.
Fase 3 - Contenitori widget e composizione annidata
Implementare DashboardWidgetContainerComponent che ospita componenti custom via caricamento dinamico, con contract input/output standard (config, refresh, error state).
Supportare contenitori annidati: ogni widget container può esporre una zona di composizione interna per altri custom component (slot o host dinamico), con limiti per prevenire nesting infinito nel MVP (es. profondità max 2).
Definire protocollo dati widget: configurazione, datasource, stato loading/error/empty e canale eventi.
Fase 4 - Catalogo widget MVP (5-6)
Creare 5-6 widget iniziali (es. KPI summary, tabella dati, grafico trend, elenco alert, attività recenti, stato sistema) come standalone component lazy-load.
Registrare i widget nel registry dashboard con metadata (titolo, icona, min/max size, default position).
Implementare UI catalogo/add/remove widget nella pagina dashboard mantenendo drag/resize sempre attivi (decisione utente).
Fase 5 - Integrazione shell e routing
Aggiungere route dashboard nell’albero protetto app e voce menu nella shell.
Allineare guard e permessi (se presenti) per l’accesso dashboard.
Fase 6 - Robustezza e qualità
Gestire migrazione versione layout (schema version) per compatibilità futura.
Aggiungere fallback graceful se GridStack non inizializza (render statico read-only).
Coprire test unitari per services core, mapping layout, registry e comportamento container dinamico.
Eseguire test manuali E2E di drag/resize/reload/persistenza e responsive.
Relevant files

app.routes.ts — estendere routing protetto con route lazy dashboard.
shell.component.ts — aggiungere voce menu dashboard e gestione navigazione.
custom-components.registry.ts — riferimento pattern registry lazy già esistente da riusare.
custom-components.routes.ts — riferimento pattern route generation dinamica.
package.json — aggiungere dipendenze GridStack.
Nuovi file previsti sotto /home/alex/repos/edom_new/edom-ui/src/app/features/dashboard/ — pages, components, services, models, registry, examples.
Verification

Build frontend: install dipendenze e build senza errori type-check.
Verifica lazy chunking: route dashboard e widget caricati on-demand (controllo network/chunk).
Verifica UX: drag and drop, resize, reorder multipli, comportamento responsive desktop/tablet/mobile.
Verifica persistenza: refresh pagina mantiene layout; fallback localStorage attivo quando API non raggiungibile.
Verifica composizione annidata: inserimento custom component dentro widget container e rendering corretto.
Verifica resilienza: errori widget isolati senza rompere l’intera dashboard.
Verifica performance: nessun lag evidente durante drag con 5-6 widget attivi.
Decisions

Include: GridStack come motore layout.
Include: drag/resize sempre attivi (senza edit mode nel MVP).
Include: 5-6 widget iniziali nel catalogo.
Include: persistenza layout backend-first con fallback localStorage.
Exclude (MVP): template multipli dashboard per utente, collaboration real-time, RBAC granulare per widget.
Further Considerations

API contract persistenza layout: endpoint dedicato dashboard per utente (raccomandato) vs estensione endpoint preferenze utente esistente.
Nested containers: fissare profondità massima nel MVP per evitare complessità di UX e performance.
Strategia salvataggio: autosave debounce (raccomandato) con eventuale pulsante Save come fallback UX.