using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.ViewModels;

namespace MobilOfis.Web.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    #region MVC Actions
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetMyNotificationsAsync(userId);
            var viewModel = notifications.Select(n => new NotificationViewModel
            {
                NotificationId = n.NotificationId,
                RecipientUserId = n.RecipientUserId,
                Message = n.Message ?? string.Empty,
                SendDate = n.SendDate,
                IsRead = n.IsRead,
                RelatedEntityType = n.RelatedEntityType,
                RelatedEntityId = n.RelatedEntityId
            }).ToList();
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpPost("MarkAsRead/{id}")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(id);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("GetRecent")]
    public async Task<IActionResult> GetRecent()
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetMyNotificationsAsync(userId);
            var recentNotifications = notifications
                .OrderByDescending(n => n.SendDate)
                .Take(10)
                .Select(n => new
                {
                    id = n.NotificationId,
                    title = GetNotificationTitle(n.RelatedEntityType, n.Message),
                    message = n.Message ?? "Bildirim",
                    isRead = n.IsRead,
                    type = n.RelatedEntityType ?? "Info",
                    createdAt = n.SendDate
                })
                .ToList();

            return Json(recentNotifications);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string GetNotificationTitle(string? relatedEntityType, string? message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            // İlk 50 karakteri al
            return message.Length > 50 ? message.Substring(0, 50) + "..." : message;
        }

        return relatedEntityType switch
        {
            "Leave" => "İzin Bildirimi",
            "Event" => "Etkinlik Bildirimi",
            "User" => "Kullanıcı Bildirimi",
            "Department" => "Departman Bildirimi",
            "Salary" => "Maaş Bildirimi",
            _ => "Bildirim"
        };
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetMyNotificationsAsync(userId);
            var count = notifications.Count(n => !n.IsRead);
            return Json(new { count });
        }
        catch
        {
            return Json(new { count = 0 });
        }
    }

    #endregion

    #region API Actions
    
    [HttpGet]
    [Route("api/[controller]/my-notifications")]
    public async Task<IActionResult> GetMyNotificationsApi()
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetMyNotificationsAsync(userId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Route("api/[controller]/mark-read/{id}")]
    public async Task<IActionResult> MarkAsReadApi(Guid id)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { message = "Bildirim okundu olarak işaretlendi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Route("api/[controller]/mark-all-read")]
    public async Task<IActionResult> MarkAllAsReadApi()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { message = "Tüm bildirimler okundu olarak işaretlendi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/[controller]/unread-count")]
    public async Task<IActionResult> GetUnreadCountApi()
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetMyNotificationsAsync(userId);
            var count = notifications.Count(n => !n.IsRead);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/[controller]/recent")]
    public async Task<IActionResult> GetRecentApi()
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetMyNotificationsAsync(userId);
            var recentNotifications = notifications
                .OrderByDescending(n => n.SendDate)
                .Take(10)
                .Select(n => new
                {
                    id = n.NotificationId,
                    title = GetNotificationTitle(n.RelatedEntityType, n.Message),
                    message = n.Message ?? "Bildirim",
                    isRead = n.IsRead,
                    type = n.RelatedEntityType ?? "Info",
                    createdAt = n.SendDate
                })
                .ToList();

            return Ok(recentNotifications);
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
