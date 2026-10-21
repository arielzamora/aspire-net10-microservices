using MasterNet.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MasterNet.Persistence.Repositories;


public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    private readonly MasterNetDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(MasterNetDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
    }
}
