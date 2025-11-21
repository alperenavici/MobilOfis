using MobilOfis.Entity;

namespace MobilOfis.Core.IRepositories;

public interface INotificationRepository : IGenericRepository<Notifications>
{
    Task<IEnumerable<Notifications>> GetUnreadByUserIdAsync(Guid userId);
    Task<IEnumerable<Notifications>> GetByUserIdAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
}

