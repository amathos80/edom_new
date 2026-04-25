using eDom.Core.Entities;
using eDom.Core.Interfaces;
using eDom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eDom.Infrastructure.Repositories;

public class UtenteRepository(HctDbContext context) : Repository<Utente>(context), IUtenteRepository
{
    public async Task<Utente?> GetByCodiceAsync(string codice, CancellationToken ct = default) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Codice == codice, ct);

    public async Task<Utente?> GetByCodiceWithRuoliAsync(string codice, CancellationToken ct = default) =>
        await DbSet
            .Include(u => u.Ruoli)
            .FirstOrDefaultAsync(u => u.Codice == codice, ct);

    public async Task UpdateUltimoLoginAsync(int utenteId, DateTime ultimoLogin, CancellationToken ct = default) =>
        await Context.Utenti
            .Where(u => u.Id == utenteId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.UltimoLogin, ultimoLogin), ct);
}
