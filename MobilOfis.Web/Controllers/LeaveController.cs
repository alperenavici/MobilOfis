using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.DTOs;

namespace MobilOfis.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    /// <summary>
    /// İzin talebi oluştur
    /// </summary>
    [Authorize]
    [HttpPost("request")]
    public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var leave = await _leaveService.CreateLeaveRequestAsync(
                userId,
                dto.StartDate,
                dto.EndDate,
                dto.LeavesType,
                dto.Reason
            );

            return Ok(new
            {
                message = "İzin talebi oluşturuldu.",
                leaveId = leave.LeavesId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kendi izinlerini listele
    /// </summary>
    [Authorize]
    [HttpGet("my-leaves")]
    public async Task<IActionResult> GetMyLeaves()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var leaves = await _leaveService.GetMyLeavesAsync(userId);

            var response = leaves.Select(l => new LeaveResponseDto
            {
                LeavesId = l.LeavesId,
                UserId = l.UserId,
                UserName = $"{l.User?.FirstName} {l.User?.LastName}",
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                RequestDate = l.RequestDate,
                Status = l.Status.ToString(),
                LeavesType = l.LeavesType.ToString(),
                Reason = l.Reason,
                RejectionReason = l.RejectionReason,
                ManagerApprovalId = l.ManagerApprovalId,
                ManagerApprovalName = l.ManagerApproval != null ? 
                    $"{l.ManagerApproval.FirstName} {l.ManagerApproval.LastName}" : null,
                ManagerApprovalDate = l.ManagerApprovalDate,
                HRApprovalId = l.HRApprovalId,
                HRApprovalDate = l.HRApprovalDate
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Manager onayı bekleyen izinler
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpGet("pending-manager")]
    public async Task<IActionResult> GetPendingLeavesForManager()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var managerId = Guid.Parse(userIdClaim.Value);
            var leaves = await _leaveService.GetPendingLeavesForManagerAsync(managerId);

            var response = leaves.Select(l => new LeaveResponseDto
            {
                LeavesId = l.LeavesId,
                UserId = l.UserId,
                UserName = $"{l.User?.FirstName} {l.User?.LastName}",
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                RequestDate = l.RequestDate,
                Status = l.Status.ToString(),
                LeavesType = l.LeavesType.ToString(),
                Reason = l.Reason
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// HR onayı bekleyen izinler
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpGet("pending-hr")]
    public async Task<IActionResult> GetPendingLeavesForHR()
    {
        try
        {
            var leaves = await _leaveService.GetPendingLeavesForHRAsync();

            var response = leaves.Select(l => new LeaveResponseDto
            {
                LeavesId = l.LeavesId,
                UserId = l.UserId,
                UserName = $"{l.User?.FirstName} {l.User?.LastName}",
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                RequestDate = l.RequestDate,
                Status = l.Status.ToString(),
                LeavesType = l.LeavesType.ToString(),
                Reason = l.Reason,
                ManagerApprovalId = l.ManagerApprovalId,
                ManagerApprovalName = l.ManagerApproval != null ? 
                    $"{l.ManagerApproval.FirstName} {l.ManagerApproval.LastName}" : null,
                ManagerApprovalDate = l.ManagerApprovalDate
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Manager olarak izin onayla
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost("approve-manager/{id}")]
    public async Task<IActionResult> ApproveLeaveByManager(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var managerId = Guid.Parse(userIdClaim.Value);
            var leave = await _leaveService.ApproveLeaveByManagerAsync(id, managerId);

            return Ok(new
            {
                message = "İzin talebi onaylandı. HR onayına gönderildi.",
                leaveId = leave.LeavesId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// HR olarak izin onayla
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpPost("approve-hr/{id}")]
    public async Task<IActionResult> ApproveLeaveByHR(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var hrUserId = Guid.Parse(userIdClaim.Value);
            var leave = await _leaveService.ApproveLeaveByHRAsync(id, hrUserId);

            return Ok(new
            {
                message = "İzin talebi kesin olarak onaylandı.",
                leaveId = leave.LeavesId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// İzin talebini reddet
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost("reject/{id}")]
    public async Task<IActionResult> RejectLeave(Guid id, [FromBody] RejectLeaveDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var approverId = Guid.Parse(userIdClaim.Value);
            var leave = await _leaveService.RejectLeaveAsync(id, approverId, dto.RejectionReason);

            return Ok(new
            {
                message = "İzin talebi reddedildi.",
                leaveId = leave.LeavesId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// İzin detayı
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetLeaveById(Guid id)
    {
        try
        {
            var leave = await _leaveService.GetLeaveByIdAsync(id);

            var response = new LeaveResponseDto
            {
                LeavesId = leave.LeavesId,
                UserId = leave.UserId,
                UserName = $"{leave.User?.FirstName} {leave.User?.LastName}",
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                RequestDate = leave.RequestDate,
                Status = leave.Status.ToString(),
                LeavesType = leave.LeavesType.ToString(),
                Reason = leave.Reason,
                RejectionReason = leave.RejectionReason,
                ManagerApprovalId = leave.ManagerApprovalId,
                ManagerApprovalName = leave.ManagerApproval != null ? 
                    $"{leave.ManagerApproval.FirstName} {leave.ManagerApproval.LastName}" : null,
                ManagerApprovalDate = leave.ManagerApprovalDate,
                HRApprovalId = leave.HRApprovalId,
                HRApprovalDate = leave.HRApprovalDate
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

