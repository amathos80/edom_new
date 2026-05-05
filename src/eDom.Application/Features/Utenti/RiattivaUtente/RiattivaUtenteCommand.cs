using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Utenti;

public sealed record RiattivaUtenteCommand(int UtenteId) : IRequest<bool>;

