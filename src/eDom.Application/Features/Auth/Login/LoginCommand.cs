using eDom.Application.Mediator;

namespace eDom.Application.Features.Auth;

public record LoginCommand(string Username, string Password, string? IpAddress = null) : IRequest<LoginResponse?>;

public record LoginResponse(
	string Token,
	int ExpiresIn,
	string RefreshToken,
	int RefreshExpiresIn,
	string Username,
	string FullName,
	IList<string> Roles,
	bool MustChangePassword = false);
