namespace eDom.Application.Features.Pazienti;

public record PazienteDto(
    int Id,
    string Codice,
    string Cognome,
    string Nome,
    string NomeCompleto,
    DateOnly DataNascita,
    string CodiceFiscale,
    string Sesso,
    string? Email,
    string? CodiceSanitario,
    string? Telefono1,
    string? Telefono2,
    string? IndirizzoResidenza,
    string? CapResidenza,
    string? IndirizzoDomicilio,
    string? CapDomicilio,
    int? MedicoId,
    bool Attivo,
    DateTime DataInserimento);
