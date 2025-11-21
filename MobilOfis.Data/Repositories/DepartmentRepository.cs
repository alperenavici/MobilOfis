using Microsoft.EntityFrameworkCore;
using MobilOfis.Core.IRepositories;
using MobilOfis.Data.Context;
using MobilOfis.Entity;

namespace MobilOfis.Data.Repositories;

public class DepartmentRepository : GenericRepository<Departments>, IDepartmentRepository
{
    public DepartmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Departments?> GetByNameAsync(string name)
    {
        return await _dbContext.Departments
            .FirstOrDefaultAsync(d => d.DepartmentName == name);
    }

    public async Task<IEnumerable<User>> GetDepartmentUsersAsync(Guid departmentId)
    {
        return await _dbContext.Users
            .Where(u => u.DepartmentId == departmentId && u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();
    }
}

