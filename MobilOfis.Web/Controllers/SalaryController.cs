using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.DTOs;

namespace MobilOfis.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalaryController : ControllerBase
{
    private readonly ISalaryService _salaryService;

    public SalaryController(ISalaryService salaryService)
    {
        _salaryService = salaryService;
    }

    /// <summary>
    /// Maaş güncelle (sadece HR)
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateSalary([FromBody] UpdateSalaryDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var hrUserId = Guid.Parse(userIdClaim.Value);
            await _salaryService.UpdateSalaryAsync(dto.UserId, dto.NewSalary, hrUserId);

            return Ok(new { message = "Maaş başarıyla güncellendi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Maaş bilgisi görüntüle
    /// </summary>
    [Authorize]
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserSalaryInfo(Guid userId)
    {
        try
        {
            var requesterIdClaim = User.FindFirst("userId");
            if (requesterIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var requesterId = Guid.Parse(requesterIdClaim.Value);
            var user = await _salaryService.GetUserSalaryInfoAsync(userId, requesterId);

            var response = new SalaryInfoDto
            {
                UserId = user.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                CurrentSalary = user.Salary,
                LastUpdateDate = user.UpdatedDate
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

