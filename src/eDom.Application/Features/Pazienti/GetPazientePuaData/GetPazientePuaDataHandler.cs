using eDom.Application.Mediator;
using eDom.Core.Interfaces;
using eDom.Core.Models;

namespace eDom.Application.Features.Pazienti;

public sealed class GetPazientePuaDataHandler(IPazientiRepository repository)
    : IRequestHandler<GetPazientePuaDataQuery, PazientePuaData?>
{
    public async Task<PazientePuaData?> HandleAsync(GetPazientePuaDataQuery q, CancellationToken ct = default)
        => await repository.GetPuaDataAsync(q.Id, ct);
}
