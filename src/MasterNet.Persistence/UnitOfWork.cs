using MasterNet.Domain.Abstractions;
using MasterNet.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MasterNet.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly MasterNetDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(MasterNetDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        if (_repositories.ContainsKey(typeof(TEntity)))
        {
            return (IGenericRepository<TEntity>)_repositories[typeof(TEntity)];
        }

        var repository = new GenericRepository<TEntity>(_context);
        _repositories.Add(typeof(TEntity), repository);
        return repository;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task RollbackAsync()
    {
        await Task.Run(() => _context.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Unchanged));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}