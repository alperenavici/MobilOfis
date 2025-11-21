using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;

namespace MobilOfis.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Tüm bildirimlerim
    /// </summary>
    [Authorize]
    [HttpGet("my-notifications")]
    public async Task<IActionResult> GetMyNotifications()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var notifications = await _notificationService.GetMyNotificationsAsync(userId);

            var response = notifications.Select(n => new
            {
                notificationId = n.NotificationId,
                message = n.Message,
                sendDate = n.SendDate,
                isRead = n.IsRead,
                relatedEntityType = n.RelatedEntityType,
                relatedEntityId = n.RelatedEntityId
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Okunmamış bildirimler
    /// </summary>
    [Authorize]
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);

            var response = notifications.Select(n => new
            {
                notificationId = n.NotificationId,
                message = n.Message,
                sendDate = n.SendDate,
                isRead = n.IsRead,
                relatedEntityType = n.RelatedEntityType,
                relatedEntityId = n.RelatedEntityId
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Bildirimi okundu olarak işaretle
    /// </summary>
    [Authorize]
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
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

    /// <summary>
    /// Tüm bildirimleri okundu olarak işaretle
    /// </summary>
    [Authorize]
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var userId = Guid.Parse(userIdClaim.Value);
            await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(new { message = "Tüm bildirimler okundu olarak işaretlendi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

