using eDom.Core.Entities;
using eDom.Core.Interfaces;
using eDom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eDom.Infrastructure.Repositories;

public class PazientiRepository(HctDbContext context) : Repository<Paziente>(context), IPazientiRepository
{
    public async Task<IEnumerable<Paziente>> SearchAsync(
        string? cognome, string? codiceFiscale, bool? attivo, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(cognome))
            query = query.Where(p => p.Cognome.ToUpper().StartsWith(cognome.ToUpper()));

        if (!string.IsNullOrWhiteSpace(codiceFiscale))
            query = query.Where(p => p.CodiceFiscale == codiceFiscale.ToUpper());

        if (attivo.HasValue)
        {
            short attivoVal = attivo.Value ? (short)1 : (short)0;
            query = query.Where(p => p.Attivo == attivoVal);
        }

        return await query.OrderBy(p => p.Cognome).ThenBy(p => p.Nome).ToListAsync(ct);
    }

    public async Task<Paziente?> GetByCodiceAsync(string codice, CancellationToken ct = default) =>
        await DbSet.AsNoTracking().FirstOrDefaultAsync(p => p.Codice == codice, ct);
}
