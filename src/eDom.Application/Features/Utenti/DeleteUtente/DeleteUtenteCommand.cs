using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Utenti;

public sealed record EliminaUtenteCommand(int Id) : IRequest<bool>;

public sealed class EliminaUtenteHandler(IRepository<Utente> repository)
    : IRequestHandler<EliminaUtenteCommand, bool>
{
    public async Task<bool> HandleAsync(EliminaUtenteCommand command, CancellationToken ct = default)
    {
        var utente = (await repository.GetAllAsync(
            filter: u => u.Id == command.Id,
            take: 1,
            ct: ct)).FirstOrDefault();

        if (utente is null)
        {
            return false;
        }

        repository.Remove(utente);
        await repository.SaveChangesAsync(ct);
        return true;
    }
}
