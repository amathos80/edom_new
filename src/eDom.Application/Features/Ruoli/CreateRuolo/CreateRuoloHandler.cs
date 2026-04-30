using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Ruoli;

public sealed class CreaRuoloHandler(
    IRepository<Ruolo> repository,
    ICurrentUser currentUser)
    : IRequestHandler<CreaRuoloCommand, RuoloDto>
{
    public async Task<RuoloDto> HandleAsync(CreaRuoloCommand cmd, CancellationToken ct = default)
    {
        var ruolo = new Ruolo
        {
            ProcedureId = cmd.ProceduraId,
            Codice = cmd.Codice.Trim(),
            Descrizione = cmd.Descrizione.Trim(),
            FlagAdmin = cmd.FlagAmministratore ? (short)1 : (short)0,
            DataInserimento = DateTime.UtcNow,
            UtenteInserimento = currentUser.Id ?? 0,
        };

        await repository.AddAsync(ruolo, ct);
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
