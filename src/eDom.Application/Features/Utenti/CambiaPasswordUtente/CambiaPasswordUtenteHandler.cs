using System.Security.Cryptography;
using System.Text;
using eDom.Application.Features.Utenti;
using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;
using static eDom.Application.Common.Auth.AuthHelpers;


public sealed class CambiaPasswordUtenteHandler(
    IRepository<Utente> repository,
    ICurrentUser currentUser)
    : IRequestHandler<CambiaPasswordUtenteCommand, bool>
{
    public async Task<bool> HandleAsync(CambiaPasswordUtenteCommand command, CancellationToken ct = default)
    {
        // Use a tracked entity to avoid attaching a second instance with the same key.
        var utente = await repository.GetByIdAsync(command.UtenteId, ct);

        if (utente is null)
        {
            return false;
        }

        // Verifica password attuale
        var passwordAttualeHash = command.PasswordAttuale.HashPassword();
        if (utente.Password != passwordAttualeHash)
        {
            return false;
        }

        // Imposta nuova password
        utente.Password = command.PasswordNuova.HashPassword();
        utente.DataScadenzaPassword = DateTime.UtcNow.AddDays(90); // Scadenza 90 giorni
        utente.FlagCambiaPwd = 0; // Password valida, non forza cambio
        utente.UtenteModifica = currentUser.Id;
        utente.DataModifica = DateTime.UtcNow;

        await repository.SaveChangesAsync(ct);
        return true;
    }

    
}
