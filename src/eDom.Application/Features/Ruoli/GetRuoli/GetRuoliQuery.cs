using eDom.Application.Mediator;

namespace eDom.Application.Features.Ruoli;

public record CercaRuoliQuery(
    string? Codice,
    string? Descrizione,
    bool? FlagAmministratore,
    int? ProceduraId) : IRequest<IEnumerable<RuoloDto>>;
