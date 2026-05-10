using eDom.Application.Mediator;
using eDom.Core.Models;

namespace eDom.Application.Features.Pazienti;

public record GetPazientiQuery(
    string? Cognome,
    string? Nome,
    string? CodiceFiscale,
    DateOnly? DataNascita,
    bool? Attivo,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<PazienteDto>>;
