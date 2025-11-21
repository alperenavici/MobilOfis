using MobilOfis.Entity;

namespace MobilOfis.Core.IRepositories;

public interface ILeaveRepository : IGenericRepository<Leaves>
{
    Task<IEnumerable<Leaves>> GetPendingLeavesByManagerIdAsync(Guid managerId);
    Task<IEnumerable<Leaves>> GetPendingLeavesForHRAsync();
    Task<IEnumerable<Leaves>> GetLeavesByUserIdAsync(Guid userId);
    Task<IEnumerable<Leaves>> GetLeavesByDepartmentIdAsync(Guid departmentId);
    Task<IEnumerable<Leaves>> GetLeavesByDateRangeAsync(DateTime start, DateTime end);
}

