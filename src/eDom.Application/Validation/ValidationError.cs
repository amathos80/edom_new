namespace eDom.Application.Validation;

public sealed record ValidationError(string Field, string Message, string? Code = null);
