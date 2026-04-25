using eDom.Application.Mediator;

namespace eDom.Application.Features.Pazienti;

public record GetPazientiQuery(
    string? Cognome,
    string? CodiceFiscale,
    bool? Attivo) : IRequest<IEnumerable<PazienteDto>>;
