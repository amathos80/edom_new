using eDom.Core.Entities;
using eDom.Core.Models;

namespace eDom.Core.Interfaces;

public interface IPazientiRepository : IRepository<Paziente>
{
    Task<PagedResult<Paziente>> SearchAsync(string? cognome, string? nome, string? codiceFiscale, DateOnly? dataNascita, bool? attivo, int page, int pageSize, CancellationToken ct = default);
    Task<Paziente?> GetByCodiceAsync(string codice, CancellationToken ct = default);
    Task<int> ReserveNextIdAsync(CancellationToken ct = default);
    Task<PazientePuaData?> GetPuaDataAsync(int id, CancellationToken ct = default);
}
