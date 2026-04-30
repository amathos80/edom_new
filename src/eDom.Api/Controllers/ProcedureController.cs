using eDom.Application.Features.Procedure;
using eDom.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eDom.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProcedureController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? codice,
        [FromQuery] string? descrizione,
        [FromQuery] bool soloVisibili = true,
        CancellationToken ct = default)
    {
        var result = await mediator.SendAsync(new GetProcedureQuery(codice, descrizione, soloVisibili), ct);
        return Ok(result);
    }

    [HttpGet("codice/{codice}")]
    public async Task<IActionResult> GetByCodice(string codice, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codice))
        {
            return BadRequest("Il codice procedura e obbligatorio.");
        }

        var result = await mediator.SendAsync(new GetProcedureByCodiceQuery(codice), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
