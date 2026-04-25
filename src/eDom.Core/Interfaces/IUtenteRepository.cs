using eDom.Core.Entities;

namespace eDom.Core.Interfaces;

public interface IUtenteRepository : IRepository<Utente>
{
    Task<Utente?> GetByCodiceAsync(string codice, CancellationToken ct = default);
    Task<Utente?> GetByCodiceWithRuoliAsync(string codice, CancellationToken ct = default);
    /// <summary>
    /// Aggiorna UTEN_LASTLOGIN tramite ExecuteUpdateAsync (senza passare per il change tracker
    /// e quindi senza generare record di audit).
    /// </summary>
    Task UpdateUltimoLoginAsync(int utenteId, DateTime ultimoLogin, CancellationToken ct = default);
}
