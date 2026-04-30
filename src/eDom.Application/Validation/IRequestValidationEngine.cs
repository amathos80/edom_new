namespace eDom.Application.Validation;

public interface IRequestValidationEngine
{
    Task ValidateAsync(object request, CancellationToken ct = default);
}
