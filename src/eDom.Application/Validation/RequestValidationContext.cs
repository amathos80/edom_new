namespace eDom.Application.Validation;

public sealed class RequestValidationContext
{
    private readonly List<ValidationError> _errors = [];

    public IReadOnlyCollection<ValidationError> Errors => _errors;
    public bool HasErrors => _errors.Count > 0;

    public void AddError(string field, string message, string? code = null)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            field = "request";
        }

        _errors.Add(new ValidationError(field, message, code));
    }
}
