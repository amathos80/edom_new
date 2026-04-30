using eDom.Core.Interfaces;

namespace eDom.Core.Entities;

public class RuoloFunzione : IAuditableEntity
{
    public int Id { get; set; }
    public int RuoloId { get; set; }
    public int FunzioneId { get; set; }
    public int RuoloProcedureId { get; set; }
    public int FunzioneProcedureId { get; set; }
    public int UtenteInserimento { get; set; }
    public DateTime DataInserimento { get; set; }
    public int? UtenteModifica { get; set; }
    public DateTime? DataModifica { get; set; }
    public DateTime? Version { get; set; }

    // ── Navigazione ───────────────────────────────────────────────────────────
    public Ruolo? Ruolo { get; set; }
    public Funzione? Funzione { get; set; }
    public Procedura? RuoloProcedura { get; set; }
    public Procedura? FunzioneProcedura { get; set; }
}
