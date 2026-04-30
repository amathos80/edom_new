using eDom.Application.Mediator;
using eDom.Core.Entities;

namespace eDom.Application.Features.Ruoli;

public record RispostaPaginata<T>(
    List<T> Items,
    int Totale,
    int Skip,
    int Take
);

public record CercaRuoliPaginataQuery(
    int Skip = 0,
    int Take = 10,
    string? Sort = null,
    string? Filter = null
) : IRequest<RispostaPaginata<RuoloDto>>;
