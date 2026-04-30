using eDom.Application.Mediator;

namespace eDom.Application.Features.Ruoli;

public record AggiornaRuoloCommand(
    int Id,
    int ProceduraId,
    string Codice,
    string Descrizione,
    bool FlagAmministratore) : IRequest<RuoloDto?>;
