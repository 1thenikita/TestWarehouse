using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace TestWarehouse.Domain.Interfaces;

public interface IRepository<T, G> where T : class
{
    public DbSet<T> GetDbSet();

    public DbContext GetDbContext();

    public IEnumerable<T> GetAll();
    public Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);

    public IQueryable<T> AsQueryable();

    public void Add(T entity);

    public Task AddRange(IEnumerable<T> entities, CancellationToken stoppingToken = default);

    public void Update(T entity);

    public Task Remove(G id, CancellationToken stoppingToken = default);

    public void Remove(T entity);

    public void RemoveRange(IEnumerable<T> entities);

    public Task SaveChangesAsync(CancellationToken stoppingToken = default);

    public Task<T?> GetById(G id, CancellationToken stoppingToken = default);

    public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
}

public abstract class Repository<T, G> : IRepository<T, G> where T : class
{
    public abstract DbSet<T> GetDbSet();

    public abstract DbContext GetDbContext();

    public virtual IEnumerable<T> GetAll()
        => AsQueryable().ToList();

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await AsQueryable().ToListAsync(ct);

    public virtual IQueryable<T> AsQueryable()
        => GetDbSet().AsQueryable();

    public virtual void Add(T entity)
        => GetDbSet().Add(entity);

    public virtual async Task AddRange(IEnumerable<T> entities, CancellationToken stoppingToken = default)
        => await GetDbSet().AddRangeAsync(entities, stoppingToken);

    public virtual void Update(T entity)
    {
        GetDbContext().Entry(entity).State = EntityState.Modified;
    }

    public virtual async Task Remove(G id, CancellationToken stoppingToken = default)
    {
        T? entity = await GetById(id, stoppingToken);
        if (entity is not null)
        {
            Remove(entity);
        }
    }

    public virtual void Remove(T entity)
        => GetDbSet().Remove(entity);

    public virtual async Task SaveChangesAsync(CancellationToken stoppingToken = default)
        => await GetDbContext().SaveChangesAsync(stoppingToken);

    public virtual async Task<T?> GetById(G id, CancellationToken stoppingToken = default)
        => await AsQueryable().FirstOrDefaultAsync(e => EF.Property<G>(e, "ID")!.Equals(id), stoppingToken);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await AsQueryable().Where(predicate).ToListAsync(ct);

    public virtual void RemoveRange(IEnumerable<T> entities)
        => GetDbSet().RemoveRange(entities);
}