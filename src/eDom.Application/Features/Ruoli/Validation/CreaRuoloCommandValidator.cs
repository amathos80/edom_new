using eDom.Application.Validation;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Ruoli.Validation;

public sealed class CreaRuoloCommandValidator : IRequestValidator<CreaRuoloCommand>
{
    private readonly IRepository<Ruolo> ruoloRepository;

    public CreaRuoloCommandValidator(
        IRepository<Ruolo> ruoloRepository)
    {
        this.ruoloRepository = ruoloRepository;
    }

    public async Task ValidateAsync(
        CreaRuoloCommand request, RequestValidationContext context,
         CancellationToken ct = default)
    {
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

          var codice = request.Codice.Trim();
          var codiceGiaUsato = await this.ruoloRepository.GetAllAsync(
            r => r.ProcedureId == request.ProceduraId && r.Codice == codice, ct:ct);
          if (codiceGiaUsato.Any())
          {
              context.AddError(nameof(request.Codice), "Il codice deve essere univoco per la procedura.", "unique");
          }

        if (string.IsNullOrWhiteSpace(request.Descrizione))
        {
            context.AddError(nameof(request.Descrizione), "La descrizione e obbligatoria.", "required");
        }
        else if (request.Descrizione.Trim().Length > 200)
        {
            context.AddError(nameof(request.Descrizione), "La descrizione non puo superare 200 caratteri.", "maxLength");
        }

        return;
    }
}
