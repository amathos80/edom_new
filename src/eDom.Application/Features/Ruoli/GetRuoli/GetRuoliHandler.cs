using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Ruoli;

public sealed class CercaRuoliHandler(
    IRepository<Ruolo> repository,
    IRepository<Procedura> proceduraRepository)
    : IRequestHandler<CercaRuoliQuery, IEnumerable<RuoloDto>>
{
    public async Task<IEnumerable<RuoloDto>> HandleAsync(CercaRuoliQuery q, CancellationToken ct = default)
    {
        var ruoli = await repository.GetAllAsync(
            filter: r =>
                (string.IsNullOrWhiteSpace(q.Codice) || r.Codice.Contains(q.Codice)) &&
                (string.IsNullOrWhiteSpace(q.Descrizione) || r.Descrizione.Contains(q.Descrizione)) &&
                (!q.FlagAmministratore.HasValue || r.FlagAdmin == (q.FlagAmministratore.Value ? (short)1 : (short)0)) &&
                (!q.ProceduraId.HasValue || r.ProcedureId == q.ProceduraId.Value),
            orderBy: src => src.OrderBy(r => r.Codice),
            ct: ct);

        var proceduraIds = ruoli
            .Select(r => r.ProcedureId)
            .Distinct()
            .ToArray();

        var procedure = await proceduraRepository.GetAllAsync(
            filter: p => proceduraIds.Contains(p.Id),
            ct: ct);

        var proceduraCodici = procedure.ToDictionary(p => p.Id, p => p.Codice);

        return ruoli.Select(r => new RuoloDto(
            r.Id,
            r.ProcedureId,
            r.Codice,
            r.Descrizione,
            r.FlagAdmin == 1,
            r.DataInserimento,
            r.DataModifica,
            proceduraCodici.GetValueOrDefault(r.ProcedureId)));
    }
}
