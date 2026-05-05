using eDom.Application.Mediator;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Auth;

public sealed class OttieniPermessiHandler(
    ICurrentUser currentUser,
    IUtenteRepository utenteRepository)
    : IRequestHandler<OttieniPermessiCommand, RispostaPermessi?>
{
    public async Task<RispostaPermessi?> HandleAsync(OttieniPermessiCommand cmd, CancellationToken ct = default)
    {
        // Prefer stable numeric id from token when available.
        if (currentUser.Id is int userId)
        {
            var utente = await utenteRepository.GetByIdAsync(userId, ct);
            if (utente is not null)
            {
                var profiloById = await utenteRepository.OttieniProfiloAutorizzativoAsync(utente.Codice, ct);
                if (profiloById is not null)
                {
                    return new RispostaPermessi(
                        profiloById.Codice,
                        profiloById.NomeCompleto,
                        profiloById.Ruoli,
                        profiloById.Funzioni);
                }
            }
        }

        var username = currentUser.Username;
        if (string.IsNullOrWhiteSpace(username))
            return null;

        var profilo = await utenteRepository.OttieniProfiloAutorizzativoAsync(username, ct);
        if (profilo is null)
            return null;

        return new RispostaPermessi(
            profilo.Codice,
            profilo.NomeCompleto,
            profilo.Ruoli,
            profilo.Funzioni);
    }
}