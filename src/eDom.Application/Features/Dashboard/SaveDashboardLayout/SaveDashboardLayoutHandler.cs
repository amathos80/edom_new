using eDom.Application.Mediator;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Dashboard;

public sealed class SaveDashboardLayoutHandler(IUserDashboardLayoutRepository repository)
    : IRequestHandler<SaveDashboardLayoutCommand, DashboardLayoutDto>
{
    public async Task<DashboardLayoutDto> HandleAsync(SaveDashboardLayoutCommand request, CancellationToken ct = default)
    {
        await repository.UpsertAsync(request.UserCodice, request.LayoutJson, ct);

        var saved = await repository.GetByUserAsync(request.UserCodice, ct);
        return new DashboardLayoutDto(1, saved!.UpdatedAt.ToString("O"), saved.LayoutJson);
    }
}
