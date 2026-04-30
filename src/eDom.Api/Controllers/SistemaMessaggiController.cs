using eDom.Application.Features.SistemaMessaggi;
using eDom.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eDom.Api.Controllers;

[ApiController]
[Route("api/sistema-messaggi")]
[Authorize]
public class SistemaMessaggiController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? classe,
        [FromQuery] string? nome,
        [FromQuery] string? lingua,
        [FromQuery] bool soloAttivi = true,
        CancellationToken ct = default)
    {
        var result = await mediator.SendAsync(new GetSistemaMessaggiQuery(classe, nome, lingua, soloAttivi), ct);
        return Ok(result);
    }

    [HttpGet("by-key")]
    public async Task<IActionResult> GetByKey(
        [FromQuery] string classe,
        [FromQuery] string nome,
        [FromQuery] string lingua,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(classe) || string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(lingua))
        {
            return BadRequest("I parametri classe, nome e lingua sono obbligatori.");
        }

        var result = await mediator.SendAsync(new GetSistemaMessaggioByChiaveQuery(classe, nome, lingua), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
