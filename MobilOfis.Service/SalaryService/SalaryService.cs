using MobilOfis.Core.IRepositories;
using MobilOfis.Core.IServices;
using MobilOfis.Entity;

namespace MobilOfis.Service.SalaryService;

public class SalaryService : ISalaryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public SalaryService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<bool> UpdateSalaryAsync(Guid userId, decimal newSalary, Guid requesterId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        if (newSalary <= 0)
        {
            throw new Exception("Maaş değeri sıfırdan büyük olmalıdır.");
        }

        var requester = await _unitOfWork.Users.GetByIdAsync(requesterId);
        if (requester == null)
        {
            throw new Exception("İstekte bulunan kullanıcı bulunamadı.");
        }

        var isHrOrAdmin = string.Equals(requester.Role, "HR", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(requester.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        var isManagerOfUser = string.Equals(requester.Role, "Manager", StringComparison.OrdinalIgnoreCase) &&
                              user.ManagerId == requesterId;

        if (!isHrOrAdmin && !isManagerOfUser)
        {
            throw new Exception("Bu işlemi gerçekleştirmek için yetkiniz yok.");
        }

        var oldSalary = user.Salary;
        user.Salary = newSalary;
        user.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        // Kullanıcıya bildirim gönder
        var message = oldSalary.HasValue 
            ? $"Maaşınız {oldSalary:C} tutarından {newSalary:C} tutarına güncellendi."
            : $"Maaşınız {newSalary:C} olarak belirlendi.";

        await _notificationService.SendNotificationAsync(userId, message, "Salary", userId);

        return true;
    }

    public async Task<User> GetUserSalaryInfoAsync(Guid userId, Guid requesterId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        var requester = await _unitOfWork.Users.GetByIdAsync(requesterId);
        if (requester == null)
        {
            throw new Exception("İstekte bulunan kullanıcı bulunamadı.");
        }

        var isSelf = userId == requesterId;
        var isHrOrAdmin = string.Equals(requester.Role, "HR", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(requester.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        var isManagerOfUser = string.Equals(requester.Role, "Manager", StringComparison.OrdinalIgnoreCase) &&
                              user.ManagerId == requesterId;

        // Yetki kontrolü: Sadece kendi maaşını veya HR/Admin ya da yöneticisi maaşları görebilir
        if (!isSelf && !isHrOrAdmin && !isManagerOfUser)
        {
            throw new Exception("Bu bilgilere erişim yetkiniz yok.");
        }

        return user;
    }

    public async Task<IEnumerable<User>> GetTeamMembersAsync(Guid managerId)
    {
        return await _unitOfWork.Users.GetSubordinatesAsync(managerId);
    }
}

