using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Ruoli;

public sealed class OttieniRuoloPerIdHandler(IRepository<Ruolo> repository)
    : IRequestHandler<OttieniRuoloPerIdQuery, RuoloDto?>
{
    public async Task<RuoloDto?> HandleAsync(OttieniRuoloPerIdQuery q, CancellationToken ct = default)
    {
        var ruolo = await repository.GetByIdAsync(q.Id, ct);
        if (ruolo is null) return null;

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
