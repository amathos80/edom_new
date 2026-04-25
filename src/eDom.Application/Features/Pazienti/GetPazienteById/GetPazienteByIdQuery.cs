using eDom.Application.Mediator;

namespace eDom.Application.Features.Pazienti;

public record GetPazienteByIdQuery(int Id) : IRequest<PazienteDto?>;
