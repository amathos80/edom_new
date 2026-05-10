using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.SistemaMessaggi;

public sealed class GetSistemaMessaggiHandler(ISistemaMessaggioRepository repository)
    : IRequestHandler<GetSistemaMessaggiQuery, IEnumerable<SistemaMessaggioDto>>
{
    public async Task<IEnumerable<SistemaMessaggioDto>> HandleAsync(GetSistemaMessaggiQuery query, CancellationToken ct = default)
    {
        var messaggi = await repository.SearchAsync(
            query.Classe,
            query.Nome,
            query.Lingua,
            query.SoloAttivi,
            ct);

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
