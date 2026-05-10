using eDom.Application.Mediator;
using eDom.Core.Interfaces;
using PuaEntity = eDom.Core.Entities.PuaRecord;

namespace eDom.Application.Features.Pua;

public sealed class GetPuaHandler(IRepository<PuaEntity> repository)
    : IRequestHandler<GetPuaQuery, IEnumerable<PuaDto>>
{
    public async Task<IEnumerable<PuaDto>> HandleAsync(GetPuaQuery query, CancellationToken ct = default)
    {
        var rows = await repository.GetAllAsync(
            filter: p =>
                (!query.PazienteId.HasValue || p.PazienteId == query.PazienteId.Value) &&
                (!query.NumeroPuaId.HasValue || p.NumeroPuaId == query.NumeroPuaId.Value) &&
                (!query.Attivo.HasValue || p.Attivo == (query.Attivo.Value ? (short)1 : (short)0)) &&
                (!query.DataDa.HasValue || p.Data >= query.DataDa.Value) &&
                (!query.DataA.HasValue || p.Data <= query.DataA.Value),
            orderBy: src => src.OrderByDescending(p => p.Data).ThenByDescending(p => p.Numero),
            take: query.Take,
            ct: ct);

        return rows.Select(PuaMapper.ToDto);
    }
}
