using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Pua.GetAree;

public sealed class GetAreeHandler(IRepository<Area> repository)
    : IRequestHandler<GetAreeQuery, IReadOnlyList<AreaDto>>
{
    public async Task<IReadOnlyList<AreaDto>> HandleAsync(GetAreeQuery request, CancellationToken ct = default)
    {
        var rows = await repository.GetAllAsync(
            filter: x => x.Attivo == 1,
            orderBy: src => src.OrderBy(x => x.Descr01),
            ct: ct);

        return rows
            .Select(x => new AreaDto(x.Id, x.Codice, x.Descr01))
            .ToList();
    }
}
