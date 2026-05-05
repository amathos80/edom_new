using eDom.Application.Features.Utenti;
using eDom.Application.Validation;
using eDom.Core.Entities;
using eDom.Core.Interfaces;
using eDom.Application.Common.Auth;

namespace eDom.Application.Features.Ruoli.Validation;

public sealed class CambioPasswordCommandValidator(IRepository<Utente> utenteRepository) 
: IRequestValidator<CambiaPasswordUtenteCommand>
{

    public async Task ValidateAsync(
        CambiaPasswordUtenteCommand request, RequestValidationContext context,
         CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.PasswordAttuale))
        {
            context.AddError(nameof(request.PasswordAttuale), "La password attuale è obbligatoria.", "required");
        }

        if (string.IsNullOrWhiteSpace(request.PasswordNuova))
        {
            context.AddError(nameof(request.PasswordNuova), "La password nuova è obbligatoria.", "required");
        }

        if (request.PasswordNuova.Length < 6)
        {
            context.AddError(nameof(request.PasswordNuova), "La password nuova deve essere almeno di 6 caratteri.", "minLength");
        }

        var oldPasswordHash = (await utenteRepository.GetByIdAsync(request.UtenteId, ct))?.Password;
        
        if (request.PasswordNuova.HashPassword().Equals(oldPasswordHash)){
            context.AddError(nameof(request.PasswordNuova),
             "La password nuova non può essere uguale alla password attuale.", "invalid");
        }

        if (!PasswordPolicy.IsPasswordValid(request.PasswordNuova))
        {
            context.AddError(nameof(request.PasswordNuova), "La password nuova non soddisfa i requisiti di sicurezza.", "invalid");
        }

        if (request.PasswordAttuale.Equals(request.PasswordNuova))
        {
            context.AddError(nameof(request.PasswordNuova), "La password nuova deve essere diversa dalla password attuale.", "invalid");
            context.AddError(nameof(request.PasswordAttuale), "La password attuale deve essere diversa dalla password nuova.", "invalid");
        }

        return;
    }
}
