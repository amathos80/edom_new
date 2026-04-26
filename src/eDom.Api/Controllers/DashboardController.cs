using eDom.Application.Features.Dashboard;
using eDom.Application.Mediator;
using eDom.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eDom.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet("layout")]
    public async Task<IActionResult> GetLayout(CancellationToken ct)
    {
        var userCodice = currentUser.Username;
        if (string.IsNullOrEmpty(userCodice)) return Unauthorized();

        var result = await mediator.SendAsync(new GetDashboardLayoutQuery(userCodice), ct);
        if (result is null) return NotFound();

        return Content(result.LayoutJson, "application/json");
    }

    [HttpPut("layout")]
    public async Task<IActionResult> SaveLayout([FromBody] object body, CancellationToken ct)
    {
        var userCodice = currentUser.Username;
        if (string.IsNullOrEmpty(userCodice)) return Unauthorized();

        var layoutJson = System.Text.Json.JsonSerializer.Serialize(body);
        var result = await mediator.SendAsync(new SaveDashboardLayoutCommand(userCodice, layoutJson), ct);

        return Content(result.LayoutJson, "application/json");
    }
}
