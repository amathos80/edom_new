/**
 * Struttura del nodo menu — corrisponde al JSON di configurazione.
 * Mappa la gerarchia dell'XML originale (Procedura → Funzione → Gestione).
 */
export interface MenuNode {
  /** Testo visualizzato nel menu */
  text: string;
  /** Identificatore univoco (corrisponde all'attributo name dell'XML) */
  name?: string;
  /** Classe icona PrimeNG (es. 'pi pi-cog') */
  icon?: string;
  /** Codice permesso richiesto per visualizzare la voce */
  permissions?: string;
  /**
   * URL interno Angular (routerLink).
   * Usare questo per le voci con navigazione intra-app.
   */
  routerLink?: string;
  /**
   * URL WebForms legacy (solo riferimento, non usato per la navigazione Angular).
   * Mantenuto per traceability durante la migrazione.
   */
  navigateUrl?: string;
  /**
   * URL esterno. Supporta segnaposto {param} sostituibili a runtime.
   * Usato assieme a `method` e `target`.
   */
  externalUrl?: string;
  /** Parametri da iniettare nei segnaposto {key} di externalUrl */
  externalParams?: Record<string, string>;
  /** Target del link esterno (es. '_blank') */
  target?: string;
  /** Metodo HTTP per la navigazione esterna. Default 'GET'. */
  method?: 'GET' | 'POST';
  /** Codice procedura dell'applicazione originale (riferimento) */
  codiceProcedura?: number;
  /** Voci figlio (gruppo/sottomenu) */
  children?: MenuNode[];
}

/** Radice del file di configurazione menu */
export interface MenuConfig {
  menu: MenuNode[];
}
