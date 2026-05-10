namespace eDom.Core.Entities;

/// <summary>
/// Keyless entity mapped via ToSqlQuery to the UNION ALL of
/// V_CO_PAZIENTI and V_ANAGRAFE_ASSISTITI.
/// EF Core wraps the SQL as a subquery, so WHERE/ORDER BY/COUNT/LIMIT/OFFSET
/// all execute server-side.
/// </summary>
public class VistaRicercaPazienteAssistito
{
    public string? PaziId { get; set; }
    public string? Codice { get; set; }
    public string? Cognome { get; set; }
    public string? Nome { get; set; }
    public DateTime? DataNascita { get; set; }
    public string? CodiceFiscale { get; set; }
    public string? Sesso { get; set; }
    public string? Email { get; set; }
    public string? CodiceSanitario { get; set; }
    public string? Telefono1 { get; set; }
    public string? CapResidenza { get; set; }
    public string? IndirizzoResidenza { get; set; }
    public int? MedicoId { get; set; }
    public short? FAtt { get; set; }
    public string? Fonte { get; set; }
}
