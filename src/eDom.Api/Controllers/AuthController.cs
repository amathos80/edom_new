using eDom.Application.Features.Auth;
using eDom.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eDom.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await mediator.SendAsync(command with { IpAddress = ip }, ct);
        if (result is null)
            return Unauthorized(new { message = "Credenziali non valide." });
        return Ok(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken ct)
    {
        var result = await mediator.SendAsync(command, ct);
        if (!result.Success)
            return BadRequest(new { message = result.Error });
        return Ok(result);
    }
}
