using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.ViewModels;
using System.Security.Claims;

namespace MobilOfis.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IAuthServices _authServices;
    private readonly ILeaveService _leaveService;
    private readonly IEventService _eventService;
    private readonly INotificationService _notificationService;
    private readonly IDepartmentService _departmentService;

    public DashboardController(
        IAuthServices authServices,
        ILeaveService leaveService,
        IEventService eventService,
        INotificationService notificationService,
        IDepartmentService departmentService)
    {
        _authServices = authServices;
        _leaveService = leaveService;
        _eventService = eventService;
        _notificationService = notificationService;
        _departmentService = departmentService;
    }

    /// <summary>
    /// Dashboard ana sayfası - Role'e göre farklı view render eder
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";

            var viewModel = new DashboardViewModel
            {
                CurrentUser = await GetCurrentUserViewModelAsync(userId),
                PendingLeavesCount = await GetPendingLeavesCountAsync(userId),
                ApprovedLeavesCount = await GetApprovedLeavesCountAsync(userId),
                UpcomingEventsCount = await GetUpcomingEventsCountAsync(),
                UnreadNotificationsCount = await GetUnreadNotificationsCountAsync(userId),
                RecentLeaves = await GetRecentLeavesAsync(userId, 5),
                UpcomingEvents = await GetUpcomingEventsAsync(3),
                RecentNotifications = await GetRecentNotificationsAsync(userId, 5)
            };

            // Role'e göre ek veriler yükle
            if (userRole == "Manager" || userRole == "HR" || userRole == "Admin")
            {
                viewModel.PendingApprovals = await GetPendingApprovalsAsync(userId, userRole);
                viewModel.TeamMembers = await GetTeamMembersAsync(userId);
            }

            if (userRole == "HR" || userRole == "Admin")
            {
                viewModel.TotalEmployees = await GetTotalEmployeesCountAsync();
                viewModel.ActiveEmployees = await GetActiveEmployeesCountAsync();
                viewModel.DepartmentDistribution = await GetDepartmentDistributionAsync();
            }

            ViewBag.UserRole = userRole;
            return View(viewModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DashboardController.Index: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            TempData["ErrorMessage"] = $"Dashboard yüklenirken hata oluştu: {ex.Message}";
            return RedirectToAction("Logout", "Auth");
        }
    }

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

    private async Task<UserViewModel> GetCurrentUserViewModelAsync(Guid userId)
    {
        var user = await _authServices.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        return new UserViewModel
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
            Role = user.Role,
            IsActive = user.IsActive,
            HireDate = user.HireDate,
            LastLoginDate = user.LastLoginDate
        };
    }

    private async Task<int> GetPendingLeavesCountAsync(Guid userId)
    {
        try
        {
            var leaves = await _leaveService.GetMyLeavesAsync(userId);
            return leaves.Count(l => l.Status == Entity.Enums.Status.Pending);
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> GetApprovedLeavesCountAsync(Guid userId)
    {
        try
        {
            var leaves = await _leaveService.GetMyLeavesAsync(userId);
            return leaves.Count(l => l.Status == Entity.Enums.Status.Approved);
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> GetUpcomingEventsCountAsync()
    {
        try
        {
            var events = await _eventService.GetUpcomingEventsAsync();
            return events.Count(e => e.StartTime > DateTime.Now && e.StartTime < DateTime.Now.AddDays(7));
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> GetUnreadNotificationsCountAsync(Guid userId)
    {
        try
        {
            var notifications = await _notificationService.GetMyNotificationsAsync(userId);
            return notifications.Count(n => !n.IsRead);
        }
        catch
        {
            return 0;
        }
    }

    private async Task<List<LeaveViewModel>> GetRecentLeavesAsync(Guid userId, int count)
    {
        try
        {
            var leaves = await _leaveService.GetMyLeavesAsync(userId);
            return leaves.OrderByDescending(l => l.RequestDate)
                .Take(count)
                .Select(l => new LeaveViewModel
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
                    ManagerApprovalDate = l.ManagerApprovalDate
                }).ToList();
        }
        catch
        {
            return new List<LeaveViewModel>();
        }
    }

    private async Task<List<EventViewModel>> GetUpcomingEventsAsync(int count)
    {
        try
        {
            var events = await _eventService.GetUpcomingEventsAsync();
            return events.Where(e => e.StartTime > DateTime.Now)
                .OrderBy(e => e.StartTime)
                .Take(count)
                .Select(e => new EventViewModel
                {
                    EventId = e.EventId,
                    Title = e.Title ?? string.Empty,
                    Description = e.Description ?? string.Empty,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location ?? string.Empty,
                    CreatedByUserId = e.CreatedByUserId,
                    CreatedByUserName = e.CreatedByUser != null ? 
                        $"{e.CreatedByUser.FirstName} {e.CreatedByUser.LastName}" : null,
                    ParticipantCount = e.Participants?.Count ?? 0
                }).ToList();
        }
        catch
        {
            return new List<EventViewModel>();
        }
    }

    private async Task<List<NotificationViewModel>> GetRecentNotificationsAsync(Guid userId, int count)
    {
        try
        {
            var notifications = await _notificationService.GetMyNotificationsAsync(userId);
            return notifications.OrderByDescending(n => n.SendDate)
                .Take(count)
                .Select(n => new NotificationViewModel
                {
                    NotificationId = n.NotificationId,
                    RecipientUserId = n.RecipientUserId,
                    Message = n.Message ?? string.Empty,
                    SendDate = n.SendDate,
                    IsRead = n.IsRead,
                    RelatedEntityType = n.RelatedEntityType,
                    RelatedEntityId = n.RelatedEntityId
                }).ToList();
        }
        catch
        {
            return new List<NotificationViewModel>();
        }
    }

    private async Task<List<LeaveViewModel>> GetPendingApprovalsAsync(Guid userId, string role)
    {
        try
        {
            if (role == "Manager")
            {
                var leaves = await _leaveService.GetPendingLeavesForManagerAsync(userId);
                return leaves.Select(l => new LeaveViewModel
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
                }).ToList();
            }
            else if (role == "HR" || role == "Admin")
            {
                var leaves = await _leaveService.GetPendingLeavesForHRAsync();
                return leaves.Select(l => new LeaveViewModel
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
                }).ToList();
            }
            return new List<LeaveViewModel>();
        }
        catch
        {
            return new List<LeaveViewModel>();
        }
    }

    private Task<List<UserViewModel>> GetTeamMembersAsync(Guid userId)
    {
        // Bu metod şu an basit bir implementasyon
        // Gerçek implementasyonda manager'ın subordinates'lerini getirmeli
        try
        {
            return Task.FromResult(new List<UserViewModel>());
        }
        catch
        {
            return Task.FromResult(new List<UserViewModel>());
        }
    }

    private Task<int> GetTotalEmployeesCountAsync()
    {
        try
        {
            // Bu metod bir IUserService'den total employee count getirmeli
            // Şimdilik basit bir sayı dönüyoruz
            return Task.FromResult(0);
        }
        catch
        {
            return Task.FromResult(0);
        }
    }

    private Task<int> GetActiveEmployeesCountAsync()
    {
        try
        {
            // Bu metod bir IUserService'den active employee count getirmeli
            return Task.FromResult(0);
        }
        catch
        {
            return Task.FromResult(0);
        }
    }

    private async Task<Dictionary<string, int>> GetDepartmentDistributionAsync()
    {
        try
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return departments.ToDictionary(
                d => d.DepartmentName ?? "Bilinmeyen",
                d => d.Users?.Count ?? 0
            );
        }
        catch
        {
            return new Dictionary<string, int>();
        }
    }

    #endregion

    #region API Actions

    /// <summary>
    /// Dashboard istatistiklerini getir (API)
    /// </summary>
    [HttpGet]
    [Route("api/[controller]/stats")]
    public async Task<IActionResult> GetStatsApi()
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";

            var stats = new
            {
                pendingLeaves = await GetPendingLeavesCountAsync(userId),
                approvedLeaves = await GetApprovedLeavesCountAsync(userId),
                upcomingEvents = await GetUpcomingEventsCountAsync(),
                unreadNotifications = await GetUnreadNotificationsCountAsync(userId)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Son izinleri getir (API)
    /// </summary>
    [HttpGet]
    [Route("api/[controller]/recent-leaves")]
    public async Task<IActionResult> GetRecentLeavesApi()
    {
        try
        {
            var userId = GetCurrentUserId();
            var leaves = await GetRecentLeavesAsync(userId, 5);
            return Ok(leaves);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Yaklaşan etkinlikleri getir (API)
    /// </summary>
    [HttpGet]
    [Route("api/[controller]/upcoming-events")]
    public async Task<IActionResult> GetUpcomingEventsApi()
    {
        try
        {
            var events = await GetUpcomingEventsAsync(3);
            return Ok(events);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Son bildirimleri getir (API)
    /// </summary>
    [HttpGet]
    [Route("api/[controller]/recent-notifications")]
    public async Task<IActionResult> GetRecentNotificationsApi()
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await GetRecentNotificationsAsync(userId, 5);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Yönetici için onay bekleyen izinleri getir (API)
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpGet]
    [Route("api/[controller]/pending-approvals")]
    public async Task<IActionResult> GetPendingApprovalsApi()
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";
            
            var leaves = await GetPendingApprovalsAsync(userId, userRole);
            return Ok(leaves);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// HR için genel istatistikleri getir (API)
    /// </summary>
    [Authorize(Policy = "HROnly")]
    [HttpGet]
    [Route("api/[controller]/hr-stats")]
    public async Task<IActionResult> GetHRStatsApi()
    {
        try
        {
            var stats = new
            {
                totalEmployees = await GetTotalEmployeesCountAsync(),
                activeEmployees = await GetActiveEmployeesCountAsync(),
                departmentDistribution = await GetDepartmentDistributionAsync()
            };
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion
}

