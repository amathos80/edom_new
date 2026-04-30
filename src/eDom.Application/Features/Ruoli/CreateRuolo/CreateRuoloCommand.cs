using eDom.Application.Mediator;

namespace eDom.Application.Features.Ruoli;

public record CreaRuoloCommand(
    int ProceduraId,
    string Codice,
    string Descrizione,
    bool FlagAmministratore) : IRequest<RuoloDto>;
