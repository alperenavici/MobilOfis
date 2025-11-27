using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.ViewModels;
using MobilOfis.Web.Models.DTOs;

namespace MobilOfis.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IAuthServices _authServices;
    private readonly IFileStorageService _fileStorageService;

    public ProfileController(IAuthServices authServices, IFileStorageService fileStorageService)
    {
        _authServices = authServices;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Profil sayfası
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _authServices.GetUserByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var viewModel = new ProfileViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                JobTitle = user.JobTitle,
                DepartmentName = user.Department?.DepartmentName,
                ManagerName = user.Manager != null ? $"{user.Manager.FirstName} {user.Manager.LastName}" : null,
                Role = user.Role,
                HireDate = user.HireDate,
                LastLoginDate = user.LastLoginDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }

    /// <summary>
    /// Profil güncelle
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        try
        {
            var userId = GetCurrentUserId();
            var profilePictureUrl = model.ProfilePictureUrl;

            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                using var stream = model.ProfilePicture.OpenReadStream();
                var uploadedUrl = await _fileStorageService.SaveProfileImageAsync(
                    stream, 
                    model.ProfilePicture.FileName, 
                    model.ProfilePicture.Length);
                
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    profilePictureUrl = uploadedUrl;
                }
            }

            await _authServices.UpdateUserProfileAsync(
                userId,
                model.FirstName,
                model.LastName,
                model.PhoneNumber ?? string.Empty,
                profilePictureUrl ?? string.Empty
            );

            TempData["SuccessMessage"] = "Profil başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Şifre değiştir
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ProfileViewModel model)
    {
        try
        {
            if (string.IsNullOrEmpty(model.CurrentPassword) || string.IsNullOrEmpty(model.NewPassword))
            {
                TempData["ErrorMessage"] = "Mevcut şifre ve yeni şifre gereklidir.";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetCurrentUserId();
            await _authServices.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

            TempData["SuccessMessage"] = "Şifre başarıyla değiştirildi.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Bildirim ayarlarını güncelle
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateNotificationSettings(ProfileViewModel model)
    {
        try
        {
            // Bildirim ayarları service'de implement edilmeli
            TempData["SuccessMessage"] = "Bildirim ayarları güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhoto(IFormFile photo)
    {
        try
        {
            if (photo == null || photo.Length == 0)
            {
                return BadRequest(new { message = "Dosya seçilmedi." });
            }

            var userId = GetCurrentUserId();
            using var stream = photo.OpenReadStream();
            var uploadedUrl = await _fileStorageService.SaveProfileImageAsync(
                stream, 
                photo.FileName, 
                photo.Length);

            if (string.IsNullOrEmpty(uploadedUrl))
            {
                return BadRequest(new { message = "Dosya yüklenemedi." });
            }

            // Update user profile picture in DB
            var user = await _authServices.GetUserByIdAsync(userId);
            if (user != null)
            {
                await _authServices.UpdateUserProfileAsync(
                    userId,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber ?? string.Empty,
                    uploadedUrl
                );
            }

            return Ok(new { photoUrl = uploadedUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
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

