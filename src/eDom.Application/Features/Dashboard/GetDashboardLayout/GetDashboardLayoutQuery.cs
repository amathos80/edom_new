using eDom.Application.Mediator;

namespace eDom.Application.Features.Dashboard;

public record GetDashboardLayoutQuery(string UserCodice) : IRequest<DashboardLayoutDto?>;
