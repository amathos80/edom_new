using eDom.Application.Mediator;

namespace eDom.Application.Features.Auth;

public record RefreshTokenCommand(string RefreshToken, string? IpAddress = null) : IRequest<LoginResponse?>;
