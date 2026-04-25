using eDom.Application.Mediator;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Pazienti;

public sealed class GetPazientiHandler(IPazientiRepository repository, PazientiMapper mapper)
    : IRequestHandler<GetPazientiQuery, IEnumerable<PazienteDto>>
{
    public async Task<IEnumerable<PazienteDto>> HandleAsync(GetPazientiQuery q, CancellationToken ct = default)
    {
        var pazienti = await repository.SearchAsync(q.Cognome, q.CodiceFiscale, q.Attivo, ct);
        return mapper.ToDtoList(pazienti);
    }
}
