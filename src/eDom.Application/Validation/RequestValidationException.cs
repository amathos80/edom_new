namespace eDom.Application.Validation;

public sealed class RequestValidationException : Exception
{
    public RequestValidationException(IReadOnlyCollection<ValidationError> errors)
        : base("Validation failed for request.")
    {
        Errors = errors;
    }

    public IReadOnlyCollection<ValidationError> Errors { get; }
}
