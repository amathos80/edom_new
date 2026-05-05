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

