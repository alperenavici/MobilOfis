using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.DTOs;
using MobilOfis.Web.Models.ViewModels;

namespace MobilOfis.Web.Controllers;

public class DepartmentController : Controller
{
    private readonly IDepartmentService _departmentService;
    private readonly IAuthServices _authServices;

    public DepartmentController(IDepartmentService departmentService, IAuthServices authServices)
    {
        _departmentService = departmentService;
        _authServices = authServices;
    }

    
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
                DepartmentName = d.DepartmentName ?? string.Empty,
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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Detail(Guid id)
    {
        try
        {
            var isHrOrAdmin = User.IsInRole("HR") || User.IsInRole("Admin");
            var userDepartmentIdClaim = User.FindFirst("departmentId")?.Value;
            var userDepartmentId = !string.IsNullOrEmpty(userDepartmentIdClaim) ? Guid.Parse(userDepartmentIdClaim) : (Guid?)null;

            if (!isHrOrAdmin && userDepartmentId != id)
            {
                TempData["ErrorMessage"] = "Bu departmanı görüntüleme yetkiniz yok.";
                return RedirectToAction("Index", "Dashboard");
            }

            var department = await _departmentService.GetDepartmentByIdAsync(id);
            var viewModel = new DepartmentViewModel
            {
                DepartmentId = department.DepartmentId,
                DepartmentName = department.DepartmentName ?? string.Empty,
                ManagerId = department.ManagerId,
                ManagerName = department.Manager != null ? $"{department.Manager.FirstName} {department.Manager.LastName}" : "Atanmamış",
                EmployeeCount = department.Users?.Count ?? 0,
                CreatedAt = department.CreatedDate,
                Description = "Şirketimizin değerli bir departmanı.", // Örnek veri
                Employees = department.Users?.Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    JobTitle = u.JobTitle,
                    Role = u.Role,
                    ProfilePictureUrl = u.ProfilePictureUrl
                }).ToList() ?? new List<UserViewModel>()
            };
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> MyDepartment()
    {
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
             return RedirectToAction("Login", "Auth");
        }

        var user = await _authServices.GetUserByIdAsync(userId);
        
        if (user == null || user.DepartmentId == null)
        {
            TempData["ErrorMessage"] = "Herhangi bir departmana atanmamışsınız.";
            return RedirectToAction("Index", "Dashboard");
        }

        return RedirectToAction(nameof(Detail), new { id = user.DepartmentId });
    }

    [Authorize(Policy = "HROnly")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            var users = await _authServices.GetAllUsersAsync();
            ViewBag.PotentialManagers = users;

            var viewModel = new DepartmentViewModel
            {
                DepartmentId = department.DepartmentId,
                DepartmentName = department.DepartmentName ?? string.Empty,
                ManagerId = department.ManagerId,
                ManagerName = department.Manager != null ? $"{department.Manager.FirstName} {department.Manager.LastName}" : null,
                EmployeeCount = department.Users?.Count ?? 0,
                Employees = department.Users?.Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    JobTitle = u.JobTitle,
                    Role = u.Role,
                    ProfilePictureUrl = u.ProfilePictureUrl
                }).ToList() ?? new List<UserViewModel>()
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
    public async Task<IActionResult> Edit(DepartmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _departmentService.UpdateDepartmentAsync(model.DepartmentId, model.DepartmentName, model.ManagerId);
            TempData["SuccessMessage"] = "Departman başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
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
            var message = ex.InnerException != null ? $"{ex.Message} {ex.InnerException.Message}" : ex.Message;
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Policy = "HROnly")]
    [HttpPost("Department/Delete/{id}")]
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

    [Authorize(Policy = "HROnly")]
    [HttpGet]
    public async Task<IActionResult> FixDates()
    {
        try
        {
            await _departmentService.FixDepartmentDatesAsync();
            return Content("Dates fixed successfully. You can go back to the list.");
        }
        catch (Exception ex)
        {
            return Content($"Error fixing dates: {ex.Message}");
        }
    }

}
