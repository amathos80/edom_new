using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.SistemaMessaggi;

public sealed class GetSistemaMessaggiHandler(IRepository<SistemaMessaggio> repository)
    : IRequestHandler<GetSistemaMessaggiQuery, IEnumerable<SistemaMessaggioDto>>
{
    public async Task<IEnumerable<SistemaMessaggioDto>> HandleAsync(GetSistemaMessaggiQuery query, CancellationToken ct = default)
    {
        var messaggi = await repository.GetAllAsync(
            filter: m =>
                (string.IsNullOrWhiteSpace(query.Classe) || m.Classe.Contains(query.Classe)) &&
                (string.IsNullOrWhiteSpace(query.Nome) || m.Nome.Contains(query.Nome)) &&
                (string.IsNullOrWhiteSpace(query.Lingua) || m.Lingua == query.Lingua) &&
                (!query.SoloAttivi || m.FlagAttivo == 1),
            orderBy: src => src.OrderBy(m => m.Classe).ThenBy(m => m.Nome).ThenBy(m => m.Lingua),
            ct: ct);

        return messaggi.Select(m => new SistemaMessaggioDto(
            m.Id,
            m.Classe,
            m.Nome,
            m.Descrizione,
            m.Lingua,
            m.Custom01,
            m.Custom02,
            m.Custom03,
            m.Custom04,
            m.Custom05,
            m.FlagAttivo == 1));
    }
}
