using eDom.Application.Mediator;
using eDom.Core.Interfaces;
using PuaEntity = eDom.Core.Entities.PuaRecord;

namespace eDom.Application.Features.Pua;

public sealed class UpdatePuaHandler(
    IRepository<PuaEntity> repository,
    ICurrentUser currentUser)
    : IRequestHandler<UpdatePuaCommand, PuaDto?>
{
    public async Task<PuaDto?> HandleAsync(UpdatePuaCommand cmd, CancellationToken ct = default)
    {
        var row = await repository.GetByIdAsync(cmd.Id, ct);
        if (row is null)
        {
            return null;
        }

        row.Data = cmd.Data;
        row.AreaInterventoId = cmd.AreaInterventoId;
        row.PazienteId = cmd.PazienteId;
        row.PazienteCognome = cmd.PazienteCognome;
        row.PazienteNome = cmd.PazienteNome;
        row.PazienteCodiceFiscale = cmd.PazienteCodiceFiscale;
        row.AccessoId = cmd.AccessoId;
        row.AccessoNote = cmd.AccessoNote;
        row.MotivoId = cmd.MotivoId;
        row.MotivoNote = cmd.MotivoNote;
        row.RichiestaId = cmd.RichiestaId;
        row.RichiestaAltro = cmd.RichiestaAltro;
        row.EsitoId = cmd.EsitoId;
        row.EsitoNote = cmd.EsitoNote;
        row.Urgente = cmd.Urgente ? (short)1 : (short)0;
        row.OrigineId = cmd.OrigineId;
        row.DataAvvio = cmd.DataAvvio;
        row.DataChiusura = cmd.DataChiusura;
        row.MotivoChiusuraId = cmd.MotivoChiusuraId;
        row.Attivo = cmd.Attivo ? (short)1 : (short)0;
        row.DataDisattivazione = cmd.Attivo ? null : (row.DataDisattivazione ?? DateTime.UtcNow);
        row.UtenteModifica = currentUser.Id;
        row.DataModifica = DateTime.UtcNow;

        repository.Update(row);
        await repository.SaveChangesAsync(ct);

        return PuaMapper.ToDto(row);
    }
}
