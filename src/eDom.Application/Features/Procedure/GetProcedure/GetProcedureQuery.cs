using eDom.Application.Mediator;

namespace eDom.Application.Features.Procedure;

public record GetProcedureQuery(
    string? Codice,
    string? Descrizione,
    bool SoloVisibili) : IRequest<IEnumerable<ProcedureDto>>;
