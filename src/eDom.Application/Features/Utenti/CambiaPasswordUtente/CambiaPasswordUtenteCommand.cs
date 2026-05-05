using System.Security.Cryptography;
using System.Text;
using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Utenti;

public sealed record CambiaPasswordUtenteCommand(
    int UtenteId,
    string PasswordAttuale,
    string PasswordNuova) : IRequest<bool>;

