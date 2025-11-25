using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.DTOs;
using MobilOfis.Web.Models.ViewModels;

namespace MobilOfis.Web.Controllers;

public class DepartmentController : Controller
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    #region MVC Actions
    
    [Authorize(Policy = "HROnly")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            var viewModel = departments.Select(d => new DepartmentViewModel
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                EmployeeCount = d.Users?.Count ?? 0
            }).ToList();
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [Authorize(Policy = "HROnly")]
    [HttpGet]
    public async Task<IActionResult> Detail(Guid id)
    {
        try
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            var viewModel = new DepartmentViewModel
            {
                DepartmentId = department.DepartmentId,
                DepartmentName = department.DepartmentName,
                EmployeeCount = department.Users?.Count ?? 0
            };
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Policy = "HROnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string departmentName)
    {
        try
        {
            await _departmentService.CreateDepartmentAsync(departmentName);
            TempData["SuccessMessage"] = "Departman başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Policy = "HROnly")]
    [HttpPost("Delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _departmentService.DeleteDepartmentAsync(id);
            return Json(new { success = true, message = "Departman başarıyla silindi." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region API Actions
    
    [HttpPost]
    [Route("api/[controller]")]
    public async Task<IActionResult> CreateDepartmentApi([FromBody] CreateDepartmentDto dto)
    {
        try
        {
            var department = await _departmentService.CreateDepartmentAsync(dto.DepartmentName);
            return Ok(new { message = "Departman oluşturuldu.", departmentId = department.DepartmentId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "HROnly")]
    [HttpGet]
    [Route("api/[controller]")]
    public async Task<IActionResult> GetAllDepartmentsApi()
    {
        try
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "HROnly")]
    [HttpGet]
    [Route("api/[controller]/{id}")]
    public async Task<IActionResult> GetDepartmentApi(Guid id)
    {
        try
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            return Ok(new
            {
                departmentId = department.DepartmentId,
                departmentName = department.DepartmentName,
                employeeCount = department.Users?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "HROnly")]
    [HttpPut]
    [Route("api/[controller]/{id}")]
    public async Task<IActionResult> UpdateDepartmentApi(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        try
        {
            var department = await _departmentService.UpdateDepartmentAsync(id, dto.DepartmentName);
            return Ok(new { message = "Departman başarıyla güncellendi.", departmentId = department.DepartmentId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "HROnly")]
    [HttpDelete]
    [Route("api/[controller]/{id}")]
    public async Task<IActionResult> DeleteDepartmentApi(Guid id)
    {
        try
        {
            await _departmentService.DeleteDepartmentAsync(id);
            return Ok(new { message = "Departman başarıyla silindi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion
}
