namespace eDom.Core.Interfaces;

public interface IConfigurazioneRepository
{
    /// <summary>
    /// Restituisce il valore del parametro <paramref name="codice"/> per la procedura indicata.
    /// Ritorna null se il parametro non esiste.
    /// </summary>
    Task<string?> GetValoreAsync(string codice, int procedureId = 999999, CancellationToken ct = default);
}
