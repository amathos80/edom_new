using eDom.Core.Entities;
using eDom.Core.Interfaces;
using eDom.Infrastructure.Data;

namespace eDom.Infrastructure.Repositories;

public class LogAccessoRepository(HctDbContext context) : ILogAccessoRepository
{
    public async Task InsertAsync(LogAccesso logAccesso, CancellationToken ct = default)
    {
        context.LogAccessi.Add(logAccesso);
        await context.SaveChangesAsync(ct);
    }
}
