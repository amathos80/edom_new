using eDom.Core.Entities;
using eDom.Core.Interfaces;
using eDom.Core.Models;
using eDom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eDom.Infrastructure.Repositories;

public class UtenteRepository(HctDbContext context) : Repository<Utente>(context), IUtenteRepository
{
    public async Task<Utente?> GetByCodiceAsync(string codice, CancellationToken ct = default) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Codice == codice, ct);

    public async Task<Utente?> GetByCodiceWithRuoliAsync(string codice, CancellationToken ct = default)
        => await DbSet
            .Include(u => u.Ruoli)
            .FirstOrDefaultAsync(u => u.Codice == codice, ct);

    public async Task<ProfiloAutorizzativoUtente?> OttieniProfiloAutorizzativoAsync(string codice, CancellationToken ct = default)
    {
        var profilo = await DbSet
            .AsNoTracking()
            .Where(u => u.Codice == codice)
            .Select(u => new ProfiloAutorizzativoUtente
            {
                Id = u.Id,
                Codice = u.Codice,
                Password = u.Password,
                Nome = u.Nome,
                Cognome = u.Cognome,
                FlagCambiaPwd = u.FlagCambiaPwd,
                DataDisattivazione = u.DataDisattivazione,
                DataScadenzaPassword = u.DataScadenzaPassword,
                UltimoLogin = u.UltimoLogin,
            })
            .FirstOrDefaultAsync(ct);

        if (profilo is null)
            return null;

        var ruoli = await DbSet
            .AsNoTracking()
            .Where(u => u.Id == profilo.Id)
            .SelectMany(u => u.Ruoli)
            .Select(r => new { r.Id, r.Codice })
            .Distinct()
            .OrderBy(r => r.Codice)
            .ToListAsync(ct);

        profilo.Ruoli = ruoli
            .Select(r => r.Codice)
            .ToList();

        var ruoloIds = ruoli
            .Select(r => r.Id)
            .ToArray();

        if (ruoloIds.Length == 0)
            return profilo;

        profilo.Funzioni = await Context.RuoliFunzione
            .AsNoTracking()
            .Where(rf => ruoloIds.Contains(rf.RuoloId) && rf.Funzione != null)
            .Select(rf => rf.Funzione!.Codice)
            .Distinct()
            .OrderBy(codiceFunzione => codiceFunzione)
            .ToListAsync(ct);

        return profilo;
    }

    public async Task UpdateUltimoLoginAsync(int utenteId, DateTime ultimoLogin, CancellationToken ct = default) =>
        await Context.Utenti
            .Where(u => u.Id == utenteId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.UltimoLogin, ultimoLogin), ct);
}
