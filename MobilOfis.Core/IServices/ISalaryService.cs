using MobilOfis.Entity;

namespace MobilOfis.Core.IServices;

public interface ISalaryService
{
    Task<bool> UpdateSalaryAsync(Guid userId, decimal newSalary, Guid requesterId);
    Task<User> GetUserSalaryInfoAsync(Guid userId, Guid requesterId);
    Task<IEnumerable<User>> GetTeamMembersAsync(Guid managerId);
}

