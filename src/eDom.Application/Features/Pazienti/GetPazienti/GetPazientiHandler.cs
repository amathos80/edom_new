using eDom.Application.Mediator;
using eDom.Core.Interfaces;
using eDom.Core.Models;

namespace eDom.Application.Features.Pazienti;

public sealed class GetPazientiHandler(IPazientiRepository repository)
    : IRequestHandler<GetPazientiQuery, PagedResult<PazienteDto>>
{
    public async Task<PagedResult<PazienteDto>> HandleAsync(GetPazientiQuery q, CancellationToken ct = default)
    {
        // var page = Math.Max(1, q.Page);
        // var pageSize = Math.Clamp(q.PageSize, 1, 100);

        // var pagedRows = await repository.SearchFromViewsAsync(
        //     q.Cognome, q.Nome, q.CodiceFiscale, q.DataNascita, q.Attivo,
        //     page, pageSize, ct);

        // var dtos = pagedRows.Items.Select(r => new PazienteDto(
        //     r.PaziId ?? string.Empty,
        //     r.Codice ?? string.Empty,
        //     r.Cognome ?? string.Empty,
        //     r.Nome ?? string.Empty,
        //     $"{r.Cognome} {r.Nome}".Trim(),
        //     r.DataNascita ?? DateTime.MinValue,
        //     r.CodiceFiscale ?? string.Empty,
        //     r.Sesso ?? string.Empty,
        //     r.Email,
        //     r.CodiceSanitario,
        //     r.Telefono1,
        //     r.IndirizzoResidenza,
        //     r.CapResidenza,
        //     r.MedicoId,
        //     r.Fonte == "PAZI",
        //     DateTime.MinValue));

        // return new PagedResult<PazienteDto>(dtos, pagedRows.TotalCount, page, pageSize);
        return await Task.FromResult(new PagedResult<PazienteDto>(Enumerable.Empty<PazienteDto>(), 0, q.Page, q.PageSize));
    }
}
