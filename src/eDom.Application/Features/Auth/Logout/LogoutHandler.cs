using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Auth;

public sealed class LogoutHandler(
    ICurrentUser currentUser,
    IRepository<RefreshTokenSession> refreshTokenRepository,
    IRepository<UserTokenState> userTokenStateRepository)
    : IRequestHandler<LogoutCommand, bool>
{
    public async Task<bool> HandleAsync(LogoutCommand cmd, CancellationToken ct = default)
    {
        if (!currentUser.Id.HasValue)
        {
            return false;
        }

        var userId = currentUser.Id.Value;
        var now = DateTime.UtcNow;

        var activeSessions = await refreshTokenRepository.GetAllAsync(
            filter: t => t.UserId == userId && t.RevokedAtUtc == null,
            ct: ct);

        foreach (var token in activeSessions)
        {
            token.RevokedAtUtc = now;
            token.RevokedByIp = cmd.IpAddress;
            token.RevokedReason = "logout";
            refreshTokenRepository.Update(token);
        }

        if (activeSessions.Count > 0)
        {
            await refreshTokenRepository.SaveChangesAsync(ct);
        }

        var states = await userTokenStateRepository.GetAllAsync(filter: s => s.UserId == userId, take: 1, ct: ct);
        var state = states.FirstOrDefault();

        if (state is null)
        {
            await userTokenStateRepository.AddAsync(new UserTokenState
            {
                UserId = userId,
                InvalidBeforeUtc = now,
                UpdatedAtUtc = now,
            }, ct);
        }
        else
        {
            state.InvalidBeforeUtc = now;
            state.UpdatedAtUtc = now;
            userTokenStateRepository.Update(state);
        }

        await userTokenStateRepository.SaveChangesAsync(ct);
        return true;
    }
}
