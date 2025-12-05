using MobilOfis.Core.IRepositories;
using MobilOfis.Core.IServices;
using MobilOfis.Entity;

namespace MobilOfis.Service.NotificationService;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task SendNotificationAsync(Guid recipientId, string message, string? entityType, Guid? entityId)
    {
        var notification = new Notifications
        {
            NotificationId = Guid.NewGuid(),
            RecipientUserId = recipientId,
            Message = message,
            SendDate = DateTime.UtcNow,
            IsRead = false,
            RelatedEntityType = entityType,
            RelatedEntityId = entityId
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SendLeaveNotificationAsync(Leaves leave, string action)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(leave.UserId);
        if (user == null) return;

        string message = action switch
        {
            "created" => $"{user.FirstName} {user.LastName} yeni bir izin talebi oluşturdu.",
            "manager_approved" => $"İzin talebiniz yöneticiniz tarafından onaylandı. HR onayı bekleniyor.",
            "approved" => $"İzin talebiniz HR tarafından onaylandı.",
            "rejected" => $"İzin talebiniz reddedildi. Sebep: {leave.RejectionReason}",
            "cancelled" => "İzin talebiniz iptal edildi.",
            _ => "İzin talebinizle ilgili bir güncelleme var."
        };

        // Çalışana bildirim
        if (action != "created")
        {
            await SendNotificationAsync(leave.UserId, message, "Leave", leave.LeavesId);
        }

        // Manager'a bildirim
        if (action == "created" && user.ManagerId.HasValue)
        {
            await SendNotificationAsync(user.ManagerId.Value, message, "Leave", leave.LeavesId);
        }

        // HR'a bildirim (Manager onayladıktan sonra)
        if (action == "manager_approved")
        {
            // HR rolüne sahip tüm kullanıcılara bildirim gönder
            var hrUsers = await _unitOfWork.Users.FindAsync(u => u.Role == "HR" || u.Role == "Admin");
            foreach (var hrUser in hrUsers)
            {
                await SendNotificationAsync(hrUser.UserId, $"{user.FirstName} {user.LastName}'ın izin talebi manager onayından geçti. HR onayı bekliyor.", "Leave", leave.LeavesId);
            }
        }

        // Manager'a iptal bildirimi
        if (action == "cancelled" && user.ManagerId.HasValue)
        {
            await SendNotificationAsync(user.ManagerId.Value, $"{user.FirstName} {user.LastName} izin talebini iptal etti.", "Leave", leave.LeavesId);
        }
    }

    public async Task SendEventNotificationAsync(Events evt, List<Guid> participantIds, string action)
    {
        var creator = await _unitOfWork.Users.GetByIdAsync(evt.CreatedByUserId);
        if (creator == null) return;

        string message = action switch
        {
            "created" => $"{creator.FirstName} {creator.LastName} yeni bir etkinlik oluşturdu: {evt.Title}",
            "updated" => $"{evt.Title} etkinliği güncellendi.",
            "deleted" => $"{evt.Title} etkinliği iptal edildi.",
            _ => $"{evt.Title} etkinliğiyle ilgili bir güncelleme var."
        };

        foreach (var participantId in participantIds)
        {
            await SendNotificationAsync(participantId, message, "Event", evt.EventId);
        }
    }

    public async Task<IEnumerable<Notifications>> GetMyNotificationsAsync(Guid userId)
    {
        return await _unitOfWork.Notifications.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Notifications>> GetUnreadNotificationsAsync(Guid userId)
    {
        return await _unitOfWork.Notifications.GetUnreadByUserIdAsync(userId);
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
    }
}

