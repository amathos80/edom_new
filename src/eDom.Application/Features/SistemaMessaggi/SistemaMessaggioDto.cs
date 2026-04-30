namespace eDom.Application.Features.SistemaMessaggi;

public record SistemaMessaggioDto(
    int Id,
    string Classe,
    string Nome,
    string Descrizione,
    string Lingua,
    string? Custom01,
    string? Custom02,
    string? Custom03,
    string? Custom04,
    string? Custom05,
    bool Attivo);
