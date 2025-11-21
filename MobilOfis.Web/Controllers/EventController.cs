using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.DTOs;

namespace MobilOfis.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    /// Etkinlik oluştur
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var creatorId = Guid.Parse(userIdClaim.Value);
            var eventEntity = await _eventService.CreateEventAsync(
                dto.Title,
                dto.Description,
                dto.StartTime,
                dto.EndTime,
                dto.Location,
                creatorId,
                dto.ParticipantIds
            );

            return Ok(new
            {
                message = "Etkinlik oluşturuldu.",
                eventId = eventEntity.EventId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Etkinlik güncelle
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var eventEntity = await _eventService.UpdateEventAsync(
                id,
                dto.Title,
                dto.Description,
                dto.StartTime,
                dto.EndTime,
                dto.Location,
                userId
            );

            return Ok(new
            {
                message = "Etkinlik güncellendi.",
                eventId = eventEntity.EventId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Etkinlik sil
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var userId = Guid.Parse(userIdClaim.Value);
            await _eventService.DeleteEventAsync(id, userId);

            return Ok(new { message = "Etkinlik silindi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Yaklaşan etkinlikler
    /// </summary>
    [Authorize]
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingEvents()
    {
        try
        {
            var events = await _eventService.GetUpcomingEventsAsync();

            var response = events.Select(e => new EventResponseDto
            {
                EventId = e.EventId,
                Title = e.Title,
                Description = e.Description,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                CreatedByUserId = e.CreatedByUserId,
                CreatedByName = $"{e.CreatedByUser?.FirstName} {e.CreatedByUser?.LastName}",
                CreatedDate = e.CreatedDate,
                Participants = e.Participants?.Select(p => new ParticipantDto
                {
                    UserId = p.UserId,
                    UserName = $"{p.User?.FirstName} {p.User?.LastName}"
                }).ToList()
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Katıldığım etkinlikler
    /// </summary>
    [Authorize]
    [HttpGet("my-events")]
    public async Task<IActionResult> GetMyEvents()
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var events = await _eventService.GetMyEventsAsync(userId);

            var response = events.Select(e => new EventResponseDto
            {
                EventId = e.EventId,
                Title = e.Title,
                Description = e.Description,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                CreatedByUserId = e.CreatedByUserId,
                CreatedByName = $"{e.CreatedByUser?.FirstName} {e.CreatedByUser?.LastName}",
                CreatedDate = e.CreatedDate,
                Participants = e.Participants?.Select(p => new ParticipantDto
                {
                    UserId = p.UserId,
                    UserName = $"{p.User?.FirstName} {p.User?.LastName}"
                }).ToList()
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Etkinliğe katıl
    /// </summary>
    [Authorize]
    [HttpPost("{id}/join")]
    public async Task<IActionResult> JoinEvent(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var userId = Guid.Parse(userIdClaim.Value);
            await _eventService.AddParticipantAsync(id, userId);

            return Ok(new { message = "Etkinliğe katıldınız." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Etkinlikten ayrıl
    /// </summary>
    [Authorize]
    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> LeaveEvent(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı." });
            }

            var userId = Guid.Parse(userIdClaim.Value);
            await _eventService.RemoveParticipantAsync(id, userId);

            return Ok(new { message = "Etkinlikten ayrıldınız." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Etkinlik detayı
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(Guid id)
    {
        try
        {
            var eventEntity = await _eventService.GetEventByIdAsync(id);

            var response = new EventResponseDto
            {
                EventId = eventEntity.EventId,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                StartTime = eventEntity.StartTime,
                EndTime = eventEntity.EndTime,
                Location = eventEntity.Location,
                CreatedByUserId = eventEntity.CreatedByUserId,
                CreatedByName = $"{eventEntity.CreatedByUser?.FirstName} {eventEntity.CreatedByUser?.LastName}",
                CreatedDate = eventEntity.CreatedDate,
                Participants = eventEntity.Participants?.Select(p => new ParticipantDto
                {
                    UserId = p.UserId,
                    UserName = $"{p.User?.FirstName} {p.User?.LastName}"
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

