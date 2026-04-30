using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace eDom.Application.Features.Auth;

public sealed class RefreshTokenHandler(
    IRepository<RefreshTokenSession> refreshTokenRepository,
    IRepository<Utente> utenteRepository,
    IUtenteRepository userProfileRepository,
    IConfiguration configuration)
    : IRequestHandler<RefreshTokenCommand, LoginResponse?>
{
    public async Task<LoginResponse?> HandleAsync(RefreshTokenCommand cmd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.RefreshToken))
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var tokenHash = AuthTokenFactory.ComputeSha256(cmd.RefreshToken);
        var sessions = await refreshTokenRepository.GetAllAsync(
            filter: t => t.TokenHash == tokenHash,
            take: 1,
            ct: ct);
        var session = sessions.FirstOrDefault();

        if (session is null)
        {
            return null;
        }

        // Reuse detection: a rotated token reused again indicates compromise.
        if (session.RevokedAtUtc.HasValue && session.ReplacedByTokenId.HasValue)
        {
            await RevokeFamilyAsync(session.FamilyId, now, cmd.IpAddress, "reuse-detected", ct);
            return null;
        }

        if (session.RevokedAtUtc.HasValue || session.ExpiresAtUtc <= now)
        {
            return null;
        }

        var utente = await utenteRepository.GetByIdAsync(session.UserId, ct);
        if (utente is null)
        {
            return null;
        }

        var profile = await userProfileRepository.OttieniProfiloAutorizzativoAsync(utente.Codice, ct);
        if (profile is null)
        {
            return null;
        }

        var (accessToken, accessExpiresAt, _) = AuthTokenFactory.BuildAccessToken(
            configuration,
            profile.Id,
            profile.Codice,
            profile.NomeCompleto,
            profile.Ruoli);

        var (newRefreshToken, newRefreshHash) = AuthTokenFactory.BuildRefreshToken();
        var refreshExpiresAt = now.AddHours(8);

        var replacement = new RefreshTokenSession
        {
            UserId = session.UserId,
            TokenHash = newRefreshHash,
            FamilyId = session.FamilyId,
            CreatedAtUtc = now,
            ExpiresAtUtc = refreshExpiresAt,
            CreatedByIp = cmd.IpAddress,
        };

        await refreshTokenRepository.AddAsync(replacement, ct);
        await refreshTokenRepository.SaveChangesAsync(ct);

        session.RevokedAtUtc = now;
        session.RevokedByIp = cmd.IpAddress;
        session.RevokedReason = "rotated";
        session.ReplacedByTokenId = replacement.Id;
        refreshTokenRepository.Update(session);
        await refreshTokenRepository.SaveChangesAsync(ct);

        var accessExpiresIn = (int)Math.Max(1, (accessExpiresAt - DateTime.UtcNow).TotalSeconds);
        var refreshExpiresIn = (int)Math.Max(1, (refreshExpiresAt - DateTime.UtcNow).TotalSeconds);

        return new LoginResponse(
            accessToken,
            accessExpiresIn,
            newRefreshToken,
            refreshExpiresIn,
            profile.Codice,
            profile.NomeCompleto,
            profile.Ruoli);
    }

    private async Task RevokeFamilyAsync(Guid familyId, DateTime now, string? ip, string reason, CancellationToken ct)
    {
        var activeFamilyTokens = await refreshTokenRepository.GetAllAsync(
            filter: t => t.FamilyId == familyId && t.RevokedAtUtc == null,
            ct: ct);

        foreach (var token in activeFamilyTokens)
        {
            token.RevokedAtUtc = now;
            token.RevokedByIp = ip;
            token.RevokedReason = reason;
            refreshTokenRepository.Update(token);
        }

        await refreshTokenRepository.SaveChangesAsync(ct);
    }
}
