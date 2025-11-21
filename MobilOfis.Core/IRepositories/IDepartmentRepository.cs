using MobilOfis.Entity;

namespace MobilOfis.Core.IRepositories;

public interface IDepartmentRepository : IGenericRepository<Departments>
{
    Task<Departments?> GetByNameAsync(string name);
    Task<IEnumerable<User>> GetDepartmentUsersAsync(Guid departmentId);
}

