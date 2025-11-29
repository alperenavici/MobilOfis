using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.DTOs;
using MobilOfis.Web.Models.ViewModels;
using MobilOfis.Entity.Enums;

namespace MobilOfis.Web.Controllers;

public class LeaveController : Controller
{
    private readonly ILeaveService _leaveService;

    public LeaveController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    #region MVC Actions

    /// <summary>
    /// İzinlerim sayfası
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> MyLeaves(string? status = null, string? leavesType = null, int? year = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var leaves = await _leaveService.GetMyLeavesAsync(userId);

            // Filtreleme
            Console.WriteLine($"MyLeaves Filter: Status={status}, Type={leavesType}, Year={year}");
            
            if (!string.IsNullOrEmpty(status))
            {
                leaves = leaves.Where(l => l.Status.ToString() == status).ToList();
            }
            if (!string.IsNullOrEmpty(leavesType))
            {
                Console.WriteLine($"Filtering by LeaveType: {leavesType}");
                leaves = leaves.Where(l => l.LeavesType.ToString() == leavesType).ToList();
                Console.WriteLine($"Filtered count: {leaves.Count()}");
            }
            if (year.HasValue)
            {
                leaves = leaves.Where(l => l.StartDate.Year == year.Value).ToList();
            }

            int totalLeaveDays;
            if (!string.IsNullOrEmpty(leavesType))
            {
                totalLeaveDays = 14;
            }
            else
            {
                var leaveTypeCount = Enum.GetValues(typeof(LeavesType)).Length;
                totalLeaveDays = leaveTypeCount * 14;
            }

           
            
            var allMyLeaves = await _leaveService.GetMyLeavesAsync(userId);
            var approvedLeaves = allMyLeaves.Where(l => l.Status == Status.Approved);

            if (!string.IsNullOrEmpty(leavesType))
            {
                approvedLeaves = approvedLeaves.Where(l => l.LeavesType.ToString() == leavesType);
            }
            
            if (year.HasValue)
            {
                approvedLeaves = approvedLeaves.Where(l => l.StartDate.Year == year.Value);
            }

            var usedLeaveDays = approvedLeaves.Sum(l => (l.EndDate - l.StartDate).Days + 1);

            var viewModel = new LeaveListViewModel
            {
                Leaves = leaves.Select(l => MapToViewModel(l)).ToList(),
                Filters = new LeaveFilterViewModel
                {
                    Status = status,
                    LeavesType = leavesType,
                    Year = year
                },
                TotalLeaveDays = totalLeaveDays,
                UsedLeaveDays = usedLeaveDays,
                RemainingLeaveDays = totalLeaveDays - usedLeaveDays
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LeaveController.MyLeaves: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }

    /// <summary>
    /// İzin detay sayfası
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Detail(Guid id)
    {
        try
        {
            var leave = await _leaveService.GetLeaveByIdAsync(id);
            var viewModel = MapToViewModel(leave);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(MyLeaves));
        }
    }

    /// <summary>
    /// İzin talebi oluştur (Modal'dan post)
    /// </summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLeaveDto dto)
    {
        try
        {
            Console.WriteLine("LeaveController.Create started.");
            if (dto == null)
            {
                Console.WriteLine("DTO is null!");
                throw new Exception("Form verisi alınamadı.");
            }
            Console.WriteLine($"DTO: Start={dto.StartDate}, End={dto.EndDate}, Type={dto.LeavesType}, Reason={dto.Reason}");

            var userId = GetCurrentUserId();
            Console.WriteLine($"UserId: {userId}");

            var leave = await _leaveService.CreateLeaveRequestAsync(
                userId,
                dto.StartDate,
                dto.EndDate,
                dto.LeavesType,
                dto.Reason
            );
            Console.WriteLine("CreateLeaveRequestAsync completed.");

            TempData["SuccessMessage"] = "İzin talebiniz başarıyla oluşturuldu.";
            return RedirectToAction(nameof(MyLeaves));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LeaveController.Create: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(MyLeaves));
        }
    }

    /// <summary>
    /// İzin iptal et (AJAX)
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var leave = await _leaveService.GetLeaveByIdAsync(id);
            
            if (leave.UserId != userId)
            {
                return Json(new { success = false, message = "Bu işlemi yapmaya yetkiniz yok." });
            }

            if (leave.Status != Status.Pending)
            {
                return Json(new { success = false, message = "Sadece bekleyen izinleri iptal edebilirsiniz." });
            }

            // İzin iptal işlemi burada yapılacak (service'de metod eklenmeli)
            return Json(new { success = true, message = "İzin talebi iptal edildi." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Manager onay bekleyen izinler sayfası
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpGet]
    public async Task<IActionResult> PendingManager()
    {
        try
        {
            var managerId = GetCurrentUserId();
            var leaves = await _leaveService.GetPendingLeavesForManagerAsync(managerId);

            var viewModel = new LeaveListViewModel
            {
                Leaves = leaves.Select(l => MapToViewModel(l)).ToList()
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
    /// Manager olarak izin onayla
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost]
    public async Task<IActionResult> ApproveAsManager(Guid id)
    {
        try
        {
            var managerId = GetCurrentUserId();
            await _leaveService.ApproveLeaveByManagerAsync(id, managerId);

            TempData["SuccessMessage"] = "İzin talebi onaylandı ve HR'a gönderildi.";
            return RedirectToAction(nameof(PendingManager));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(PendingManager));
        }
    }

    /// <summary>
    /// Manager olarak izin onayla (AJAX için route)
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost("ApproveByManager/{id}")]
    public async Task<IActionResult> ApproveByManager(Guid id)
    {
        try
        {
            var managerId = GetCurrentUserId();
            await _leaveService.ApproveLeaveByManagerAsync(id, managerId);

            return Json(new { success = true, message = "İzin talebi onaylandı ve HR'a gönderildi." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// İzin reddet
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, string rejectionReason)
    {
        try
        {
            var approverId = GetCurrentUserId();
            await _leaveService.RejectLeaveAsync(id, approverId, rejectionReason);

            TempData["SuccessMessage"] = "İzin talebi reddedildi.";
            return RedirectToAction(nameof(PendingManager));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(PendingManager));
        }
    }

    /// <summary>
    /// İzin reddet (AJAX için route)
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost("RejectByManager/{id}")]
    public async Task<IActionResult> RejectByManager(Guid id, [FromBody] RejectLeaveDto dto)
    {
        try
        {
            var approverId = GetCurrentUserId();
            await _leaveService.RejectLeaveAsync(id, approverId, dto.RejectionReason);

            return Json(new { success = true, message = "İzin talebi reddedildi." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// HR onay bekleyen izinler sayfası
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpGet]
    public async Task<IActionResult> PendingHR()
    {
        try
        {
            var leaves = await _leaveService.GetPendingLeavesForHRAsync();

            var viewModel = new LeaveListViewModel
            {
                Leaves = leaves.Select(l => MapToViewModel(l)).ToList()
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
    /// HR olarak izin onayla
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpPost]
    public async Task<IActionResult> ApproveAsHR(Guid id)
    {
        try
        {
            var hrUserId = GetCurrentUserId();
            await _leaveService.ApproveLeaveByHRAsync(id, hrUserId);

            TempData["SuccessMessage"] = "İzin talebi kesin olarak onaylandı.";
            return RedirectToAction(nameof(PendingHR));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(PendingHR));
        }
    }

    /// <summary>
    /// Tüm izinler (HR/Admin)
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpGet]
    public IActionResult AllLeaves()
    {
        try
        {
            // Bu metod service'de implement edilmeli
            var viewModel = new LeaveListViewModel
            {
                Leaves = new List<LeaveViewModel>()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }

    #endregion

    #region API Actions

    /// <summary>
    /// İzin talebi oluştur (API)
    /// </summary>
    [Authorize]
    [HttpPost]
    [Route("api/[controller]/request")]
    public async Task<IActionResult> CreateLeaveRequestApi([FromBody] CreateLeaveDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
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
    /// Kendi izinlerini listele (API)
    /// </summary>
    [Authorize]
    [HttpGet]
    [Route("api/[controller]/my-leaves")]
    public async Task<IActionResult> GetMyLeavesApi()
    {
        try
        {
            var userId = GetCurrentUserId();
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
    /// Manager onayı bekleyen izinler (API)
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpGet]
    [Route("api/[controller]/pending-manager")]
    public async Task<IActionResult> GetPendingLeavesForManagerApi()
    {
        try
        {
            var managerId = GetCurrentUserId();
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
    /// HR onayı bekleyen izinler (API)
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpGet]
    [Route("api/[controller]/pending-hr")]
    public async Task<IActionResult> GetPendingLeavesForHRApi()
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
    /// Manager olarak izin onayla (API)
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost]
    [Route("api/[controller]/approve-manager/{id}")]
    public async Task<IActionResult> ApproveLeaveByManagerApi(Guid id)
    {
        try
        {
            var managerId = GetCurrentUserId();
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
    /// HR olarak izin onayla (API)
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpPost]
    [Route("api/[controller]/approve-hr/{id}")]
    public async Task<IActionResult> ApproveLeaveByHRApi(Guid id)
    {
        try
        {
            var hrUserId = GetCurrentUserId();
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
    /// İzin talebini reddet (API)
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost]
    [Route("api/[controller]/reject/{id}")]
    public async Task<IActionResult> RejectLeaveApi(Guid id, [FromBody] RejectLeaveDto dto)
    {
        try
        {
            var approverId = GetCurrentUserId();
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
    /// İzin talebini reddet (HR) (API)
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpPost]
    [Route("api/[controller]/reject-hr/{id}")]
    public async Task<IActionResult> RejectLeaveByHRApi(Guid id, [FromBody] RejectLeaveDto dto)
    {
        try
        {
            var approverId = GetCurrentUserId();
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
    /// Toplu izin onayla (HR) (API)
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpPost]
    [Route("api/[controller]/bulk-approve-hr")]
    public async Task<IActionResult> BulkApproveLeavesByHRApi([FromBody] BulkApproveDto dto)
    {
        try
        {
            var hrUserId = GetCurrentUserId();
            var approvedCount = 0;
            var errors = new List<string>();

            foreach (var leaveId in dto.LeaveIds)
            {
                try
                {
                    await _leaveService.ApproveLeaveByHRAsync(leaveId, hrUserId);
                    approvedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"İzin ID {leaveId}: {ex.Message}");
                }
            }

            if (approvedCount == 0 && errors.Any())
            {
                return BadRequest(new { message = "Hiçbir izin onaylanamadı.", errors });
            }

            return Ok(new
            {
                message = $"{approvedCount} izin başarıyla onaylandı.",
                approvedCount,
                errors
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// İzin talebini iptal et (API)
    /// </summary>
    [Authorize]
    [HttpPost]
    [Route("api/[controller]/cancel/{id}")]
    public async Task<IActionResult> CancelLeaveApi(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _leaveService.CancelLeaveAsync(id, userId);
            
            return Ok(new { message = "İzin talebi iptal edildi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// İzin detayı (API)
    /// </summary>
    [Authorize]
    [HttpGet]
    [Route("api/[controller]/{id}")]
    public async Task<IActionResult> GetLeaveByIdApi(Guid id)
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

    #endregion

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Kullanıcı bilgisi bulunamadı.");
        }
        return userId;
    }

    private LeaveViewModel MapToViewModel(Entity.Leaves leave)
    {
        return new LeaveViewModel
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
    }

    #endregion
}
