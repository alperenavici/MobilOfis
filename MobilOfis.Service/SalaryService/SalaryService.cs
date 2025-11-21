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

    public async Task<bool> UpdateSalaryAsync(Guid userId, decimal newSalary, Guid hrUserId)
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

        // Yetki kontrolü: Sadece kendi maaşını veya HR/Admin maaşları görebilir
        if (userId != requesterId && requester.Role != "HR" && requester.Role != "Admin")
        {
            throw new Exception("Bu bilgilere erişim yetkiniz yok.");
        }

        return user;
    }
}

