using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.DTOs;
using MobilOfis.Web.Models.ViewModels;

namespace MobilOfis.Web.Controllers;

[Authorize]
public class SalaryController : Controller
{
    private readonly ISalaryService _salaryService;
    private readonly IAuthServices _authServices;

    public SalaryController(ISalaryService salaryService, IAuthServices authServices)
    {
        _salaryService = salaryService;
        _authServices = authServices;
    }

    #region MVC Actions
    
    [HttpGet]
    public async Task<IActionResult> MySalary()
    {
        try
        {
            var userId = GetCurrentUserId();
            var salaryInfo = await _salaryService.GetUserSalaryInfoAsync(userId, userId);
            var viewModel = new SalaryViewModel
            {
                UserId = userId,
                GrossSalary = salaryInfo.Salary ?? 0
            };
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
    public IActionResult Management()
    {
        return View();
    }

    [HttpGet]
    public IActionResult DownloadPayslip(int month, int year)
    {
        // PDF oluşturma işlemi buraya eklenecek
        return File(new byte[0], "application/pdf", $"bordro_{month}_{year}.pdf");
    }

    #endregion

    #region API Actions
    
    [Authorize(Policy = "HROnly")]
    [HttpPut]
    [Route("api/[controller]/update")]
    public async Task<IActionResult> UpdateSalaryApi([FromBody] UpdateSalaryDto dto)
    {
        try
        {
            var hrUserId = GetCurrentUserId();
            await _salaryService.UpdateSalaryAsync(dto.UserId, dto.NewSalary, hrUserId);
            return Ok(new { message = "Maaş başarıyla güncellendi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/[controller]/{userId}")]
    public async Task<IActionResult> GetUserSalaryInfoApi(Guid userId)
    {
        try
        {
            var requesterId = GetCurrentUserId();
            var salaryInfo = await _salaryService.GetUserSalaryInfoAsync(userId, requesterId);
            return Ok(salaryInfo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "HROnly")]
    [HttpGet]
    [Route("api/[controller]/all")]
    public async Task<IActionResult> GetAllSalariesApi()
    {
        try
        {
            // Tüm kullanıcıların maaş bilgilerini getir (service'de implement edilmeli)
            // Şimdilik boş liste döndürüyoruz
            return Ok(new List<object>());
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/[controller]/history/{userId}")]
    public async Task<IActionResult> GetSalaryHistoryApi(Guid userId)
    {
        try
        {
            var requesterId = GetCurrentUserId();
            // Sadece kendi maaş geçmişini veya HR ise herkesin geçmişini görebilir
            if (userId != requesterId && !User.IsInRole("HR") && !User.IsInRole("Admin"))
            {
                return Unauthorized(new { message = "Bu işlem için yetkiniz yok." });
            }

            // Maaş geçmişi service'de implement edilmeli
            // Şimdilik boş liste döndürüyoruz
            return Ok(new List<object>());
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    private Guid GetCurrentUserId()
    {
        return Guid.Parse(User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());
    }
}
