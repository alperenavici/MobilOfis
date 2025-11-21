using MobilOfis.Core.IRepositories;
using MobilOfis.Core.IServices;
using MobilOfis.Entity;

namespace MobilOfis.Service.EventService;

public class EventService : IEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public EventService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Events> CreateEventAsync(string title, string description, DateTime startTime, DateTime endTime, string? location, Guid creatorId, List<Guid>? participantIds)
    {
        if (startTime >= endTime)
        {
            throw new Exception("Bitiş zamanı başlangıç zamanından sonra olmalıdır.");
        }

        var eventEntity = new Events
        {
            EventId = Guid.NewGuid(),
            Title = title,
            Description = description,
            StartTime = startTime,
            EndTime = endTime,
            Location = location,
            CreatedByUserId = creatorId,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Events.AddAsync(eventEntity);
        await _unitOfWork.SaveChangesAsync();

        // Katılımcıları ekle
        if (participantIds != null && participantIds.Any())
        {
            foreach (var participantId in participantIds)
            {
                await AddParticipantAsync(eventEntity.EventId, participantId);
            }

            // Katılımcılara bildirim gönder
            await _notificationService.SendEventNotificationAsync(eventEntity, participantIds, "created");
        }

        return eventEntity;
    }

    public async Task<Events> UpdateEventAsync(Guid eventId, string title, string description, DateTime startTime, DateTime endTime, string? location, Guid userId)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (eventEntity == null)
        {
            throw new Exception("Etkinlik bulunamadı.");
        }

        if (eventEntity.CreatedByUserId != userId)
        {
            throw new Exception("Bu etkinliği güncelleme yetkiniz yok.");
        }

        if (startTime >= endTime)
        {
            throw new Exception("Bitiş zamanı başlangıç zamanından sonra olmalıdır.");
        }

        eventEntity.Title = title;
        eventEntity.Description = description;
        eventEntity.StartTime = startTime;
        eventEntity.EndTime = endTime;
        eventEntity.Location = location;
        eventEntity.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Events.Update(eventEntity);
        await _unitOfWork.SaveChangesAsync();

        return eventEntity;
    }

    public async Task<bool> DeleteEventAsync(Guid eventId, Guid userId)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (eventEntity == null)
        {
            throw new Exception("Etkinlik bulunamadı.");
        }

        if (eventEntity.CreatedByUserId != userId)
        {
            throw new Exception("Bu etkinliği silme yetkiniz yok.");
        }

        _unitOfWork.Events.Remove(eventEntity);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AddParticipantAsync(Guid eventId, Guid userId)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (eventEntity == null)
        {
            throw new Exception("Etkinlik bulunamadı.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("Kullanıcı bulunamadı.");
        }

        var participant = new Participants
        {
            EventId = eventId,
            UserId = userId
        };

        await _unitOfWork.Repository<Participants>().AddAsync(participant);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveParticipantAsync(Guid eventId, Guid userId)
    {
        var participants = await _unitOfWork.Repository<Participants>()
            .FindAsync(p => p.EventId == eventId && p.UserId == userId);
        
        var participant = participants.FirstOrDefault();
        if (participant == null)
        {
            throw new Exception("Katılımcı bulunamadı.");
        }

        _unitOfWork.Repository<Participants>().Remove(participant);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Events>> GetUpcomingEventsAsync()
    {
        return await _unitOfWork.Events.GetUpcomingEventsAsync();
    }

    public async Task<IEnumerable<Events>> GetMyEventsAsync(Guid userId)
    {
        return await _unitOfWork.Events.GetEventsByUserIdAsync(userId);
    }

    public async Task<Events> GetEventByIdAsync(Guid eventId)
    {
        var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
        if (eventEntity == null)
        {
            throw new Exception("Etkinlik bulunamadı.");
        }
        return eventEntity;
    }
}

