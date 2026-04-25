using eDom.Application.Mediator;

namespace eDom.Application.Features.Pazienti;

public record CreatePazienteCommand(
    string Codice,
    string Cognome,
    string Nome,
    DateTime DataNascita,
    string CodiceFiscale,
    string Sesso,
    string? Email,
    string? CodiceSanitario,
    string? Telefono1,
    string? Telefono2,
    string? IndirizzoResidenza,
    string? CapResidenza,
    int? MedicoId) : IRequest<PazienteDto>;
