using Microsoft.EntityFrameworkCore.Storage;
using MobilOfis.Core.IRepositories;
using MobilOfis.Data.Context;
using MobilOfis.Data.Repositories;

namespace MobilOfis.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private IUserRepository? _userRepository;
    private ILeaveRepository? _leaveRepository;
    private IEventRepository? _eventRepository;
    private IDepartmentRepository? _departmentRepository;
    private INotificationRepository? _notificationRepository;
    private readonly Dictionary<Type, object> _repositories;
    private IPostRepository? _postRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }

    public IUserRepository Users => _userRepository ??= new UserRepository(_context);
    public ILeaveRepository Leaves => _leaveRepository ??= new LeaveRepository(_context);
    public IEventRepository Events => _eventRepository ??= new EventRepository(_context);
    public IDepartmentRepository Departments => _departmentRepository ??= new DepartmentRepository(_context);
    public INotificationRepository Notifications => _notificationRepository ??= new NotificationRepository(_context);
    public IPostRepository Posts => _postRepository ??= new PostRepository(_context);

    public IGenericRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new GenericRepository<T>(_context);
        }

        return (IGenericRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

