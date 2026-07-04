using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Wheelsell.DataAccess.Entities;

namespace Wheelsell.DataAccess.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);
    IQueryable<T> Query();
    Task AddAsync(T entity);
    void Update(T entity);
    Task SoftDeleteAsync(T entity);
    Task SaveChangesAsync();
}

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly WheelsellDbContext Context;
    protected readonly DbSet<T> Set;

    public Repository(WheelsellDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) =>
        await Set.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = Set;
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public IQueryable<T> Query() => Set.AsQueryable();

    public async Task AddAsync(T entity) => await Set.AddAsync(entity);

    public void Update(T entity) => Set.Update(entity);

    public Task SoftDeleteAsync(T entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        Set.Update(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await Context.SaveChangesAsync();
}
