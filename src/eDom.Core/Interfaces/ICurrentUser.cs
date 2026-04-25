namespace eDom.Core.Interfaces;

/// <summary>
/// Rappresenta l'utente correntemente autenticato nella richiesta HTTP.
/// Implementato nel layer Api tramite IHttpContextAccessor.
/// </summary>
public interface ICurrentUser
{
    int? Id { get; }
    string? Username { get; }
}
