using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.ViewModels;
using MobilOfis.Web.Models.DTOs;

namespace MobilOfis.Web.Controllers;

[Authorize(Policy = "HROnly")]
public class UserController : Controller
{
    private readonly IAuthServices _authServices;
    private readonly IDepartmentService _departmentService;

    public UserController(IAuthServices authServices, IDepartmentService departmentService)
    {
        _authServices = authServices;
        _departmentService = departmentService;
    }

    /// <summary>
    /// Kullanıcılar listesi
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm = null, Guid? departmentId = null, string? role = null, bool? isActive = null)
    {
        try
        {
            var users = await _authServices.GetAllUsersAsync();

            // Filtreleme
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                users = users.Where(u => 
                    (u.FirstName?.ToLower().Contains(searchTerm) ?? false) || 
                    (u.LastName?.ToLower().Contains(searchTerm) ?? false) || 
                    (u.Email?.ToLower().Contains(searchTerm) ?? false));
            }

            if (departmentId.HasValue)
            {
                users = users.Where(u => u.DepartmentId == departmentId);
            }

            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => u.Role == role);
            }

            if (isActive.HasValue)
            {
                users = users.Where(u => u.IsActive == isActive.Value);
            }

            var viewModel = new UserListViewModel
            {
                Users = users.Select(u => new UserViewModel
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    ProfilePictureUrl = u.ProfilePictureUrl,
                    JobTitle = u.JobTitle,
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department?.DepartmentName,
                    ManagerId = u.ManagerId,
                    ManagerName = u.Manager != null ? $"{u.Manager.FirstName} {u.Manager.LastName}" : null,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    HireDate = u.HireDate
                }).ToList(),
                Filters = new UserFilterViewModel
                {
                    SearchTerm = searchTerm,
                    DepartmentId = departmentId,
                    Role = role,
                    IsActive = isActive
                }
            };
            
            await LoadDepartmentsToViewBag();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }

    /// <summary>
    /// Kullanıcı detay sayfası
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Detail(Guid id)
    {
        try
        {
            var user = await _authServices.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UserViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                JobTitle = user.JobTitle,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.DepartmentName,
                ManagerId = user.ManagerId,
                ManagerName = user.Manager != null ? $"{user.Manager.FirstName} {user.Manager.LastName}" : null,
                Role = user.Role,
                IsActive = user.IsActive,
                HireDate = user.HireDate,
                Salary = user.Salary,
                Address = user.Address,
                City = user.City,
                Country = user.Country,
                PostalCode = user.PostalCode,
                DateOfBirth = user.DateOfBirth,
                EmergencyContactName = user.EmergencyContactName,
                EmergencyContactPhone = user.EmergencyContactPhone,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Yeni kullanıcı oluşturma sayfası
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDepartmentsToViewBag();
        await LoadManagersToViewBag();
        return View(new UserViewModel());
    }

    /// <summary>
    /// Yeni kullanıcı oluştur
    /// </summary>
    /// <summary>
    /// Yeni kullanıcı oluştur
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserViewModel model, string password)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartmentsToViewBag();
            await LoadManagersToViewBag();
            return View(model);
        }

        try
        {
            var user = await _authServices.RegisterAsync(model.FirstName, model.LastName, model.Email, password, model.PhoneNumber);
            
            user.JobTitle = model.JobTitle;
            user.DepartmentId = model.DepartmentId;
            user.ManagerId = model.ManagerId;
            user.Role = model.Role;
            user.IsActive = model.IsActive;
            user.HireDate = model.HireDate;
            user.Salary = model.Salary;
            user.Address = model.Address;
            user.City = model.City;
            user.Country = model.Country;
            user.PostalCode = model.PostalCode;
            user.DateOfBirth = model.DateOfBirth;
            user.EmergencyContactName = model.EmergencyContactName;
            user.EmergencyContactPhone = model.EmergencyContactPhone;
            
            await _authServices.UpdateUserAsync(user);

            TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadDepartmentsToViewBag();
            await LoadManagersToViewBag();
            return View(model);
        }
    }

    /// <summary>
    /// Kullanıcı düzenleme sayfası
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var user = await _authServices.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UserViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                JobTitle = user.JobTitle,
                DepartmentId = user.DepartmentId,
                ManagerId = user.ManagerId,
                Role = user.Role,
                IsActive = user.IsActive,
                HireDate = user.HireDate,
                Salary = user.Salary,
                Address = user.Address,
                City = user.City,
                Country = user.Country,
                PostalCode = user.PostalCode,
                DateOfBirth = user.DateOfBirth,
                EmergencyContactName = user.EmergencyContactName,
                EmergencyContactPhone = user.EmergencyContactPhone
            };

            await LoadDepartmentsToViewBag();
            await LoadManagersToViewBag();
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Kullanıcı güncelle
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartmentsToViewBag();
            await LoadManagersToViewBag();
            return View(model);
        }

        try
        {
            var user = await _authServices.GetUserByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.JobTitle = model.JobTitle;
            user.DepartmentId = model.DepartmentId;
            user.ManagerId = model.ManagerId;
            user.Role = model.Role;
            user.IsActive = model.IsActive;
            user.HireDate = model.HireDate;
            user.Salary = model.Salary;
            user.Address = model.Address;
            user.City = model.City;
            user.Country = model.Country;
            user.PostalCode = model.PostalCode;
            user.DateOfBirth = model.DateOfBirth;
            user.EmergencyContactName = model.EmergencyContactName;
            user.EmergencyContactPhone = model.EmergencyContactPhone;
            
            await _authServices.UpdateUserAsync(user);

            TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
            return RedirectToAction(nameof(Detail), new { id = model.UserId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadDepartmentsToViewBag();
            await LoadManagersToViewBag();
            return View(model);
        }
    }

    [HttpPost("Deactivate/{id}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        try
        {
            await _authServices.UpdateUserStatusAsync(id, false);
            return Json(new { success = true, message = "Kullanıcı pasif yapıldı." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("Activate/{id}")]
    public async Task<IActionResult> Activate(Guid id)
    {
        try
        {
            await _authServices.UpdateUserStatusAsync(id, true);
            return Json(new { success = true, message = "Kullanıcı aktif yapıldı." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #region API Actions

    [HttpGet("api/[controller]")]
    public async Task<IActionResult> GetAllUsersApi(string? searchTerm = null, Guid? departmentId = null, string? role = null, bool? isActive = null)
    {
        try
        {
            // Tüm kullanıcıları getir (service'de implement edilmeli)
            // Şimdilik boş liste döndürüyoruz
            return Ok(new List<object>());
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("api/[controller]/{id}")]
    public async Task<IActionResult> GetUserApi(Guid id)
    {
        try
        {
            var user = await _authServices.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Kullanıcı bulunamadı." });
            }

            return Ok(new
            {
                userId = user.UserId,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                profilePictureUrl = user.ProfilePictureUrl,
                jobTitle = user.JobTitle,
                departmentId = user.DepartmentId,
                departmentName = user.Department?.DepartmentName,
                managerId = user.ManagerId,
                managerName = user.Manager != null ? $"{user.Manager.FirstName} {user.Manager.LastName}" : null,
                role = user.Role,
                isActive = user.IsActive,
                hireDate = user.HireDate,
                salary = user.Salary,
                address = user.Address,
                city = user.City,
                country = user.Country,
                postalCode = user.PostalCode,
                dateOfBirth = user.DateOfBirth,
                emergencyContactName = user.EmergencyContactName,
                emergencyContactPhone = user.EmergencyContactPhone,
                createdDate = user.CreatedDate,
                lastLoginDate = user.LastLoginDate
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("api/[controller]")]
    public async Task<IActionResult> CreateUserApi([FromBody] RegisterDto dto)
    {
        try
        {
            var user = await _authServices.RegisterAsync(
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Password,
                dto.PhoneNumber
            );

            return Ok(new
            {
                message = "Kullanıcı başarıyla oluşturuldu.",
                userId = user.UserId,
                email = user.Email
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("api/[controller]/{id}")]
    public async Task<IActionResult> UpdateUserApi(Guid id, [FromBody] UpdateProfileDto dto)
    {
        try
        {
            var user = await _authServices.UpdateUserProfileAsync(
                id,
                dto.FirstName,
                dto.LastName,
                dto.PhoneNumber ?? string.Empty,
                dto.ProfilePictureUrl ?? string.Empty
            );

            return Ok(new
            {
                message = "Kullanıcı başarıyla güncellendi.",
                userId = user.UserId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    private async Task LoadDepartmentsToViewBag()
    {
        try
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            ViewBag.Departments = departments;
        }
        catch
        {
            ViewBag.Departments = new List<Entity.Departments>();
        }
    }

    private async Task LoadManagersToViewBag()
    {
        try
        {
            var users = await _authServices.GetAllUsersAsync();
            ViewBag.Managers = users.Select(u => new UserViewModel
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName
            }).ToList();
        }
        catch
        {
            ViewBag.Managers = new List<UserViewModel>();
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Kullanıcı bilgisi bulunamadı.");
        }
        return userId;
    }
}

