using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Ruoli;

public sealed class AggiornaRuoloHandler(
    IRepository<Ruolo> repository,
    ICurrentUser currentUser)
    : IRequestHandler<AggiornaRuoloCommand, RuoloDto?>
{
    public async Task<RuoloDto?> HandleAsync(AggiornaRuoloCommand cmd, CancellationToken ct = default)
    {
        var ruolo = await repository.GetByIdAsync(cmd.Id, ct);
        if (ruolo is null) return null;

        ruolo.ProcedureId = cmd.ProceduraId;
        ruolo.Codice = cmd.Codice.Trim();
        ruolo.Descrizione = cmd.Descrizione.Trim();
        ruolo.FlagAdmin = cmd.FlagAmministratore ? (short)1 : (short)0;
        ruolo.DataModifica = DateTime.UtcNow;
        ruolo.UtenteModifica = currentUser.Id;

        repository.Update(ruolo);
        await repository.SaveChangesAsync(ct);

        return new RuoloDto(
            ruolo.Id,
            ruolo.ProcedureId,
            ruolo.Codice,
            ruolo.Descrizione,
            ruolo.FlagAdmin == 1,
            ruolo.DataInserimento,
            ruolo.DataModifica);
    }
}
