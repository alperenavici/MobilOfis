using MobilOfis.Entity;

namespace MobilOfis.Core.IServices;

public interface IEventService
{
    Task<Events> CreateEventAsync(string title, string description, DateTime startTime, DateTime endTime, string? location, string? eventType, Guid creatorId, List<Guid>? participantIds);
    Task<Events> UpdateEventAsync(Guid eventId, string title, string description, DateTime startTime, DateTime endTime, string? location, string? eventType, Guid userId);
    Task<bool> DeleteEventAsync(Guid eventId, Guid userId);
    Task<bool> AddParticipantAsync(Guid eventId, Guid userId);
    Task<bool> RemoveParticipantAsync(Guid eventId, Guid userId);
    Task<IEnumerable<Events>> GetUpcomingEventsAsync();
    Task<IEnumerable<Events>> GetAllEventsAsync();
    Task<IEnumerable<Events>> GetMyEventsAsync(Guid userId);
    Task<Events> GetEventByIdAsync(Guid eventId);
}

