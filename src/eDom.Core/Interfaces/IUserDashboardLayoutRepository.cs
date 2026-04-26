using eDom.Core.Entities;

namespace eDom.Core.Interfaces;

public interface IUserDashboardLayoutRepository
{
    Task<UserDashboardLayout?> GetByUserAsync(string userCodice, CancellationToken ct = default);
    Task UpsertAsync(string userCodice, string layoutJson, CancellationToken ct = default);
}
