using eDom.Application.Mediator;

namespace eDom.Application.Features.Ruoli;

public sealed record OttieniRuoloFunzioniQuery(int RuoloId) : IRequest<IReadOnlyList<RuoloFunzioneNodoDto>?>;

public sealed class RuoloFunzioneNodoDto
{
    public int Id { get; init; }
    public string Codice { get; init; } = string.Empty;
    public string Descrizione { get; init; } = string.Empty;
    public int? ParentId { get; init; }
    public bool Selezionata { get; set; }
    public List<RuoloFunzioneNodoDto> Figlie { get; } = [];
}
