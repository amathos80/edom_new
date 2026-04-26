namespace eDom.Application.Features.Dashboard;

/// <summary>DTO che rispecchia il contratto del frontend per il layout dashboard.</summary>
public record DashboardLayoutDto(
    int SchemaVersion,
    string UpdatedAt,
    string LayoutJson);
