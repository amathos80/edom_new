using eDom.Application.Mediator;

namespace eDom.Application.Features.SistemaMessaggi;

public record GetSistemaMessaggiQuery(
    string? Classe,
    string? Nome,
    string? Lingua,
    bool SoloAttivi) : IRequest<IEnumerable<SistemaMessaggioDto>>;
