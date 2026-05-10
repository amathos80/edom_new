using eDom.Application.Mediator;
using eDom.Core.Interfaces;
using PuaEntity = eDom.Core.Entities.PuaRecord;

namespace eDom.Application.Features.Pua;

public sealed class GetPuaByIdHandler(IRepository<PuaEntity> repository)
    : IRequestHandler<GetPuaByIdQuery, PuaDto?>
{
    public async Task<PuaDto?> HandleAsync(GetPuaByIdQuery query, CancellationToken ct = default)
    {
        var row = await repository.GetByIdAsync(query.Id, ct);
        return row is null ? null : PuaMapper.ToDto(row);
    }
}
