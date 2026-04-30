using eDom.Application.Mediator;

namespace eDom.Application.Features.Procedure;

public record GetProcedureByCodiceQuery(string Codice) : IRequest<ProcedureDto?>;
