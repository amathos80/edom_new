using eDom.Application.Mediator;

namespace eDom.Application.Features.Pazienti;

public record DeletePazienteCommand(int Id) : IRequest<bool>;
