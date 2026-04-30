using eDom.Application.Features.Ruoli;
using eDom.Application.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eDom.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RuoliController(IMediator mediator, ILogger<RuoliController> logger) : ControllerBase
{
    public sealed record AggiornaRuoloFunzioniRequest(IReadOnlyList<int> FunzioneIds);

    [HttpGet]
    public async Task<IActionResult> Cerca([FromQuery] CercaRuoliQuery query, CancellationToken ct)
    {
        var result = await mediator.SendAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("paginated")]
    public async Task<IActionResult> CercaPaginata(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        [FromQuery] string? sort = null,
        [FromQuery] string? filter = null,
        CancellationToken ct = default)
    {
        var query = new CercaRuoliPaginataQuery(
                Skip: skip,
                Take: take,
            Sort: sort,
            Filter: filter
        );

        var result = await mediator.SendAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> OttieniPerId(int id, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new OttieniRuoloPerIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Crea([FromBody] CreaRuoloCommand command, CancellationToken ct)
    {
        var created = await mediator.SendAsync(command, ct);
        return CreatedAtAction(nameof(OttieniPerId), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Aggiorna(int id, [FromBody] AggiornaRuoloCommand command, CancellationToken ct)
    {
        var cmd = command with { Id = id };
        var result = await mediator.SendAsync(cmd, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Elimina(int id, CancellationToken ct)
    {
        var deleted = await mediator.SendAsync(new EliminaRuoloCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/funzioni")]
    public async Task<IActionResult> OttieniFunzioniRuolo(int id, CancellationToken ct)
    {
        var result = await mediator.SendAsync(new OttieniRuoloFunzioniQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:int}/funzioni")]
    public async Task<IActionResult> AggiornaFunzioniRuolo(
        int id,
        [FromBody] AggiornaRuoloFunzioniRequest? request,
        CancellationToken ct)
    {
        var funzioneIds = request?.FunzioneIds ?? [];
        var updated = await mediator.SendAsync(new AggiornaRuoloFunzioniCommand(id, funzioneIds), ct);
        return updated ? NoContent() : NotFound();
    }
}
