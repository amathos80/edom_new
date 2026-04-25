namespace eDom.Core.Entities;

/// <summary>
/// Storico delle modifiche. Mappa su SI_AUDIT_LOG.
/// Ogni INSERT/UPDATE/DELETE su entità IAuditableEntity genera un record.
/// </summary>
public class AuditLog
{
    /// <summary>AULO_ID — PK auto-increment.</summary>
    public long Id { get; set; }

    /// <summary>AULO_TABELLA — Nome del tipo entità EF (es. "Paziente").</summary>
    public string Tabella { get; set; } = string.Empty;

    /// <summary>AULO_ENTITA_ID — Valore della PK dell'entità modificata (come stringa).</summary>
    public string EntitaId { get; set; } = string.Empty;

    /// <summary>AULO_OPERAZIONE — "INS", "UPD" o "DEL".</summary>
    public string Operazione { get; set; } = string.Empty;

    /// <summary>AULO_UTEN_ID — ID dell'utente autenticato che ha eseguito l'operazione.</summary>
    public int? UtenteId { get; set; }

    /// <summary>AULO_DTOP — Timestamp UTC dell'operazione.</summary>
    public DateTime DataOperazione { get; set; }

    /// <summary>AULO_OLD_VALUES — JSON dei valori prima della modifica. Null per INS.</summary>
    public string? ValoriPrecedenti { get; set; }

    /// <summary>AULO_NEW_VALUES — JSON dei valori dopo la modifica. Null per DEL.</summary>
    public string? ValoriNuovi { get; set; }
}
