using eDom.Application.Mediator;

namespace eDom.Application.Features.Auth;

public record OttieniPermessiCommand : IRequest<RispostaPermessi?>;

public record RispostaPermessi(string Username, string FullName, IList<string> Ruoli, IList<string> Funzioni);