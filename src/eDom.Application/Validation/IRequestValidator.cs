namespace eDom.Application.Validation;

public interface IRequestValidator<in TRequest>
{
    Task ValidateAsync(TRequest request, RequestValidationContext context, CancellationToken ct = default);
}
