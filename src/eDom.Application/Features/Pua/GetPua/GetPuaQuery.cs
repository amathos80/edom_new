using eDom.Application.Mediator;

namespace eDom.Application.Features.Pua;

public record GetPuaQuery(
    int? PazienteId,
    int? NumeroPuaId,
    bool? Attivo,
    DateTime? DataDa,
    DateTime? DataA,
    int? Take) : IRequest<IEnumerable<PuaDto>>;
