using eDom.Application.Mediator;

namespace eDom.Application.Features.Pua;

public record DeletePuaCommand(int Id) : IRequest<bool>;
