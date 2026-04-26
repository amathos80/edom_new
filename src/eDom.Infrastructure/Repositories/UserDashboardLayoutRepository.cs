using eDom.Core.Entities;
using eDom.Core.Interfaces;
using eDom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eDom.Infrastructure.Repositories;

public class UserDashboardLayoutRepository(HctDbContext db) : IUserDashboardLayoutRepository
{
    public Task<UserDashboardLayout?> GetByUserAsync(string userCodice, CancellationToken ct = default)
        => db.UserDashboardLayouts
             .AsNoTracking()
             .FirstOrDefaultAsync(x => x.UserCodice == userCodice, ct);

    public async Task UpsertAsync(string userCodice, string layoutJson, CancellationToken ct = default)
    {
        var existing = await db.UserDashboardLayouts
            .FirstOrDefaultAsync(x => x.UserCodice == userCodice, ct);

        if (existing is null)
        {
            db.UserDashboardLayouts.Add(new UserDashboardLayout
            {
                UserCodice = userCodice,
                LayoutJson = layoutJson,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.LayoutJson = layoutJson;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }
}
