namespace eDom.Core.Entities;

/// <summary>Parametro di configurazione applicativa. Mappa su SI_CONFIG.</summary>
public class Configurazione
{
    public int Id { get; set; }
    public int ProcedureId { get; set; }
    public string Codice { get; set; } = string.Empty;
    public string? Valore { get; set; }
    public string? Descrizione { get; set; }
}
