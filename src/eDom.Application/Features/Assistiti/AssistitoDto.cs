namespace eDom.Application.Features.Assistiti;

public record AssistitoDto(
    string? Id,
    string Codice,
    string Cognome,
    string Nome,
    string NomeCompleto,
    DateTime DataNascita,
    string CodiceFiscale,
    string Sesso,
    string? Email,
    string? CodiceSanitario,
    string? Telefono1,
    string? IndirizzoResidenza,
    string? CapResidenza,
    int? MedicoId,
    bool Attivo,
    DateTime DataInserimento);
