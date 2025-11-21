using MobilOfis.Entity;

namespace MobilOfis.Core.IServices;

public interface INotificationService
{
    Task SendNotificationAsync(Guid recipientId, string message, string? entityType, Guid? entityId);
    Task SendLeaveNotificationAsync(Leaves leave, string action);
    Task SendEventNotificationAsync(Events evt, List<Guid> participantIds, string action);
    Task<IEnumerable<Notifications>> GetMyNotificationsAsync(Guid userId);
    Task<IEnumerable<Notifications>> GetUnreadNotificationsAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
}

