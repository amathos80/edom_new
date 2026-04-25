using System.Security.Claims;
using eDom.Core.Interfaces;

namespace eDom.Api.Services;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public int? Id =>
        int.TryParse(accessor.HttpContext?.User.FindFirstValue("uid"), out var id) ? id : null;

    public string? Username =>
        accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)
        ?? accessor.HttpContext?.User.FindFirstValue("unique_name");
}
