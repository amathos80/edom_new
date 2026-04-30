using Microsoft.Extensions.DependencyInjection;
using eDom.Application.Validation;

namespace eDom.Application.Mediator;

/// <summary>
/// Implementazione custom del Mediator pattern.
/// Risolve l'handler corrispondente al tipo di request tramite DI
/// ed invoca HandleAsync senza dipendere da librerie esterne.
/// </summary>
public sealed class Mediator(
    IServiceProvider serviceProvider,
    IRequestValidationEngine validationEngine) : IMediator
{
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        await validationEngine.ValidateAsync(request, ct);

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        dynamic handler = serviceProvider.GetRequiredService(handlerType);
        return await handler.HandleAsync((dynamic)request, ct);
    }
}
