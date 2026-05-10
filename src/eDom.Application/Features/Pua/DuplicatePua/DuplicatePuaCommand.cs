using eDom.Application.Mediator;

namespace eDom.Application.Features.Pua;

public record DuplicatePuaCommand(int Id, DateTime? Data) : IRequest<PuaDto?>;
