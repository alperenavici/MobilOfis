using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Web.Models.DTOs;
using MobilOfis.Web.Models.ViewModels;

namespace MobilOfis.Web.Controllers;

public class EventController : Controller
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    #region MVC Actions
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index(string viewMode = "calendar")
    {
        try
        {
            var events = await _eventService.GetUpcomingEventsAsync();
            var viewModel = new EventListViewModel
            {
                Events = events.Select(e => new EventViewModel
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    Description = e.Description,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location,
                    CreatedByUserId = e.CreatedByUserId,
                    CreatedByUserName = $"{e.CreatedByUser?.FirstName} {e.CreatedByUser?.LastName}",
                    ParticipantCount = e.Participants?.Count ?? 0
                }).ToList(),
                ViewMode = viewMode
            };
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Detail(Guid id)
    {
        try
        {
            var eventEntity = await _eventService.GetEventByIdAsync(id);
            var viewModel = new EventViewModel
            {
                EventId = eventEntity.EventId,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                StartTime = eventEntity.StartTime,
                EndTime = eventEntity.EndTime,
                Location = eventEntity.Location,
                CreatedByUserId = eventEntity.CreatedByUserId,
                CreatedByUserName = $"{eventEntity.CreatedByUser?.FirstName} {eventEntity.CreatedByUser?.LastName}",
                ParticipantCount = eventEntity.Participants?.Count ?? 0,
                IsUserParticipant = eventEntity.Participants?.Any(p => p.UserId == GetCurrentUserId()) ?? false,
                IsPastEvent = eventEntity.EndTime < DateTime.UtcNow
            };
            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Policy = "ManagerOnly")]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new EventViewModel());
    }

    [Authorize(Policy = "ManagerOnly")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventViewModel model)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _eventService.CreateEventAsync(model.Title, model.Description, model.StartTime, 
                model.EndTime, model.Location, userId, new List<Guid>());
            TempData["SuccessMessage"] = "Etkinlik başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost("JoinEvent/{id}")]
    public async Task<IActionResult> JoinEvent(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _eventService.AddParticipantAsync(id, userId);
            return Json(new { success = true, message = "Etkinliğe başarıyla katıldınız." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("LeaveEvent/{id}")]
    public async Task<IActionResult> LeaveEvent(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _eventService.RemoveParticipantAsync(id, userId);
            return Json(new { success = true, message = "Etkinlikten ayrıldınız." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [Authorize(Policy = "ManagerOnly")]
    [HttpPost("Event/Delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _eventService.DeleteEventAsync(id, userId);
            return Json(new { success = true, message = "Etkinlik silindi." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("GetEventsForCalendar")]
    public async Task<IActionResult> GetEventsForCalendar()
    {
        try
        {
            var events = await _eventService.GetUpcomingEventsAsync();
            var calendarEvents = events.Select(e => new
            {
                id = e.EventId.ToString(),
                title = e.Title,
                startDate = e.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                endDate = e.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                type = "General" // EventType property yoksa default
            }).ToList();

            return Json(calendarEvents);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("ExportToCalendar/{id}")]
    public async Task<IActionResult> ExportToCalendar(Guid id)
    {
        try
        {
            var eventEntity = await _eventService.GetEventByIdAsync(id);
            
            // iCal format oluştur
            var icalContent = $@"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//MobilOfis//Event//EN
BEGIN:VEVENT
UID:{eventEntity.EventId}@mobilofis.com
DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}
DTSTART:{eventEntity.StartTime:yyyyMMddTHHmmssZ}
DTEND:{eventEntity.EndTime:yyyyMMddTHHmmssZ}
SUMMARY:{eventEntity.Title}
DESCRIPTION:{eventEntity.Description ?? ""}
LOCATION:{eventEntity.Location ?? ""}
END:VEVENT
END:VCALENDAR";

            var bytes = System.Text.Encoding.UTF8.GetBytes(icalContent);
            return File(bytes, "text/calendar", $"{eventEntity.Title.Replace(" ", "_")}.ics");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region API Actions
    
    [Authorize(Policy = "ManagerOnly")]
    [HttpPost]
    [Route("api/[controller]")]
    public async Task<IActionResult> CreateEventApi([FromBody] CreateEventDto dto)
    {
        try
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Geçersiz veri gönderildi." });
            }

            var creatorId = GetCurrentUserId();
            var eventEntity = await _eventService.CreateEventAsync(dto.Title, dto.Description, 
                dto.StartTime, dto.EndTime, dto.Location, creatorId, dto.ParticipantIds);
            return Ok(new { message = "Etkinlik oluşturuldu.", eventId = eventEntity.EventId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet]
    [Route("api/[controller]")]
    public async Task<IActionResult> GetAllEventsApi()
    {
        try
        {
            var events = await _eventService.GetUpcomingEventsAsync();
            return Ok(events);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost]
    [Route("api/[controller]/join/{id}")]
    public async Task<IActionResult> JoinEventApi(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _eventService.AddParticipantAsync(id, userId);
            return Ok(new { message = "Etkinliğe başarıyla katıldınız." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost]
    [Route("api/[controller]/leave/{id}")]
    public async Task<IActionResult> LeaveEventApi(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _eventService.RemoveParticipantAsync(id, userId);
            return Ok(new { message = "Etkinlikten ayrıldınız." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "ManagerOnly")]
    [HttpDelete]
    [Route("api/[controller]/{id}")]
    public async Task<IActionResult> DeleteEventApi(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _eventService.DeleteEventAsync(id, userId);
            return Ok(new { message = "Etkinlik silindi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet]
    [Route("api/[controller]/calendar")]
    public async Task<IActionResult> GetCalendarEventsApi()
    {
        try
        {
            var events = await _eventService.GetAllEventsAsync();
            var calendarEvents = events.Select(e => new
            {
                id = e.EventId.ToString(),
                title = e.Title,
                startDate = e.StartTime.ToString("O"), // ISO 8601 format
                endDate = e.EndTime.ToString("O"),     // ISO 8601 format
                type = "General"
            });
            return Ok(calendarEvents);
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
