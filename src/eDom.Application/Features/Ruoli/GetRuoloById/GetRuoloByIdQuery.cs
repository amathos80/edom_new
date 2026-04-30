using eDom.Application.Mediator;

namespace eDom.Application.Features.Ruoli;

public record OttieniRuoloPerIdQuery(int Id) : IRequest<RuoloDto?>;
