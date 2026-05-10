namespace eDom.Core.Models;

/// <summary>
/// Proiezione piatta del paziente con dati denormalizzati (comuni, medico) per il form PUA.
/// </summary>
public class PazientePuaData
{
    public int Id { get; set; }
    public string Codice { get; set; } = string.Empty;
    public string Cognome { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string NomeCompleto => $"{Cognome} {Nome}".Trim();
    public DateOnly DataNascita { get; set; }
    public string? CodiceFiscale { get; set; }
    public string? Sesso { get; set; }
    public string? Email { get; set; }
    public string? Telefono1 { get; set; }
    public string? Telefono2 { get; set; }
    public string? ComuneResidenzaDescr { get; set; }
    public string? IndirizzoResidenza { get; set; }
    public string? CapResidenza { get; set; }
    public string? ComuneDomicilioDescr { get; set; }
    public string? IndirizzoDomicilio { get; set; }
    public string? CapDomicilio { get; set; }
    public string? ComuneReperibilitaDescr { get; set; }
    public string? IndirizzoReperibilita { get; set; }
    public string? CapReperibilita { get; set; }
    public string? NomeCampanelloReperibilita { get; set; }
    public int AreaResidenzaId { get; set; }
    public int? AreaDomicilioId { get; set; }
    public int? AreaReperibilitaId { get; set; }
    public string? MedicoCodice { get; set; }
    public string? MedicoNominativo { get; set; }
    public string? MedicoEmail { get; set; }
    public string? MedicoTelefono1 { get; set; }
    public string? MedicoTelefono2 { get; set; }
}
