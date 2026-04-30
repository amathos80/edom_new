using Microsoft.Extensions.DependencyInjection;

namespace eDom.Application.Validation;

public sealed class RequestValidationEngine(IServiceProvider serviceProvider) : IRequestValidationEngine
{
    public async Task ValidateAsync(object request, CancellationToken ct = default)
    {
        var requestType = request.GetType();
        var validatorType = typeof(IRequestValidator<>).MakeGenericType(requestType);
        var validators = serviceProvider.GetServices(validatorType).ToList();

        if (validators.Count == 0)
        {
            return;
        }

        var context = new RequestValidationContext();

        foreach (var validator in validators)
        {
            await ((dynamic)validator).ValidateAsync((dynamic)request, context, ct);
        }

        if (context.HasErrors)
        {
            throw new RequestValidationException(context.Errors);
        }
    }
}
