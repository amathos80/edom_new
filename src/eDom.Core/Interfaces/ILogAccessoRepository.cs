using eDom.Core.Entities;

namespace eDom.Core.Interfaces;

public interface ILogAccessoRepository
{
    Task InsertAsync(LogAccesso logAccesso, CancellationToken ct = default);
}
