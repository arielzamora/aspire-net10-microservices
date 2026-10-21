namespace MasterNet.Domain.Abstractions;
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<bool> SaveChangesAsync();
    Task RollbackAsync();
}
