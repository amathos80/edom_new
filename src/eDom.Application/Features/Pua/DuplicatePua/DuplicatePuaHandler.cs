using eDom.Application.Mediator;
using eDom.Core.Interfaces;
using PuaEntity = eDom.Core.Entities.PuaRecord;

namespace eDom.Application.Features.Pua;

public sealed class DuplicatePuaHandler(
    IRepository<PuaEntity> repository,
    ICurrentUser currentUser)
    : IRequestHandler<DuplicatePuaCommand, PuaDto?>
{
    public async Task<PuaDto?> HandleAsync(DuplicatePuaCommand cmd, CancellationToken ct = default)
    {
        var source = await repository.GetByIdAsync(cmd.Id, ct);
        if (source is null)
        {
            return null;
        }

        var lastForCounter = await repository.GetAllAsync(
            filter: p => p.NumeroPuaId == source.NumeroPuaId,
            orderBy: src => src.OrderByDescending(p => p.Numero),
            take: 1,
            ct: ct);

        var nextNumero = lastForCounter.FirstOrDefault()?.Numero + 1 ?? 1;

        var row = new PuaEntity
        {
            NumeroPuaId = source.NumeroPuaId,
            Numero = nextNumero,
            Data = cmd.Data ?? source.Data,
            AreaInterventoId = source.AreaInterventoId,
            PazienteId = source.PazienteId,
            PazienteCognome = source.PazienteCognome,
            PazienteNome = source.PazienteNome,
            PazienteCodiceFiscale = source.PazienteCodiceFiscale,
            AccessoId = source.AccessoId,
            AccessoNote = source.AccessoNote,
            MotivoId = source.MotivoId,
            MotivoNote = source.MotivoNote,
            RichiestaId = source.RichiestaId,
            RichiestaAltro = source.RichiestaAltro,
            EsitoId = source.EsitoId,
            EsitoNote = source.EsitoNote,
            Urgente = source.Urgente,
            OrigineId = source.OrigineId,
            DataAvvio = source.DataAvvio,
            DataChiusura = null,
            MotivoChiusuraId = null,
            Attivo = 1,
            DataDisattivazione = null,
            UtenteInserimento = currentUser.Id ?? 0,
            DataInserimento = DateTime.UtcNow
        };

        await repository.AddAsync(row, ct);
        await repository.SaveChangesAsync(ct);

        return PuaMapper.ToDto(row);
    }
}
