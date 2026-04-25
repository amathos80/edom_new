using eDom.Application.Mediator;
using eDom.Core.Interfaces;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Pazienti;

public sealed class UpdatePazienteHandler(
    IPazientiRepository repository,
    PazientiMapper mapper,
    ICurrentUser currentUser)
    : IRequestHandler<UpdatePazienteCommand, PazienteDto?>
{
    public async Task<PazienteDto?> HandleAsync(UpdatePazienteCommand cmd, CancellationToken ct = default)
    {
        var paziente = await repository.GetByIdAsync(cmd.Id, ct);
        if (paziente is null) return null;

        mapper.ApplyUpdate(cmd, paziente);
        paziente.DataModifica = DateTime.UtcNow;
        paziente.UtenteModifica = currentUser.Id;

        repository.Update(paziente);
        await repository.SaveChangesAsync(ct);

        return mapper.ToDto(paziente);
    }
}
