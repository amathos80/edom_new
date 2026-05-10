using eDom.Application.Mediator;
using eDom.Core.Interfaces;
using PuaEntity = eDom.Core.Entities.PuaRecord;

namespace eDom.Application.Features.Pua;

public sealed class CreatePuaHandler(
    IRepository<PuaEntity> repository,
    ICurrentUser currentUser)
    : IRequestHandler<CreatePuaCommand, PuaDto>
{
    public async Task<PuaDto> HandleAsync(CreatePuaCommand cmd, CancellationToken ct = default)
    {
        var lastForCounter = await repository.GetAllAsync(
            filter: p => p.NumeroPuaId == cmd.NumeroPuaId,
            orderBy: src => src.OrderByDescending(p => p.Numero),
            take: 1,
            ct: ct);

        var nextNumero = lastForCounter.FirstOrDefault()?.Numero + 1 ?? 1;

        var row = new PuaEntity
        {
            NumeroPuaId = cmd.NumeroPuaId,
            Numero = nextNumero,
            Data = cmd.Data,
            AreaInterventoId = cmd.AreaInterventoId,
            PazienteId = cmd.PazienteId,
            PazienteCognome = cmd.PazienteCognome,
            PazienteNome = cmd.PazienteNome,
            PazienteCodiceFiscale = cmd.PazienteCodiceFiscale,
            AccessoId = cmd.AccessoId,
            AccessoNote = cmd.AccessoNote,
            MotivoId = cmd.MotivoId,
            MotivoNote = cmd.MotivoNote,
            RichiestaId = cmd.RichiestaId,
            RichiestaAltro = cmd.RichiestaAltro,
            EsitoId = cmd.EsitoId,
            EsitoNote = cmd.EsitoNote,
            Urgente = cmd.Urgente ? (short)1 : (short)0,
            OrigineId = cmd.OrigineId,
            DataAvvio = cmd.DataAvvio,
            DataChiusura = cmd.DataChiusura,
            MotivoChiusuraId = cmd.MotivoChiusuraId,
            Attivo = cmd.Attivo ? (short)1 : (short)0,
            DataDisattivazione = cmd.Attivo ? null : DateTime.UtcNow,
            UtenteInserimento = currentUser.Id ?? 0,
            DataInserimento = DateTime.UtcNow
        };

        await repository.AddAsync(row, ct);
        await repository.SaveChangesAsync(ct);

        return PuaMapper.ToDto(row);
    }
}
