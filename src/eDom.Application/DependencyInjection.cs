using eDom.Application.Features.Pazienti;
using eDom.Application.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace eDom.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Custom mediator
        services.AddScoped<IMediator, eDom.Application.Mediator.Mediator>();

        // Mapperly mappers (source-generated, zero runtime reflection)
        services.AddScoped<PazientiMapper>();

        // Auto-registrazione di tutti gli IRequestHandler<,> nell'assembly
        var assembly = typeof(DependencyInjection).Assembly;
        foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface))
        {
            foreach (var iface in type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            {
                services.AddScoped(iface, type);
            }
        }

        return services;
    }
}
