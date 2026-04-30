using System.Net;
using System.Security.Cryptography;
using System.Text;
using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace eDom.Application.Features.Auth;

public sealed class LoginHandler(
    IUtenteRepository utenteRepository,
    ILogAccessoRepository logAccessoRepository,
    IConfigurazioneRepository configurazioneRepository,
    IRepository<RefreshTokenSession> refreshTokenRepository,
    IConfiguration configuration)
    : IRequestHandler<LoginCommand, LoginResponse?>
{
    public async Task<LoginResponse?> HandleAsync(LoginCommand cmd, CancellationToken ct = default)
    {
        var hashedPassword = HashPassword(cmd.Password);
        var utente = await utenteRepository.OttieniProfiloAutorizzativoAsync(cmd.Username, ct);

        // Credenziali errate o utente inesistente
        if (utente is null || utente.Password != hashedPassword)
            return null;

        // Account disattivato esplicitamente
        if (utente.DataDisattivazione.HasValue && utente.DataDisattivazione.Value <= DateTime.UtcNow)
            return null;

        // Check inattività: se UTEN_LASTLOGIN è valorizzato e sono passati più di USER_MAX_INACT giorni
        if (utente.UltimoLogin.HasValue)
        {
            var maxInattValStr = await configurazioneRepository.GetValoreAsync("USER_MAX_INACT", ct: ct);
            if (maxInattValStr is not null && double.TryParse(maxInattValStr, out double maxInattDays))
            {
                if (utente.UltimoLogin.Value.Date.AddDays(maxInattDays) <= DateTime.UtcNow.Date)
                    return null;
            }
        }

        // --- Deve cambiare password? ---
        var hashedDefault = HashPassword("1234");
        
        bool mustChangePassword =
            !utente.UltimoLogin.HasValue ||                                                              // primo accesso
            (utente.DataScadenzaPassword.HasValue && utente.DataScadenzaPassword.Value.Date < DateTime.UtcNow.Date) || // password scaduta
            utente.Password == hashedDefault ||                                                         // password default "1234"
            utente.FlagCambiaPwd == 1;                                                                  // flag esplicito admin

        var ruoli = utente.Ruoli;
        var now = DateTime.UtcNow;

        // Aggiorna UTEN_LASTLOGIN (ExecuteUpdate — bypass change tracker, nessun audit record)
        await utenteRepository.UpdateUltimoLoginAsync(utente.Id, now, ct);

        // Scrivi il record di accesso su SI_LOGACC
        await logAccessoRepository.InsertAsync(new LogAccesso
        {
            UtenteId    = utente.Id,
            Data        = now,
            IndirizzoIp = cmd.IpAddress,
            NomeMacchina = await ResolveHostnameAsync(cmd.IpAddress),
            ProcedureId = 999999,
            FunzioneId  = 999999
        }, ct);

        var (accessToken, accessExpiresAt, _) = AuthTokenFactory.BuildAccessToken(
            configuration,
            utente.Id,
            utente.Codice,
            utente.NomeCompleto,
            ruoli);

        var (refreshToken, refreshHash) = AuthTokenFactory.BuildRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddHours(8);

        await refreshTokenRepository.AddAsync(new RefreshTokenSession
        {
            UserId = utente.Id,
            TokenHash = refreshHash,
            FamilyId = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = refreshExpiresAt,
            CreatedByIp = cmd.IpAddress
        }, ct);
        await refreshTokenRepository.SaveChangesAsync(ct);

        var accessExpiresIn = (int)Math.Max(1, (accessExpiresAt - DateTime.UtcNow).TotalSeconds);
        var refreshExpiresIn = (int)Math.Max(1, (refreshExpiresAt - DateTime.UtcNow).TotalSeconds);

        return new LoginResponse(
            accessToken,
            accessExpiresIn,
            refreshToken,
            refreshExpiresIn,
            utente.Codice,
            utente.NomeCompleto,
            ruoli,
            mustChangePassword);
    }

    private static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA512.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }

    private static async Task<string?> ResolveHostnameAsync(string? ip)
    {
        if (string.IsNullOrEmpty(ip)) return null;
        try
        {
            var entry = await Dns.GetHostEntryAsync(ip);
            return entry.HostName;
        }
        catch
        {
            return "Unknown";
        }
    }
}
