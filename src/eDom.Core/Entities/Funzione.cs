using eDom.Core.Interfaces;

namespace eDom.Core.Entities;

public class Funzione : IAuditableEntity
{
    public int Id { get; set; }
    public int ProcedureId { get; set; }
    public string Codice { get; set; } = string.Empty;
    public string Descrizione { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int UtenteInserimento { get; set; }
    public DateTime DataInserimento { get; set; }
    public int? UtenteModifica { get; set; }
    public DateTime? DataModifica { get; set; }
    public DateTime? Version { get; set; }
    public string? Sort { get; set; }

    // ── Navigazione ───────────────────────────────────────────────────────────
    public ICollection<Funzione> Figlie { get; set; } = [];
    public Funzione? Padre { get; set; }
    public Procedura? Procedura { get; set; }
    public ICollection<RuoloFunzione> RuoliFunzione { get; set; } = [];
}
