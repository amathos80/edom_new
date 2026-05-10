using eDom.Application.Mediator;
using eDom.Core.Interfaces;
using PuaEntity = eDom.Core.Entities.PuaRecord;

namespace eDom.Application.Features.Pua;

public sealed class DeletePuaHandler(IRepository<PuaEntity> repository)
    : IRequestHandler<DeletePuaCommand, bool>
{
    public async Task<bool> HandleAsync(DeletePuaCommand cmd, CancellationToken ct = default)
    {
        var row = await repository.GetByIdAsync(cmd.Id, ct);
        if (row is null)
        {
            return false;
        }

        repository.Remove(row);
        await repository.SaveChangesAsync(ct);
        return true;
    }
}
