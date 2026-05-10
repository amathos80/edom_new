using eDom.Application.Mediator;
using eDom.Core.Interfaces;
using eDom.Core.Models;

namespace eDom.Application.Features.Assistiti;

public sealed class GetAssistitiEPazientiHandler(IAssistitiEPazientiRepository repository)
    : IRequestHandler<GetAssistitiEPazientiQuery, PagedResult<AssistitoDto>>
{
    public async Task<PagedResult<AssistitoDto>> HandleAsync(
        GetAssistitiEPazientiQuery q,
     CancellationToken ct = default)
    {
        var page = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 100);

        var pagedRows = await repository.SearchAsync(
            q.Cognome, q.Nome, q.CodiceFiscale, q.DataNascita, 
            page, pageSize, ct);

        var dtos = pagedRows.Items.Select(r => new AssistitoDto(
            r.PaziId ?? string.Empty,
            r.Codice ?? string.Empty,
            r.Cognome ?? string.Empty,
            r.Nome ?? string.Empty,
            $"{r.Cognome} {r.Nome}".Trim(),
            r.DataNascita ?? DateTime.MinValue,
            r.CodiceFiscale ?? string.Empty,
            r.Sesso ?? string.Empty,
            r.Email,
            r.CodiceSanitario,
            r.Telefono1,
            r.IndirizzoResidenza,
            r.CapResidenza,
            r.MedicoId,
            r.Fonte == "PAZI",
            DateTime.MinValue));

        return new PagedResult<AssistitoDto>(dtos, pagedRows.TotalCount, page, pageSize);
    }
}
