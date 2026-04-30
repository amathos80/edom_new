using eDom.Application.Features.Utenti;
using eDom.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eDom.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UtentiController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Cerca([FromQuery] CercaUtentiQuery query, CancellationToken ct)
    {
        var result = await mediator.SendAsync(query, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Crea([FromBody] CreaUtenteCommand command, CancellationToken ct)
    {
        var created = await mediator.SendAsync(command, ct);
        return CreatedAtAction(nameof(Cerca), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Aggiorna(int id, [FromBody] AggiornaUtenteCommand command, CancellationToken ct)
    {
        var cmd = command with { Id = id };
        var result = await mediator.SendAsync(cmd, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Elimina(int id, CancellationToken ct)
    {
        var deleted = await mediator.SendAsync(new EliminaUtenteCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, CancellationToken ct)
    {
        var updated = await mediator.SendAsync(new ResetPasswordUtenteCommand(id), ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/riattiva")]
    public async Task<IActionResult> Riattiva(int id, CancellationToken ct)
    {
        var updated = await mediator.SendAsync(new RiattivaUtenteCommand(id), ct);
        return updated ? NoContent() : NotFound();
    }
}
