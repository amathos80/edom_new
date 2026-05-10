using eDom.Core.Entities;

namespace eDom.Core.Interfaces;

public interface ISistemaMessaggioRepository : IRepository<SistemaMessaggio>
{
    Task<IReadOnlyList<SistemaMessaggio>> SearchAsync(
        string? classe,
        string? nome,
        string? lingua,
        bool soloAttivi,
        CancellationToken ct = default);

    Task<SistemaMessaggio?> GetByKeyAsync(
        string classe,
        string nome,
        string lingua,
        CancellationToken ct = default);
}