namespace eDom.Core.Models;

/// <summary>
/// Flat record returned by the combined view search (V_CO_PAZIENTI UNION V_ANAGRAFE_ASSISTITI).
/// </summary>
public record AssistitoSearchRow(
    string? PaziId,
    string? Codice,
    string? Cognome,
    string? Nome,
    DateTime? DataNascita,
    string? CodiceFiscale,
    string? Sesso,
    string? Email,
    string? CodiceSanitario,
    string? Telefono1,
    string? CapResidenza,
    string? IndirizzoResidenza,
    int? MedicoId,
    string Fonte);
