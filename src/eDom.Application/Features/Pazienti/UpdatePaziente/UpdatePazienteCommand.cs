using eDom.Application.Mediator;

namespace eDom.Application.Features.Pazienti;

public record UpdatePazienteCommand(
    int Id,
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
    int? MedicoId,
    bool Attivo) : IRequest<PazienteDto?>;
