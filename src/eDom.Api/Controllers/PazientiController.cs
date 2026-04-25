using eDom.Application.Features.Pazienti;
using eDom.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eDom.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PazientiController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] GetPazientiQuery query, CancellationToken ct)
    {
        var result = await mediator.SendAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new GetPazienteByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePazienteCommand command, CancellationToken ct)
    {
        var created = await mediator.SendAsync(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePazienteCommand command, CancellationToken ct)
    {
        // Garantisce che l'Id nel body corrisponda alla route
        var cmd = command with { Id = id };
        var result = await mediator.SendAsync(cmd, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await mediator.SendAsync(new DeletePazienteCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}
