using eDom.Application.Mediator;

namespace eDom.Application.Features.Auth;

public record ChangePasswordCommand(string OldPassword, string NewPassword) : IRequest<ChangePasswordResponse>;

public record ChangePasswordResponse(bool Success, string? Error = null);
