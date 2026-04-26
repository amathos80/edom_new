using eDom.Application.Mediator;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Dashboard;

public sealed class GetDashboardLayoutHandler(IUserDashboardLayoutRepository repository)
    : IRequestHandler<GetDashboardLayoutQuery, DashboardLayoutDto?>
{
    public async Task<DashboardLayoutDto?> HandleAsync(GetDashboardLayoutQuery request, CancellationToken ct = default)
    {
        var entity = await repository.GetByUserAsync(request.UserCodice, ct);
        if (entity is null) return null;

        return new DashboardLayoutDto(1, entity.UpdatedAt.ToString("O"), entity.LayoutJson);
    }
}
