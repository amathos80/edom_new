using eDom.Application.Validation;

namespace eDom.Application.Features.Ruoli.Validation;

public sealed class AggiornaRuoloCommandValidator : IRequestValidator<AggiornaRuoloCommand>
{
    public Task ValidateAsync(AggiornaRuoloCommand request, RequestValidationContext context, CancellationToken ct = default)
    {
        if (request.Id <= 0)
        {
            context.AddError(nameof(request.Id), "L'identificativo del ruolo e obbligatorio.", "required");
        }

        if (request.ProceduraId <= 0)
        {
            context.AddError(nameof(request.ProceduraId), "La procedura e obbligatoria.", "required");
        }

        if (string.IsNullOrWhiteSpace(request.Codice))
        {
            context.AddError(nameof(request.Codice), "Il codice e obbligatorio.", "required");
        }
        else if (request.Codice.Trim().Length > 50)
        {
            context.AddError(nameof(request.Codice), "Il codice non puo superare 50 caratteri.", "maxLength");
        }

        if (string.IsNullOrWhiteSpace(request.Descrizione))
        {
            context.AddError(nameof(request.Descrizione), "La descrizione e obbligatoria.", "required");
        }
        else if (request.Descrizione.Trim().Length > 200)
        {
            context.AddError(nameof(request.Descrizione), "La descrizione non puo superare 200 caratteri.", "maxLength");
        }

        return Task.CompletedTask;
    }
}
