using MobilOfis.Entity;

namespace MobilOfis.Core.IServices;

public interface ISalaryService
{
    Task<bool> UpdateSalaryAsync(Guid userId, decimal newSalary, Guid hrUserId);
    Task<User> GetUserSalaryInfoAsync(Guid userId, Guid requesterId);
}

