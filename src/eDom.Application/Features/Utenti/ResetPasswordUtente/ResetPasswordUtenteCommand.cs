using System.Security.Cryptography;
using System.Text;
using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Utenti;

public sealed record ResetPasswordUtenteCommand(int UtenteId) : IRequest<bool>;

public sealed class ResetPasswordUtenteHandler(
    IRepository<Utente> repository,
    ICurrentUser currentUser)
    : IRequestHandler<ResetPasswordUtenteCommand, bool>
{
    public async Task<bool> HandleAsync(ResetPasswordUtenteCommand command, CancellationToken ct = default)
    {
        var utente = (await repository.GetAllAsync(
            filter: u => u.Id == command.UtenteId,
            take: 1,
            ct: ct)).FirstOrDefault();

        if (utente is null)
        {
            return false;
        }

        utente.Password = HashPassword("1234");
        utente.FlagCambiaPwd = 1;
        utente.DataScadenzaPassword = DateTime.UtcNow;
        utente.UtenteModifica = currentUser.Id;
        utente.DataModifica = DateTime.UtcNow;

        repository.Update(utente);
        await repository.SaveChangesAsync(ct);
        return true;
    }

    private static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA512.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}
