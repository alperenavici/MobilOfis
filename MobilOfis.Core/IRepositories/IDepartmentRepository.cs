using MobilOfis.Entity;

namespace MobilOfis.Core.IRepositories;

public interface IDepartmentRepository : IGenericRepository<Departments>
{
    Task<Departments?> GetByNameAsync(string name);
    Task<IEnumerable<User>> GetDepartmentUsersAsync(Guid departmentId);
    Task<IEnumerable<Departments>> GetAllWithDetailsAsync();
    Task<Departments?> GetByIdWithDetailsAsync(Guid id);
}

