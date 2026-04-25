using eDom.Application.Mediator;
using eDom.Core.Interfaces;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Pazienti;

public sealed class CreatePazienteHandler(
    IPazientiRepository repository,
    PazientiMapper mapper,
    ICurrentUser currentUser)
    : IRequestHandler<CreatePazienteCommand, PazienteDto>
{
    public async Task<PazienteDto> HandleAsync(CreatePazienteCommand cmd, CancellationToken ct = default)
    {
        var paziente = mapper.FromCreate(cmd);
        paziente.Attivo = 1;   // PAZI_F_ATT: 1 = attivo (Oracle NUMBER(1))
        paziente.DataInserimento = DateTime.UtcNow;
        paziente.UtenteInserimento = currentUser.Id ?? 0;

        await repository.AddAsync(paziente, ct);
        await repository.SaveChangesAsync(ct);

        return mapper.ToDto(paziente);
    }
}
