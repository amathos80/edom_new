using eDom.Application.Mediator;
using eDom.Core.Models;

namespace eDom.Application.Features.Assistiti;

public record GetAssistitiEPazientiQuery(
    string? Cognome,
    string? Nome,
    string? CodiceFiscale,
    DateOnly? DataNascita,
    bool? Attivo,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<AssistitoDto>>;