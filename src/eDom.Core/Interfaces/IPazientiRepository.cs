using eDom.Core.Entities;

namespace eDom.Core.Interfaces;

public interface IPazientiRepository : IRepository<Paziente>
{
    Task<IEnumerable<Paziente>> SearchAsync(string? cognome, string? codiceFiscale, bool? attivo, CancellationToken ct = default);
    Task<Paziente?> GetByCodiceAsync(string codice, CancellationToken ct = default);
}
