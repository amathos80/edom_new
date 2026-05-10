using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.SistemaMessaggi;

public sealed class GetSistemaMessaggioByChiaveHandler(ISistemaMessaggioRepository repository)
    : IRequestHandler<GetSistemaMessaggioByChiaveQuery, SistemaMessaggioDto?>
{
    public async Task<SistemaMessaggioDto?> HandleAsync(GetSistemaMessaggioByChiaveQuery query, CancellationToken ct = default)
    {
        var messaggio = await repository.GetByKeyAsync(query.Classe, query.Nome, query.Lingua, ct);
        if (messaggio is null)
        {
            return null;
        }

        return new SistemaMessaggioDto(
            messaggio.Id,
            messaggio.Classe,
            messaggio.Nome,
            messaggio.Descrizione,
            messaggio.Lingua,
            messaggio.Custom01,
            messaggio.Custom02,
            messaggio.Custom03,
            messaggio.Custom04,
            messaggio.Custom05,
            messaggio.FlagAttivo == 1);
    }
}
