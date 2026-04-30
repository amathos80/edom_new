using eDom.Application.Mediator;

namespace eDom.Application.Features.Ruoli;

public record EliminaRuoloCommand(int Id) : IRequest<bool>;
