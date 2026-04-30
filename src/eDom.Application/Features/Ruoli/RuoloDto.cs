namespace eDom.Application.Features.Ruoli;

public record RuoloDto(
    int Id,
    int ProceduraId,
    string Codice,
    string Descrizione,
    bool FlagAmministratore,
    DateTime DataInserimento,
    DateTime? DataModifica,
    string? ProceduraCodice = null);
