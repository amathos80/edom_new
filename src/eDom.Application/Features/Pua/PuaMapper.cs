using PuaEntity = eDom.Core.Entities.PuaRecord;

namespace eDom.Application.Features.Pua;

public static class PuaMapper
{
    public static PuaDto ToDto(PuaEntity entity) => new(
        entity.Id,
        entity.NumeroPuaId,
        entity.Numero,
        entity.Data,
        entity.AreaInterventoId,
        entity.PazienteId,
        entity.PazienteCognome,
        entity.PazienteNome,
        entity.PazienteCodiceFiscale,
        entity.AccessoId,
        entity.AccessoNote,
        entity.MotivoId,
        entity.MotivoNote,
        entity.RichiestaId,
        entity.RichiestaAltro,
        entity.EsitoId,
        entity.EsitoNote,
        entity.Urgente == 1,
        entity.OrigineId,
        entity.DataAvvio,
        entity.DataChiusura,
        entity.MotivoChiusuraId,
        entity.Attivo == 1);
}
