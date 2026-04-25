namespace eDom.Application.Features.Pazienti;

public record PazienteDto(
    int Id,
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
