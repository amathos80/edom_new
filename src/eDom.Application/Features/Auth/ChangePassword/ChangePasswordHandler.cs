using System.Security.Cryptography;
using System.Text;
using eDom.Application.Mediator;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Auth;

public sealed class ChangePasswordHandler(
    IUtenteRepository utenteRepository,
    ICurrentUser currentUser)
    : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    public async Task<ChangePasswordResponse> HandleAsync(ChangePasswordCommand cmd, CancellationToken ct = default)
    {
        if (currentUser.Id is null)
            return new ChangePasswordResponse(false, "Utente non autenticato.");

        // FindAsync → entità tracked → SaveChangesAsync attiverà AuditInterceptor
        var utente = await utenteRepository.GetByIdAsync(currentUser.Id.Value, ct);
        if (utente is null)
            return new ChangePasswordResponse(false, "Utente non trovato.");

        // Verifica password attuale
        var oldHash = HashPassword(cmd.OldPassword);
        if (utente.Password != oldHash)
            return new ChangePasswordResponse(false, "La password attuale non è corretta.");

        // Nuova password non può essere uguale alla precedente
        var newHash = HashPassword(cmd.NewPassword);
        if (newHash == oldHash)
            return new ChangePasswordResponse(false, "La nuova password deve essere diversa da quella attuale.");

        // Nuova password non può essere la password di default
        if (cmd.NewPassword == "1234")
            return new ChangePasswordResponse(false, "La password scelta non è consentita.");

        utente.Password = newHash;
        utente.FlagCambiaPwd = 0;

        await utenteRepository.SaveChangesAsync(ct);

        return new ChangePasswordResponse(true);
    }

    private static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA512.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}
