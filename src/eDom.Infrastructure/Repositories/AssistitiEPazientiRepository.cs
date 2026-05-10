using System.Linq.Expressions;
using eDom.Core.Entities;
using eDom.Core.Interfaces;
using eDom.Core.Models;
using eDom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eDom.Infrastructure.Repositories;

public class AssistitiEPazientiRepository(HctDbContext context) :
 Repository<VistaRicercaPazienteAssistito>(context), IAssistitiEPazientiRepository
{
    public async Task<PagedResult<VistaRicercaPazienteAssistito>> SearchAsync(
        string? cognome, string? nome, string? codiceFiscale, DateOnly? dataNascita, 
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(cognome))
            query = query.Where(p => EF.Functions.ILike(p.Cognome, $"{cognome}%"));

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(p => EF.Functions.ILike(p.Nome, $"{nome}%"));

        if (!string.IsNullOrWhiteSpace(codiceFiscale))
            query = query.Where(p => EF.Functions.ILike(p.CodiceFiscale, $"{codiceFiscale}%"));

        if (dataNascita.HasValue)
            query = query.Where(p => p.DataNascita.HasValue && p.DataNascita.Value.Date == dataNascita.Value.ToDateTime(TimeOnly.MinValue));

        

        var ordered = query.OrderBy(p => p.Cognome).ThenBy(p => p.Nome).ThenBy(p => p.DataNascita);

        var totalCount = await ordered.CountAsync(ct);
        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<VistaRicercaPazienteAssistito>(items, totalCount, page, pageSize);
    }

    

   

    // public async Task<PagedResult<PazienteSearchRow>> SearchFromViewsAsync(
    //     string? cognome, string? nome, string? codiceFiscale, DateOnly? dataNascita, bool? attivo,
    //     int page, int pageSize, CancellationToken ct = default)
    // {
    //     var query = context.VistaRicercaPazienti.AsNoTracking().AsQueryable();

    //     if (!string.IsNullOrWhiteSpace(cognome))
    //         query = query.Where(r => EF.Functions.ILike(r.Cognome!, $"{cognome}%"));
    //     if (!string.IsNullOrWhiteSpace(nome))
    //         query = query.Where(r => EF.Functions.ILike(r.Nome!, $"{nome}%"));
    //     if (!string.IsNullOrWhiteSpace(codiceFiscale))
    //         query = query.Where(r => EF.Functions.ILike(r.CodiceFiscale!, $"{codiceFiscale}%"));
    //     if (dataNascita.HasValue)
    //         query = query.Where(r => r.DataNascita!.Value.Date == dataNascita.Value.ToDateTime(TimeOnly.MinValue));
    //     if (attivo.HasValue)
    //     {
    //         short attivoVal = attivo.Value ? (short)1 : (short)0;
    //         // attivo filter only applies to V_CO_PAZIENTI rows; ANAG rows are always included
    //         query = query.Where(r => r.Fonte == "ANAG" || r.FAtt == attivoVal);
    //     }

    //     var ordered = query
    //         .OrderBy(r => r.Cognome)
    //         .ThenBy(r => r.Nome)
    //         .ThenBy(r => r.DataNascita);

    //     var totalCount = await ordered.CountAsync(ct);
    //     var items = await ordered
    //         .Skip((page - 1) * pageSize)
    //         .Take(pageSize)
    //         .ToListAsync(ct);

    //     var rows = items.Select(r => new PazienteSearchRow(
    //         r.PaziId,
    //         r.Codice,
    //         r.Cognome,
    //         r.Nome,
    //         r.DataNascita,
    //         r.CodiceFiscale,
    //         r.Sesso,
    //         r.Email,
    //         r.CodiceSanitario,
    //         r.Telefono1,
    //         r.CapResidenza,
    //         r.IndirizzoResidenza,
    //         r.MedicoId,
    //         r.Fonte ?? string.Empty));

    //     return new PagedResult<PazienteSearchRow>(rows, totalCount, page, pageSize);
    // }
}

