using MobilOfis.Entity;

namespace MobilOfis.Core.IServices;

public interface IDepartmentService
{
    Task<Departments> CreateDepartmentAsync(string departmentName);
    Task<Departments> UpdateDepartmentAsync(Guid departmentId, string departmentName, Guid? managerId = null);
    Task<bool> DeleteDepartmentAsync(Guid departmentId);
    Task<IEnumerable<Departments>> GetAllDepartmentsAsync();
    Task<Departments> GetDepartmentByIdAsync(Guid departmentId);
    Task<IEnumerable<User>> GetDepartmentEmployeesAsync(Guid departmentId);
    Task FixDepartmentDatesAsync();
}

