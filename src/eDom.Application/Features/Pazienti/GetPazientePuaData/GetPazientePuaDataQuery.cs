using eDom.Application.Mediator;
using eDom.Core.Models;

namespace eDom.Application.Features.Pazienti;

public record GetPazientePuaDataQuery(int Id) : IRequest<PazientePuaData?>;
