using eDom.Core.Interfaces;

namespace eDom.Core.Entities;

public class Ruolo : IAuditableEntity
{
    public int Id { get; set; }
    public int ProcedureId { get; set; }
    public string Codice { get; set; } = string.Empty;
    public string Descrizione { get; set; } = string.Empty;

    // ── Flag ──────────────────────────────────────────────────────────────────
    /// <summary>RUOL_F_ADMIN: 1 = ruolo di amministratore.</summary>
    public short FlagAdmin { get; set; }

    // ── Audit ─────────────────────────────────────────────────────────────────
    public int UtenteInserimento { get; set; }
    public DateTime DataInserimento { get; set; }
    public int? UtenteModifica { get; set; }
    public DateTime? DataModifica { get; set; }
    public DateTime? Version { get; set; }

    // ── Navigazione ───────────────────────────────────────────────────────────
    public ICollection<Utente> Utenti { get; set; } = [];
}
