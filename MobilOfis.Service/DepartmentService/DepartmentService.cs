using MobilOfis.Core.IRepositories;
using MobilOfis.Core.IServices;
using MobilOfis.Entity;

namespace MobilOfis.Service.DepartmentService;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Departments> CreateDepartmentAsync(string departmentName)
    {
        var existing = await _unitOfWork.Departments.GetByNameAsync(departmentName);
        if (existing != null)
        {
            throw new Exception("Bu isimde bir departman zaten mevcut.");
        }

        var department = new Departments
        {
            DepartmentId = Guid.NewGuid(),
            DepartmentName = departmentName
        };

        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        return department;
    }

    public async Task<Departments> UpdateDepartmentAsync(Guid departmentId, string departmentName)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
        if (department == null)
        {
            throw new Exception("Departman bulunamadı.");
        }

        var existing = await _unitOfWork.Departments.GetByNameAsync(departmentName);
        if (existing != null && existing.DepartmentId != departmentId)
        {
            throw new Exception("Bu isimde bir departman zaten mevcut.");
        }

        department.DepartmentName = departmentName;

        _unitOfWork.Departments.Update(department);
        await _unitOfWork.SaveChangesAsync();

        return department;
    }

    public async Task<bool> DeleteDepartmentAsync(Guid departmentId)
    {
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
        if (department == null)
        {
            throw new Exception("Departman bulunamadı.");
        }

        // Departmanda çalışan var mı kontrol et
        var employees = await _unitOfWork.Departments.GetDepartmentUsersAsync(departmentId);
        if (employees.Any())
        {
            throw new Exception("Bu departmanda çalışanlar bulunmaktadır. Önce çalışanları başka departmana taşıyın.");
        }

        _unitOfWork.Departments.Remove(department);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Departments>> GetAllDepartmentsAsync()
    {
        return await _unitOfWork.Departments.GetAllWithDetailsAsync();
    }

    public async Task<Departments> GetDepartmentByIdAsync(Guid departmentId)
    {
        var department = await _unitOfWork.Departments.GetByIdWithDetailsAsync(departmentId);
        if (department == null)
        {
            throw new Exception("Departman bulunamadı.");
        }
        return department;
    }

    public async Task<IEnumerable<User>> GetDepartmentEmployeesAsync(Guid departmentId)
    {
        return await _unitOfWork.Departments.GetDepartmentUsersAsync(departmentId);
    }
}

