using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Ruoli;

public sealed record AggiornaRuoloFunzioniCommand(int RuoloId, IReadOnlyList<int> FunzioneIds) : IRequest<bool>;

public sealed class AggiornaRuoloFunzioniHandler(
    IRepository<Ruolo> ruoloRepository,
    IRepository<Funzione> funzioneRepository,
    IRepository<RuoloFunzione> ruoloFunzioneRepository,
    ICurrentUser currentUser)
    : IRequestHandler<AggiornaRuoloFunzioniCommand, bool>
{
    public async Task<bool> HandleAsync(AggiornaRuoloFunzioniCommand command, CancellationToken ct = default)
    {
        var ruolo = (await ruoloRepository.GetAllAsync(
            filter: r => r.Id == command.RuoloId,
            take: 1,
            ct: ct)).FirstOrDefault();
        if (ruolo is null)
        {
            return false;
        }

        var requestedIds = (command.FunzioneIds ?? [])
            .Where(id => id > 0)
            .Distinct()
            .ToHashSet();

        var funzioniValide = await funzioneRepository.GetAllAsync(
            filter: f => requestedIds.Contains(f.Id) && f.ProcedureId == ruolo.ProcedureId,
            ct: ct);

        var validRequestedIds = funzioniValide
            .Select(f => f.Id)
            .ToHashSet();

        var esistenti = await ruoloFunzioneRepository.GetAllAsync(
            filter: rf => rf.RuoloId == command.RuoloId,
            ct: ct);

        var existingByFunzione = esistenti
            .ToDictionary(rf => rf.FunzioneId, rf => rf);

        var changed = false;
        var now = DateTime.UtcNow;
        var userId = currentUser.Id ?? 0;

        foreach (var esistente in esistenti)
        {
            if (!validRequestedIds.Contains(esistente.FunzioneId))
            {
                ruoloFunzioneRepository.Remove(esistente);
                changed = true;
            }
        }

        var funzioniValideById = funzioniValide.ToDictionary(f => f.Id, f => f);
        foreach (var funzioneId in validRequestedIds)
        {
            if (existingByFunzione.ContainsKey(funzioneId))
            {
                continue;
            }

            var funzione = funzioniValideById[funzioneId];
            await ruoloFunzioneRepository.AddAsync(new RuoloFunzione
            {
                RuoloId = command.RuoloId,
                FunzioneId = funzioneId,
                RuoloProcedureId = ruolo.ProcedureId,
                FunzioneProcedureId = funzione.ProcedureId,
                UtenteInserimento = userId,
                DataInserimento = now,
                UtenteModifica = userId,
                DataModifica = now,
            }, ct);
            changed = true;
        }

        if (changed)
        {
            await ruoloFunzioneRepository.SaveChangesAsync(ct);
        }

        return true;
    }
}
