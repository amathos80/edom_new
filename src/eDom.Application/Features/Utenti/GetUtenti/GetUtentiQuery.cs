using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Utenti;

public sealed record CercaUtentiQuery(
    string? Codice,
    string? Cognome,
    string? Nome,
    bool? SoloAttivi) : IRequest<IEnumerable<UtenteDto>>;

public sealed record UtenteDto(
    int Id,
    string Codice,
    string Cognome,
    string Nome,
    string? CodiceFiscale,
    string? Email,
    string? Matricola,
    bool FlagSmartCard,
    bool FlagCambiaPwd,
    DateTime? DataDisattivazione,
    DateTime? DataRiattivazione,
    DateTime? DataScadenzaPassword,
    DateTime? UltimoLogin,
    DateTime DataInserimento,
    DateTime? DataModifica);

public sealed class CercaUtentiHandler(IRepository<Utente> repository)
    : IRequestHandler<CercaUtentiQuery, IEnumerable<UtenteDto>>
{
    public async Task<IEnumerable<UtenteDto>> HandleAsync(CercaUtentiQuery q, CancellationToken ct = default)
    {
        var utenti = await repository.GetAllAsync(
            filter: u =>
                (string.IsNullOrWhiteSpace(q.Codice) || u.Codice.Contains(q.Codice)) &&
                (string.IsNullOrWhiteSpace(q.Cognome) || u.Cognome.Contains(q.Cognome)) &&
                (string.IsNullOrWhiteSpace(q.Nome) || u.Nome.Contains(q.Nome)) &&
                (!q.SoloAttivi.HasValue || !q.SoloAttivi.Value || u.DataDisattivazione == null),
            orderBy: src => src.OrderBy(u => u.Cognome).ThenBy(u => u.Nome).ThenBy(u => u.Codice),
            ct: ct);

        return utenti.Select(u => new UtenteDto(
            u.Id,
            u.Codice,
            u.Cognome,
            u.Nome,
            u.CodiceFiscale,
            u.Email,
            u.Matricola,
            u.FlagSmartCard == 1,
            u.FlagCambiaPwd == 1,
            u.DataDisattivazione,
            u.DataRiattivazione,
            u.DataScadenzaPassword,
            u.UltimoLogin,
            u.DataInserimento,
            u.DataModifica));
    }
}
