using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.DTOs;

namespace MobilOfis.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    /// <summary>
    /// Departman oluştur
    /// </summary>
    // [Authorize(Policy = "HROnly")]
    [HttpPost]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto dto)
    {
        try
        {
            var department = await _departmentService.CreateDepartmentAsync(dto.DepartmentName);

            return Ok(new
            {
                message = "Departman oluşturuldu.",
                departmentId = department.DepartmentId,
                departmentName = department.DepartmentName
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Departman güncelle
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        try
        {
            var department = await _departmentService.UpdateDepartmentAsync(id, dto.DepartmentName);

            return Ok(new
            {
                message = "Departman güncellendi.",
                departmentId = department.DepartmentId,
                departmentName = department.DepartmentName
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Departman sil
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        try
        {
            await _departmentService.DeleteDepartmentAsync(id);

            return Ok(new { message = "Departman silindi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Tüm departmanları listele
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllDepartments()
    {
        try
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();

            var response = departments.Select(d => new DepartmentResponseDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName ?? "",
                EmployeeCount = d.Users?.Count ?? 0
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Departman detayı
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDepartmentById(Guid id)
    {
        try
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);

            var response = new DepartmentResponseDto
            {
                DepartmentId = department.DepartmentId,
                DepartmentName = department.DepartmentName ?? "",
                EmployeeCount = department.Users?.Count ?? 0
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Departman çalışanları
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpGet("{id}/employees")]
    public async Task<IActionResult> GetDepartmentEmployees(Guid id)
    {
        try
        {
            var employees = await _departmentService.GetDepartmentEmployeesAsync(id);

            var response = employees.Select(e => new
            {
                userId = e.UserId,
                firstName = e.FirstName,
                lastName = e.LastName,
                email = e.Email,
                jobTitle = e.JobTitle,
                phoneNumber = e.PhoneNumber,
                isActive = e.IsActive
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

