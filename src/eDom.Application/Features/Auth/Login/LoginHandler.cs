using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace eDom.Application.Features.Auth;

public sealed class LoginHandler(
    IUtenteRepository utenteRepository,
    ILogAccessoRepository logAccessoRepository,
    IConfigurazioneRepository configurazioneRepository,
    IConfiguration configuration)
    : IRequestHandler<LoginCommand, LoginResponse?>
{
    public async Task<LoginResponse?> HandleAsync(LoginCommand cmd, CancellationToken ct = default)
    {
        var hashedPassword = HashPassword(cmd.Password);
        var utente = await utenteRepository.GetByCodiceWithRuoliAsync(cmd.Username, ct);

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

        var roles = utente.Ruoli.Select(r => r.Codice).ToList();
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

        var token = GenerateToken(utente.Id, utente.Codice, utente.NomeCompleto, roles);
        int.TryParse(configuration["JwtSettings:ExpiryMinutes"], out int expiryMinutes);
        if (expiryMinutes == 0) expiryMinutes = 60;

        return new LoginResponse(token, expiryMinutes * 60, utente.Codice, utente.NomeCompleto, roles, mustChangePassword);
    }

    private string GenerateToken(int userId, string username, string fullName, IList<string> roles)
    {
        var secret = configuration["JwtSettings:Secret"]!;
        var issuer = configuration["JwtSettings:Issuer"]!;
        var audience = configuration["JwtSettings:Audience"]!;
        var expiryMinutes = int.TryParse(configuration["JwtSettings:ExpiryMinutes"], out int exp) ? exp : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("uid", userId.ToString()),
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.GivenName, fullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
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
