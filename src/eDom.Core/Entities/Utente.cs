using eDom.Core.Interfaces;

namespace eDom.Core.Entities;

public class Utente : IAuditableEntity
{
    public int Id { get; set; }
    public string Codice { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Cognome { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? CodiceFiscale { get; set; }
    public string? Email { get; set; }
    public string? Matricola { get; set; }

    // ── Flag ──────────────────────────────────────────────────────────────────
    /// <summary>UTEN_F_CHPASS: 1 = deve cambiare password al prossimo login.</summary>
    public short FlagCambiaPwd { get; set; }
    /// <summary>UTEN_F_SMCARD: 1 = accesso tramite smart card.</summary>
    public short FlagSmartCard { get; set; }

    // ── Date operative ────────────────────────────────────────────────────────
    public DateTime? DataDisattivazione { get; set; }
    public DateTime? DataRiattivazione { get; set; }
    public DateTime? DataScadenzaPassword { get; set; }
    public DateTime? UltimoLogin { get; set; }

    // ── Audit ─────────────────────────────────────────────────────────────────
    public int UtenteInserimento { get; set; }
    public DateTime DataInserimento { get; set; }
    public int? UtenteModifica { get; set; }
    public DateTime? DataModifica { get; set; }
    public DateTime? Version { get; set; }

    // ── Navigazione ───────────────────────────────────────────────────────────
    public ICollection<Ruolo> Ruoli { get; set; } = [];

    // ── Computed (non persistito) ─────────────────────────────────────────────
    public string NomeCompleto => $"{Nome} {Cognome}".Trim();
    public bool IsAttivo => DataDisattivazione is null || DataDisattivazione > DateTime.UtcNow;
}
