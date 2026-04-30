using System.Linq.Expressions;
using eDom.Application.Common.Filtering;
using eDom.Application.Mediator;
using eDom.Core.Entities;
using eDom.Core.Interfaces;

namespace eDom.Application.Features.Ruoli;

public class CercaRuoliPaginataHandler : IRequestHandler<CercaRuoliPaginataQuery, RispostaPaginata<RuoloDto>>
{
    private static readonly IReadOnlyDictionary<string, string> FieldMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = nameof(Ruolo.Id),
            ["proceduraid"] = nameof(Ruolo.ProcedureId),
            ["codice"] = nameof(Ruolo.Codice),
            ["descrizione"] = nameof(Ruolo.Descrizione),
            ["flagamministratore"] = nameof(Ruolo.FlagAdmin),
            ["datainserimento"] = nameof(Ruolo.DataInserimento)
        };

    private readonly IRepository<Ruolo> _repository;
    private readonly IRepository<Procedura> _proceduraRepository;

    public CercaRuoliPaginataHandler(
        IRepository<Ruolo> repository,
        IRepository<Procedura> proceduraRepository)
    {
        _repository = repository;
        _proceduraRepository = proceduraRepository;
    }

    public async Task<RispostaPaginata<RuoloDto>> HandleAsync(CercaRuoliPaginataQuery request, CancellationToken ct)
    {
        Expression<Func<Ruolo, bool>> filter = r => true;

        var structuredFilter = QueryFilterEngine.Parse(request.Filter);
        if (structuredFilter is not null)
        {
            var filterExpression = QueryFilterEngine.BuildPredicate<Ruolo>(structuredFilter, FieldMap);
            if (filterExpression is not null)
            {
                filter = filterExpression;
            }
        }

        // Costruisce la funzione di ordinamento
        Func<IQueryable<Ruolo>, IOrderedQueryable<Ruolo>> orderBy = query =>
        {
            if (!string.IsNullOrWhiteSpace(request.Sort))
            {
                var isDesc = request.Sort.StartsWith("-", StringComparison.Ordinal);
                var field = isDesc ? request.Sort[1..] : request.Sort;

                return field.ToLowerInvariant() switch
                {
                    "id" => isDesc ? query.OrderByDescending(r => r.Id) : query.OrderBy(r => r.Id),
                    "codice" => isDesc ? query.OrderByDescending(r => r.Codice) : query.OrderBy(r => r.Codice),
                    "descrizione" => isDesc ? query.OrderByDescending(r => r.Descrizione) : query.OrderBy(r => r.Descrizione),
                    "proceduraid" => isDesc ? query.OrderByDescending(r => r.ProcedureId) : query.OrderBy(r => r.ProcedureId),
                    "flagamministratore" => isDesc ? query.OrderByDescending(r => r.FlagAdmin) : query.OrderBy(r => r.FlagAdmin),
                    "datainserimento" => isDesc ? query.OrderByDescending(r => r.DataInserimento) : query.OrderBy(r => r.DataInserimento),
                    _ => query.OrderByDescending(r => r.DataInserimento)
                };
            }

            return query.OrderByDescending(r => r.DataInserimento);
        };

        // Conta il totale
        var allItems = await _repository.FindAsync(filter, ct);
        var totale = allItems.Count;

        // Recupera i dati paginati
        var items = await _repository.GetAllAsync(
            filter: filter,
            orderBy: orderBy,
            skip: request.Skip,
            take: request.Take,
            ct: ct
        );

        var proceduraIds = items
            .Select(r => r.ProcedureId)
            .Distinct()
            .ToArray();

        var procedure = await _proceduraRepository.GetAllAsync(
            filter: p => proceduraIds.Contains(p.Id),
            ct: ct);

        var proceduraCodici = procedure.ToDictionary(p => p.Id, p => p.Codice);

        // Mappa a DTO
        var dtos = items.Select(r => new RuoloDto(
            Id: r.Id,
            ProceduraId: r.ProcedureId,
            Codice: r.Codice,
            Descrizione: r.Descrizione,
            FlagAmministratore: r.FlagAdmin == 1,
            DataInserimento: r.DataInserimento,
            DataModifica: r.DataModifica,
            ProceduraCodice: proceduraCodici.GetValueOrDefault(r.ProcedureId)
        )).ToList();

        return new RispostaPaginata<RuoloDto>(dtos, totale, request.Skip, request.Take);
    }
}
