using eDom.Application.Mediator;

namespace eDom.Application.Features.SistemaMessaggi;

public record GetSistemaMessaggioByChiaveQuery(
    string Classe,
    string Nome,
    string Lingua) : IRequest<SistemaMessaggioDto?>;
