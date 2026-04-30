using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Procedure;

public sealed class GetProcedureHandler(IRepository<Procedura> repository)
    : IRequestHandler<GetProcedureQuery, IEnumerable<ProcedureDto>>
{
    public async Task<IEnumerable<ProcedureDto>> HandleAsync(GetProcedureQuery query, CancellationToken ct = default)
    {
        var procedure = await repository.GetAllAsync(
            filter: procedura =>
                (string.IsNullOrWhiteSpace(query.Codice) || procedura.Codice.Contains(query.Codice)) &&
                (string.IsNullOrWhiteSpace(query.Descrizione) || procedura.Descrizione.Contains(query.Descrizione)) &&
                (!query.SoloVisibili || procedura.Visibile == 1),
            orderBy: src => src.OrderBy(procedura => procedura.Codice),
            ct: ct);

        return procedure.Select(procedura => new ProcedureDto(
            procedura.Id,
            procedura.Codice,
            procedura.Descrizione,
            procedura.Visibile == 1));
    }
}
