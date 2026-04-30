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