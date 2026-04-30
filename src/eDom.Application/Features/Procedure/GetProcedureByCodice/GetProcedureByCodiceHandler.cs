using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Procedure;

public sealed class GetProcedureByCodiceHandler(IRepository<Procedura> repository)
    : IRequestHandler<GetProcedureByCodiceQuery, ProcedureDto?>
{
    public async Task<ProcedureDto?> HandleAsync(GetProcedureByCodiceQuery query, CancellationToken ct = default)
    {
        var procedure = await repository.GetAllAsync(
            filter: procedura => procedura.Codice == query.Codice,
            take: 1,
            ct: ct);

        var procedura = procedure.FirstOrDefault();
        if (procedura is null)
        {
            return null;
        }

        return new ProcedureDto(
            procedura.Id,
            procedura.Codice,
            procedura.Descrizione,
            procedura.Visibile == 1);
    }
}
