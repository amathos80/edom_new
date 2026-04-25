using eDom.Core.Interfaces;
using eDom.Infrastructure.Data;
using eDom.Infrastructure.Interceptors;
using eDom.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eDom.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "Oracle";
        var connectionString = configuration.GetConnectionString(provider)
            ?? throw new InvalidOperationException($"Connection string for provider '{provider}' not found.");

        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<HctDbContext>((sp, options) =>
        {
            switch (provider)
            {
                case "Oracle":
                    options.UseOracle(connectionString);
                    break;
                case "SqlServer":
                    options.UseSqlServer(connectionString);
                    break;
                case "PostgreSQL":
                    options.UseNpgsql(connectionString);
                    break;
                default:
                    throw new NotSupportedException($"Database provider '{provider}' is not supported.");
            }
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddScoped<IUtenteRepository, UtenteRepository>();
        services.AddScoped<IPazientiRepository, PazientiRepository>();
        services.AddScoped<ILogAccessoRepository, LogAccessoRepository>();
        services.AddScoped<IConfigurazioneRepository, ConfigurazioneRepository>();

        return services;
    }
}
