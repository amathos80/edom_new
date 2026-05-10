using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Pua.GetNumeriPua;

public sealed class GetNumeriPuaHandler(IRepository<NumeroPua> repository)
    : IRequestHandler<GetNumeriPuaQuery, IReadOnlyList<NumeroPuaDto>>
{
    public async Task<IReadOnlyList<NumeroPuaDto>> HandleAsync(GetNumeriPuaQuery request, CancellationToken ct = default)
    {
        var rows = await repository.GetAllAsync(
            orderBy: src => src.OrderBy(x => x.Anno).ThenBy(x => x.Codice),
            ct: ct);

        return rows
            .Select(x => new NumeroPuaDto(x.Id, x.Codice, x.Anno, x.CodiceAnno))
            .ToList();
    }
}
