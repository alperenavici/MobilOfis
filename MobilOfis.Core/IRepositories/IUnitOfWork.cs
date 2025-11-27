namespace MobilOfis.Core.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ILeaveRepository Leaves { get; }
    IEventRepository Events { get; }
    IDepartmentRepository Departments { get; }
    INotificationRepository Notifications { get; }
    IPostRepository Posts { get; }
    IGenericRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

