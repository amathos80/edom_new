using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Utenti;

public sealed record AggiornaUtenteCommand(
    int Id,
    string Codice,
    string Cognome,
    string Nome,
    string? CodiceFiscale,
    string? Email,
    string? Matricola,
    bool FlagSmartCard,
    bool FlagCambiaPwd,
    DateTime? DataDisattivazione) : IRequest<UtenteDto?>;

public sealed class AggiornaUtenteHandler(
    IRepository<Utente> repository,
    ICurrentUser currentUser)
    : IRequestHandler<AggiornaUtenteCommand, UtenteDto?>
{
    public async Task<UtenteDto?> HandleAsync(AggiornaUtenteCommand command, CancellationToken ct = default)
    {
        var utente = (await repository.GetAllAsync(
            filter: u => u.Id == command.Id,
            take: 1,
            ct: ct)).FirstOrDefault();

        if (utente is null)
        {
            return null;
        }

        utente.Codice = command.Codice.Trim();
        utente.Cognome = command.Cognome.Trim();
        utente.Nome = command.Nome.Trim();
        utente.CodiceFiscale = string.IsNullOrWhiteSpace(command.CodiceFiscale) ? null : command.CodiceFiscale.Trim();
        utente.Email = string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim();
        utente.Matricola = string.IsNullOrWhiteSpace(command.Matricola) ? null : command.Matricola.Trim();
        utente.FlagSmartCard = command.FlagSmartCard ? (short)1 : (short)0;
        utente.FlagCambiaPwd = command.FlagCambiaPwd ? (short)1 : (short)0;
        utente.DataDisattivazione = command.DataDisattivazione;
        utente.UtenteModifica = currentUser.Id;
        utente.DataModifica = DateTime.UtcNow;

        repository.Update(utente);
        await repository.SaveChangesAsync(ct);

        return new UtenteDto(
            utente.Id,
            utente.Codice,
            utente.Cognome,
            utente.Nome,
            utente.CodiceFiscale,
            utente.Email,
            utente.Matricola,
            utente.FlagSmartCard == 1,
            utente.FlagCambiaPwd == 1,
            utente.DataDisattivazione,
            utente.DataRiattivazione,
            utente.DataScadenzaPassword,
            utente.UltimoLogin,
            utente.DataInserimento,
            utente.DataModifica);
    }
}
