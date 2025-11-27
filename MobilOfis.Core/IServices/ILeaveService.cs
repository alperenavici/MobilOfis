using MobilOfis.Entity;

namespace MobilOfis.Core.IServices;

public interface ILeaveService
{
    Task<Leaves> CreateLeaveRequestAsync(Guid userId, DateTime startDate, DateTime endDate, string leavesType, string? reason);
    Task<Leaves> ApproveLeaveByManagerAsync(Guid leaveId, Guid managerId);
    Task<Leaves> ApproveLeaveByHRAsync(Guid leaveId, Guid hrUserId);
    Task<Leaves> RejectLeaveAsync(Guid leaveId, Guid approverId, string reason);
    Task<IEnumerable<Leaves>> GetMyLeavesAsync(Guid userId);
    Task<IEnumerable<Leaves>> GetPendingLeavesForManagerAsync(Guid managerId);
    Task<IEnumerable<Leaves>> GetPendingLeavesForHRAsync();
    Task<Leaves> GetLeaveByIdAsync(Guid leaveId);
    Task<bool> CanUserApproveLeaveAsync(Guid leaveId, Guid userId);
    Task CancelLeaveAsync(Guid leaveId, Guid userId);
}

