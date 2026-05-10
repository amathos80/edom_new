using eDom.Application.Features.Pua;
using eDom.Application.Features.Pua.GetAree;
using eDom.Application.Features.Pua.GetNumeriPua;
using eDom.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eDom.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PuaController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] GetPuaQuery query, CancellationToken ct)
    {
        var result = await mediator.SendAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("numeri-pua")]
    public async Task<IActionResult> GetNumeriPua(CancellationToken ct)
    {
        var result = await mediator.SendAsync(new GetNumeriPuaQuery(), ct);
        return Ok(result);
    }

    [HttpGet("aree")]
    public async Task<IActionResult> GetAree(CancellationToken ct)
    {
        var result = await mediator.SendAsync(new GetAreeQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new GetPuaByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePuaCommand command, CancellationToken ct)
    {
        var created = await mediator.SendAsync(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePuaCommand command, CancellationToken ct)
    {
        var cmd = command with { Id = id };
        var result = await mediator.SendAsync(cmd, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:int}/duplica")]
    public async Task<IActionResult> Duplica(int id, [FromBody] DuplicatePuaCommand? command, CancellationToken ct)
    {
        var cmd = command is null ? new DuplicatePuaCommand(id, null) : command with { Id = id };
        var result = await mediator.SendAsync(cmd, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await mediator.SendAsync(new DeletePuaCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}
