using eDom.Application.Mediator;

namespace eDom.Application.Features.Dashboard;

public record SaveDashboardLayoutCommand(string UserCodice, string LayoutJson) : IRequest<DashboardLayoutDto>;
