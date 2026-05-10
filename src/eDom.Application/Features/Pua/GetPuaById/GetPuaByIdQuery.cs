using eDom.Application.Mediator;

namespace eDom.Application.Features.Pua;

public record GetPuaByIdQuery(int Id) : IRequest<PuaDto?>;
