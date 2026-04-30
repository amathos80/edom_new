using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Ruoli;

public sealed class EliminaRuoloHandler(IRepository<Ruolo> repository)
    : IRequestHandler<EliminaRuoloCommand, bool>
{
    public async Task<bool> HandleAsync(EliminaRuoloCommand cmd, CancellationToken ct = default)
    {
        var ruolo = await repository.GetByIdAsync(cmd.Id, ct);
        if (ruolo is null) return false;

        repository.Remove(ruolo);
        await repository.SaveChangesAsync(ct);
        return true;
    }
}
