using eDom.Core.Interfaces;

namespace eDom.Core.Entities;

public class PuaRecord : IAuditableEntity
{
    public int Id { get; set; }
    public int NumeroPuaId { get; set; }
    public int Numero { get; set; }
    public DateTime Data { get; set; }
    public int AreaInterventoId { get; set; }
    public int PazienteId { get; set; }
    public string PazienteCognome { get; set; } = string.Empty;
    public string PazienteNome { get; set; } = string.Empty;
    public string? PazienteCodiceFiscale { get; set; }
    public int AccessoId { get; set; }
    public string? AccessoNote { get; set; }
    public int? MotivoId { get; set; }
    public string? MotivoNote { get; set; }
    public int RichiestaId { get; set; }
    public string? RichiestaAltro { get; set; }
    public int EsitoId { get; set; }
    public string? EsitoNote { get; set; }
    public short Urgente { get; set; }
    public int OrigineId { get; set; }
    public DateTime DataAvvio { get; set; }
    public DateTime? DataChiusura { get; set; }
    public int? MotivoChiusuraId { get; set; }
    public short Attivo { get; set; }
    public DateTime? DataDisattivazione { get; set; }
    public int UtenteInserimento { get; set; }
    public DateTime DataInserimento { get; set; }
    public int? UtenteModifica { get; set; }
    public DateTime? DataModifica { get; set; }
    public DateTime? Version { get; set; }
}
