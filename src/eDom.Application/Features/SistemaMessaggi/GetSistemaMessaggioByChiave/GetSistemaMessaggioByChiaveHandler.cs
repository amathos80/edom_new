using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.SistemaMessaggi;

public sealed class GetSistemaMessaggioByChiaveHandler(IRepository<SistemaMessaggio> repository)
    : IRequestHandler<GetSistemaMessaggioByChiaveQuery, SistemaMessaggioDto?>
{
    public async Task<SistemaMessaggioDto?> HandleAsync(GetSistemaMessaggioByChiaveQuery query, CancellationToken ct = default)
    {
        var messaggi = await repository.GetAllAsync(
            filter: m =>
                m.Classe == query.Classe &&
                m.Nome == query.Nome &&
                m.Lingua == query.Lingua,
            take: 1,
            ct: ct);

        var messaggio = messaggi.FirstOrDefault();
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
