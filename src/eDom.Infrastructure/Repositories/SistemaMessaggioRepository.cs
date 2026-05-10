using eDom.Core.Entities;
using eDom.Core.Interfaces;
using eDom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eDom.Infrastructure.Repositories;

public class SistemaMessaggioRepository(HctDbContext context)
    : Repository<SistemaMessaggio>(context), ISistemaMessaggioRepository
{
    public async Task<IReadOnlyList<SistemaMessaggio>> SearchAsync(
        string? classe,
        string? nome,
        string? lingua,
        bool soloAttivi,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(classe))
        {
            query = query.Where(m => EF.Functions.ILike(m.Classe, $"%{classe}%"));
        }

        if (!string.IsNullOrWhiteSpace(nome))
        {
            query = query.Where(m => EF.Functions.ILike(m.Nome, $"%{nome}%"));
        }

        if (!string.IsNullOrWhiteSpace(lingua))
        {
            query = query.Where(m => EF.Functions.ILike(m.Lingua, $"{lingua}%"));
        }

        if (soloAttivi)
        {
            query = query.Where(m => m.FlagAttivo == 1);
        }

        return await query
            .OrderBy(m => m.Classe)
            .ThenBy(m => m.Nome)
            .ThenBy(m => m.Lingua)
            .ToListAsync(ct);
    }

    public async Task<SistemaMessaggio?> GetByKeyAsync(
        string classe,
        string nome,
        string lingua,
        CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(
            m => m.Classe == classe
                && m.Nome == nome
                && EF.Functions.ILike(m.Lingua, lingua),
            ct);
    }
}