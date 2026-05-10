using eDom.Core.Entities;
using eDom.Core.Interfaces;
using eDom.Core.Models;
using eDom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace eDom.Infrastructure.Repositories;

public class PazientiRepository(HctDbContext context) : Repository<Paziente>(context), IPazientiRepository
{
    public async Task<int> ReserveNextIdAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT nextval(pg_get_serial_sequence('\"HICT\".\"CO_PAZIENTI\"', 'PAZI_ID'))";

        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
            await connection.OpenAsync(ct);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var result = await command.ExecuteScalarAsync(ct);

            if (result is null || result is DBNull)
                throw new InvalidOperationException("Unable to reserve next PAZI_ID for CO_PAZIENTI.");

            return Convert.ToInt32(result, CultureInfo.InvariantCulture);
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    public async Task<PagedResult<Paziente>> SearchAsync(
        string? cognome, string? nome, string? codiceFiscale, DateOnly? dataNascita, bool? attivo,
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
            query = query.Where(p => p.DataNascita == dataNascita.Value);

        if (attivo.HasValue)
        {
            short attivoVal = attivo.Value ? (short)1 : (short)0;
            query = query.Where(p => p.Attivo == attivoVal);
        }

        var ordered = query.OrderBy(p => p.Cognome).ThenBy(p => p.Nome).ThenBy(p => p.DataNascita);

        var totalCount = await ordered.CountAsync(ct);
        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Paziente>(items, totalCount, page, pageSize);
    }

    public async Task<Paziente?> GetByCodiceAsync(string codice, CancellationToken ct = default) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(p => p.Codice == codice, ct);

    public async Task<PazientePuaData?> GetPuaDataAsync(int id, CancellationToken ct = default) =>
        await (from p in context.Pazienti.AsNoTracking()
               join cr in context.Comuni.AsNoTracking() on p.ComuneResidenzaId equals cr.Id into crGroup
               from cr in crGroup.DefaultIfEmpty()
               join cd in context.Comuni.AsNoTracking() on p.ComuneDomicilioId equals cd.Id into cdGroup
               from cd in cdGroup.DefaultIfEmpty()
               join crep in context.Comuni.AsNoTracking() on p.ComuneReperibilitaId equals crep.Id into crepGroup
               from crep in crepGroup.DefaultIfEmpty()
               join m in context.Medici.AsNoTracking() on p.MedicoId equals m.Id into mGroup
               from m in mGroup.DefaultIfEmpty()
               where p.Id == id
               select new PazientePuaData
               {
                   Id                   = p.Id,
                   Codice               = p.Codice,
                   Cognome              = p.Cognome,
                   Nome                 = p.Nome,
                   DataNascita          = p.DataNascita,
                   CodiceFiscale        = p.CodiceFiscale,
                   Sesso                = p.Sesso,
                   Email                = p.Email,
                   Telefono1            = p.Telefono1,
                   Telefono2            = p.Telefono2,
                   ComuneResidenzaDescr = cr != null ? cr.Descr01 : null,
                   IndirizzoResidenza   = p.IndirizzoResidenza,
                   CapResidenza         = p.CapResidenza,
                   ComuneDomicilioDescr = cd != null ? cd.Descr01 : null,
                   IndirizzoDomicilio   = p.IndirizzoDomicilio,
                   CapDomicilio         = p.CapDomicilio,
                   ComuneReperibilitaDescr   = crep != null ? crep.Descr01 : null,
                   IndirizzoReperibilita     = p.IndirizzoReperibilita,
                   CapReperibilita           = p.CapReperibilita,
                   NomeCampanelloReperibilita = p.NomeCampanelloReperibilita,
                   AreaResidenzaId      = p.AreaResidenzaId,
                   AreaDomicilioId      = p.AreaDomicilioId,
                   AreaReperibilitaId   = p.AreaReperibilitaId,
                   MedicoCodice         = m != null ? m.Codice : null,
                   MedicoNominativo     = m != null
                       ? (m.Cognome + " " + m.Nome).Trim()
                       : null,
                   MedicoEmail          = m != null ? m.Email : null,
                   MedicoTelefono1      = m != null ? m.Telefono1 : null,
                   MedicoTelefono2      = m != null ? m.Telefono2 : null,
               })
               .FirstOrDefaultAsync(ct);

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

