using eDom.Application.Mediator;

namespace eDom.Application.Features.Pua.GetAree;

public record GetAreeQuery : IRequest<IReadOnlyList<AreaDto>>;

public record AreaDto(int Id, string Codice, string Descrizione);
