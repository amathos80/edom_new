using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Ruoli;

public sealed class OttieniRuoloFunzioniHandler(
    IRepository<Ruolo> ruoloRepository,
    IRepository<Funzione> funzioneRepository,
    IRepository<RuoloFunzione> ruoloFunzioneRepository)
    : IRequestHandler<OttieniRuoloFunzioniQuery, IReadOnlyList<RuoloFunzioneNodoDto>?>
{
    public async Task<IReadOnlyList<RuoloFunzioneNodoDto>?> HandleAsync(OttieniRuoloFunzioniQuery query, CancellationToken ct = default)
    {
        var ruolo = (await ruoloRepository.GetAllAsync(
            filter: r => r.Id == query.RuoloId,
            take: 1,
            ct: ct)).FirstOrDefault();
        if (ruolo is null)
        {
            return null;
        }

        var funzioni = await funzioneRepository.GetAllAsync(
            filter: f => f.ProcedureId == ruolo.ProcedureId,
            orderBy: src => src.OrderBy(f => f.Sort).ThenBy(f => f.Codice),
            ct: ct);

        var ruoliFunzione = await ruoloFunzioneRepository.GetAllAsync(
            filter: rf => rf.RuoloId == query.RuoloId,
            ct: ct);

        var selezionate = ruoliFunzione
            .Select(rf => rf.FunzioneId)
            .ToHashSet();

        var mappaNodi = new Dictionary<int, RuoloFunzioneNodoDto>();
        foreach (var funzione in funzioni)
        {
            mappaNodi[funzione.Id] = new RuoloFunzioneNodoDto
            {
                Id = funzione.Id,
                Codice = funzione.Codice,
                Descrizione = funzione.Descrizione,
                ParentId = funzione.ParentId,
                Selezionata = selezionate.Contains(funzione.Id)
            };
        }

        var radici = new List<RuoloFunzioneNodoDto>();
        foreach (var nodo in mappaNodi.Values)
        {
            if (nodo.ParentId.HasValue && mappaNodi.TryGetValue(nodo.ParentId.Value, out var padre))
            {
                padre.Figlie.Add(nodo);
            }
            else
            {
                radici.Add(nodo);
            }
        }

        return radici;
    }
}
