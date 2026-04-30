using eDom.Core.Interfaces;

namespace eDom.Core.Entities;

public class Procedura : IAuditableEntity
{
    public int Id { get; set; }
    public string Codice { get; set; } = string.Empty;
    public string Descrizione { get; set; } = string.Empty;
    public int UtenteInserimento { get; set; }
    public DateTime DataInserimento { get; set; }
    public int? UtenteModifica { get; set; }
    public DateTime? DataModifica { get; set; }
    public DateTime? Version { get; set; }
    public string? DbSchema { get; set; }
    public string? DbPassword { get; set; }
    public short Visibile { get; set; }

    // ── Navigazione ───────────────────────────────────────────────────────────
    public ICollection<Funzione> Funzioni { get; set; } = [];
    
}
