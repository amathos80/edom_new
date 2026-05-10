namespace eDom.Core.Entities;

/// <summary>
/// Read-only entity mapped to the V_CO_PAZIENTI view.
/// Only the columns needed for patient search and display are mapped.
/// </summary>
public class VistaPaziente
{
    public int PaziId { get; set; }
    public string? PaziCodice { get; set; }
    public string? PaziCognome { get; set; }
    public string? PaziNome { get; set; }
    public DateTime? PaziDtnas { get; set; }
    public string? PaziCodfisc { get; set; }
    public string? PaziSesso { get; set; }
    public string? PaziEmail { get; set; }
    public string? PaziCodicesanit { get; set; }
    public string? PaziTelef01 { get; set; }
    public string? PaziCapres { get; set; }
    public string? PaziIndres { get; set; }
    public int? PaziMedicoId { get; set; }
    public short? PaziFAtt { get; set; }
    public int? PaziAreresId { get; set; }
}
