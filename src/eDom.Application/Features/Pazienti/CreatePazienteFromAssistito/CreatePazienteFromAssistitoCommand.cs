using eDom.Application.Mediator;
using eDom.Application.Features.Pazienti;

namespace eDom.Application.Features.Pazienti;

public sealed record CreatePazienteFromAssistitoCommand(string AssistitoId) : IRequest<PazienteDto>;
