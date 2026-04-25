using eDom.Application.Mediator;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Pazienti;

public sealed class GetPazienteByIdHandler(IPazientiRepository repository, PazientiMapper mapper)
    : IRequestHandler<GetPazienteByIdQuery, PazienteDto?>
{
    public async Task<PazienteDto?> HandleAsync(GetPazienteByIdQuery q, CancellationToken ct = default)
    {
        var paziente = await repository.GetByIdAsync(q.Id, ct);
        return paziente is null ? null : mapper.ToDto(paziente);
    }
}
