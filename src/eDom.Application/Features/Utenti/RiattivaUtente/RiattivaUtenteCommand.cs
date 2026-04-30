using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Utenti;

public sealed record RiattivaUtenteCommand(int UtenteId) : IRequest<bool>;

public sealed class RiattivaUtenteHandler(
    IRepository<Utente> repository,
    ICurrentUser currentUser)
    : IRequestHandler<RiattivaUtenteCommand, bool>
{
    public async Task<bool> HandleAsync(RiattivaUtenteCommand command, CancellationToken ct = default)
    {
        var utente = (await repository.GetAllAsync(
            filter: u => u.Id == command.UtenteId,
            take: 1,
            ct: ct)).FirstOrDefault();

        if (utente is null)
        {
            return false;
        }

        utente.DataDisattivazione = null;
        utente.DataRiattivazione = DateTime.UtcNow;
        utente.UtenteModifica = currentUser.Id;
        utente.DataModifica = DateTime.UtcNow;

        repository.Update(utente);
        await repository.SaveChangesAsync(ct);
        return true;
    }
}
