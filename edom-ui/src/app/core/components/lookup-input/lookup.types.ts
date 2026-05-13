/**
 * Event emitted when a lookup dialog closes with a selected result.
 * @template T - The type of the result object from the search dialog
 */
export interface LookupSelectionEvent<T = Record<string, any>> {
  /**
   * The raw result object from the search dialog (unmodified)
   */
  raw: T;

  /**
   * The mapped result object (field names transformed according to fieldMappings config).
   * Undefined if no fieldMappings were provided.
   */
  mapped?: Record<string, unknown>;

  /**
   * Whether the dialog was closed (true) or cancelled (false)
   */
  closed: boolean;
}

/**
 * Configuration for mapping result object fields to form control names.
 * Example: { pazienteId: 'pazienteId', cognome: 'pazienteCognome', nome: 'pazienteNome' }
 * Maps result.pazienteId → form.pazienteId, result.cognome → form.pazienteCognome, etc.
 */
export type LookupFieldMapping = Record<string, string>;
