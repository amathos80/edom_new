using eDom.Core.Interfaces;
using eDom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eDom.Infrastructure.Repositories;

public class ConfigurazioneRepository(HctDbContext context) : IConfigurazioneRepository
{
    public async Task<string?> GetValoreAsync(string codice, int procedureId = 999999, CancellationToken ct = default) =>
        await context.Configurazioni
            .AsNoTracking()
            .Where(c => c.Codice == codice && c.ProcedureId == procedureId)
            .Select(c => c.Valore)
            .FirstOrDefaultAsync(ct);
}
