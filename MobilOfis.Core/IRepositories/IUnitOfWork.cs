namespace MobilOfis.Core.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IGenericRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

