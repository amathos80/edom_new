using System.Linq.Expressions;
using eDom.Core.Interfaces;
using eDom.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eDom.Infrastructure.Repositories;

public class Repository<T>(HctDbContext context) : IRepository<T> where T : class
{
    protected readonly HctDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default) =>
        await DbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await DbSet.AsNoTracking().Where(predicate).ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await DbSet.AddAsync(entity, ct);

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Context.SaveChangesAsync(ct);
}
