using System.Security.Cryptography;
using System.Text;
using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Utenti;

public sealed record CreaUtenteCommand(
    string Codice,
    string Cognome,
    string Nome,
    string? CodiceFiscale,
    string? Email,
    string? Matricola,
    bool FlagSmartCard,
    bool FlagCambiaPwd,
    DateTime? DataDisattivazione) : IRequest<UtenteDto>;

public sealed class CreaUtenteHandler(
    IRepository<Utente> repository,
    ICurrentUser currentUser)
    : IRequestHandler<CreaUtenteCommand, UtenteDto>
{
    public async Task<UtenteDto> HandleAsync(CreaUtenteCommand command, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var userId = currentUser.Id ?? 0;

        var utente = new Utente
        {
            Codice = command.Codice.Trim(),
            Cognome = command.Cognome.Trim(),
            Nome = command.Nome.Trim(),
            CodiceFiscale = string.IsNullOrWhiteSpace(command.CodiceFiscale) ? null : command.CodiceFiscale.Trim(),
            Email = string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim(),
            Matricola = string.IsNullOrWhiteSpace(command.Matricola) ? null : command.Matricola.Trim(),
            Password = HashPassword("1234"),
            FlagSmartCard = command.FlagSmartCard ? (short)1 : (short)0,
            FlagCambiaPwd = command.FlagCambiaPwd ? (short)1 : (short)0,
            DataDisattivazione = command.DataDisattivazione,
            DataScadenzaPassword = now,
            UtenteInserimento = userId,
            DataInserimento = now,
            UtenteModifica = userId,
            DataModifica = now
        };

        await repository.AddAsync(utente, ct);
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

    private static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA512.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}
