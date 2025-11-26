using MobilOfis.Core.IRepositories;
using MobilOfis.Core.IServices;
using MobilOfis.Entity;
using MobilOfis.Entity.Enums;

namespace MobilOfis.Service.LeaveService;

public class LeaveService : ILeaveService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public LeaveService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Leaves> CreateLeaveRequestAsync(Guid userId, DateTime startDate, DateTime endDate, string leavesType, string? reason)
    {
        Console.WriteLine($"LeaveService.CreateLeaveRequestAsync started. UserId={userId}, Type={leavesType}");
        
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            Console.WriteLine("User not found.");
            throw new Exception("Kullanıcı bulunamadı.");
        }
        Console.WriteLine($"User found: {user.FirstName} {user.LastName}, ManagerId={user.ManagerId}");

        if (!user.ManagerId.HasValue)
        {
            Console.WriteLine("ManagerId is null.");
            throw new Exception("Kullanıcının yöneticisi tanımlı değil. İzin talebi oluşturulamaz.");
        }

        if (startDate >= endDate)
        {
            throw new Exception("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }

        if (!Enum.TryParse<LeavesType>(leavesType, out var leaveTypeEnum))
        {
            Console.WriteLine($"Invalid leave type: {leavesType}");
            throw new Exception("Geçersiz izin türü.");
        }

        var leave = new Leaves
        {
            LeavesId = Guid.NewGuid(),
            UserId = userId,
            StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc),
            RequestDate = DateTime.UtcNow,
            Status = Status.Pending,
            LeavesType = leaveTypeEnum,
            Reason = reason
        };
        Console.WriteLine("Leave entity created.");

        await _unitOfWork.Leaves.AddAsync(leave);
        Console.WriteLine("Leave added to context.");
        await _unitOfWork.SaveChangesAsync();
        Console.WriteLine("Changes saved.");

        // Manager'a bildirim gönder
        try
        {
            Console.WriteLine("Sending notification...");
            await _notificationService.SendLeaveNotificationAsync(leave, "created");
            Console.WriteLine("Notification sent.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending notification: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }

        return leave;
    }

    public async Task<Leaves> ApproveLeaveByManagerAsync(Guid leaveId, Guid managerId)
    {
        var leave = await _unitOfWork.Leaves.GetByIdAsync(leaveId);
        if (leave == null)
        {
            throw new Exception("İzin talebi bulunamadı.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(leave.UserId);
        if (user == null || user.ManagerId != managerId)
        {
            throw new Exception("Bu izin talebini onaylama yetkiniz yok.");
        }

        if (leave.Status != Status.Pending)
        {
            throw new Exception("Bu izin talebi zaten işleme alınmış.");
        }

        leave.Status = Status.ManagerApproved;
        leave.ManagerApprovalId = managerId;
        leave.ManagerApprovalDate = DateTime.UtcNow;

        _unitOfWork.Leaves.Update(leave);
        await _unitOfWork.SaveChangesAsync();

        // Çalışana ve HR'a bildirim gönder
        await _notificationService.SendLeaveNotificationAsync(leave, "manager_approved");

        return leave;
    }

    public async Task<Leaves> ApproveLeaveByHRAsync(Guid leaveId, Guid hrUserId)
    {
        var leave = await _unitOfWork.Leaves.GetByIdAsync(leaveId);
        if (leave == null)
        {
            throw new Exception("İzin talebi bulunamadı.");
        }

        if (leave.Status != Status.ManagerApproved)
        {
            throw new Exception("Bu izin talebi henüz manager onayından geçmemiş.");
        }

        leave.Status = Status.Approved;
        leave.HRApprovalId = hrUserId;
        leave.HRApprovalDate = DateTime.UtcNow;

        _unitOfWork.Leaves.Update(leave);
        await _unitOfWork.SaveChangesAsync();

        // Çalışana bildirim gönder
        await _notificationService.SendLeaveNotificationAsync(leave, "approved");

        return leave;
    }

    public async Task<Leaves> RejectLeaveAsync(Guid leaveId, Guid approverId, string reason)
    {
        var leave = await _unitOfWork.Leaves.GetByIdAsync(leaveId);
        if (leave == null)
        {
            throw new Exception("İzin talebi bulunamadı.");
        }

        if (leave.Status == Status.Approved || leave.Status == Status.Rejected)
        {
            throw new Exception("Bu izin talebi zaten sonuçlandırılmış.");
        }

        leave.Status = Status.Rejected;
        leave.RejectionReason = reason;
        
        if (leave.Status == Status.Pending)
        {
            leave.ManagerApprovalId = approverId;
            leave.ManagerApprovalDate = DateTime.UtcNow;
        }
        else if (leave.Status == Status.ManagerApproved)
        {
            leave.HRApprovalId = approverId;
            leave.HRApprovalDate = DateTime.UtcNow;
        }

        _unitOfWork.Leaves.Update(leave);
        await _unitOfWork.SaveChangesAsync();

        // Çalışana bildirim gönder
        await _notificationService.SendLeaveNotificationAsync(leave, "rejected");

        return leave;
    }

    public async Task<IEnumerable<Leaves>> GetMyLeavesAsync(Guid userId)
    {
        return await _unitOfWork.Leaves.GetLeavesByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Leaves>> GetPendingLeavesForManagerAsync(Guid managerId)
    {
        return await _unitOfWork.Leaves.GetPendingLeavesByManagerIdAsync(managerId);
    }

    public async Task<IEnumerable<Leaves>> GetPendingLeavesForHRAsync()
    {
        return await _unitOfWork.Leaves.GetPendingLeavesForHRAsync();
    }

    public async Task<Leaves> GetLeaveByIdAsync(Guid leaveId)
    {
        var leave = await _unitOfWork.Leaves.GetByIdAsync(leaveId);
        if (leave == null)
        {
            throw new Exception("İzin talebi bulunamadı.");
        }
        return leave;
    }

    public async Task<bool> CanUserApproveLeaveAsync(Guid leaveId, Guid userId)
    {
        var leave = await _unitOfWork.Leaves.GetByIdAsync(leaveId);
        if (leave == null) return false;

        var user = await _unitOfWork.Users.GetByIdAsync(leave.UserId);
        if (user == null) return false;

        // Manager kontrolü
        if (user.ManagerId == userId && leave.Status == Status.Pending)
        {
            return true;
        }

        // HR kontrolü (role bazlı - controller'da kontrol edilecek)
        if (leave.Status == Status.ManagerApproved)
        {
            return true;
        }

        return false;
    }
}

