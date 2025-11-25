using Microsoft.EntityFrameworkCore;
using MobilOfis.Core.IRepositories;
using MobilOfis.Data.Context;
using MobilOfis.Entity;
using MobilOfis.Entity.Enums;

namespace MobilOfis.Data.Repositories;

public class LeaveRepository : GenericRepository<Leaves>, ILeaveRepository
{
    public LeaveRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Leaves>> GetPendingLeavesByManagerIdAsync(Guid managerId)
    {
        // Manager'ın departmanındaki bekleyen izinleri getir
        return await _dbContext.Leaves
            .Include(l => l.User)
            .ThenInclude(u => u.Department)
            .Where(l => l.User.ManagerId == managerId && l.Status == Status.Pending)
            .OrderBy(l => l.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Leaves>> GetPendingLeavesForHRAsync()
    {
        // Manager onayından geçmiş, HR onayı bekleyen izinleri getir
        return await _dbContext.Leaves
            .Include(l => l.User)
            .Include(l => l.ManagerApproval)
            .Where(l => l.Status == Status.ManagerApproved)
            .OrderBy(l => l.ManagerApprovalDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Leaves>> GetLeavesByUserIdAsync(Guid userId)
    {
        return await _dbContext.Leaves
            .Include(l => l.User)
            .Include(l => l.ManagerApproval)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.RequestDate)
            .ToListAsync();
    }

    public override async Task<Leaves?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Leaves
            .Include(l => l.User)
            .Include(l => l.ManagerApproval)
            .FirstOrDefaultAsync(l => l.LeavesId == id);
    }

    public async Task<IEnumerable<Leaves>> GetLeavesByDepartmentIdAsync(Guid departmentId)
    {
        return await _dbContext.Leaves
            .Include(l => l.User)
            .Where(l => l.User.DepartmentId == departmentId)
            .OrderByDescending(l => l.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Leaves>> GetLeavesByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _dbContext.Leaves
            .Include(l => l.User)
            .Where(l => l.StartDate <= end && l.EndDate >= start)
            .OrderBy(l => l.StartDate)
            .ToListAsync();
    }
}

