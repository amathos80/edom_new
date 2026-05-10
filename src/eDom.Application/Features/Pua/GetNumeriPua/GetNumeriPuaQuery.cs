using eDom.Application.Mediator;

namespace eDom.Application.Features.Pua.GetNumeriPua;

public record GetNumeriPuaQuery : IRequest<IReadOnlyList<NumeroPuaDto>>;

public record NumeroPuaDto(int Id, string Codice, int Anno, string CodiceAnno);
