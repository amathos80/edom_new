using eDom.Application.Mediator;

namespace eDom.Application.Features.Auth;

public record LogoutCommand(string? IpAddress = null) : IRequest<bool>;
