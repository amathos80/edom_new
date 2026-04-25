using eDom.Application.Mediator;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Pazienti;

public sealed class DeletePazienteHandler(IPazientiRepository repository)
    : IRequestHandler<DeletePazienteCommand, bool>
{
    public async Task<bool> HandleAsync(DeletePazienteCommand cmd, CancellationToken ct = default)
    {
        var paziente = await repository.GetByIdAsync(cmd.Id, ct);
        if (paziente is null) return false;

        repository.Remove(paziente);
        await repository.SaveChangesAsync(ct);
        return true;
    }
}
