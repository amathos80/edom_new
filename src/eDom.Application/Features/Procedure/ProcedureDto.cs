namespace eDom.Application.Features.Procedure;

public record ProcedureDto(
    int Id,
    string Codice,
    string Descrizione,
    bool Visibile);
