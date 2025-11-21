using MobilOfis.Entity;

namespace MobilOfis.Core.IRepositories;

public interface IEventRepository : IGenericRepository<Events>
{
    Task<IEnumerable<Events>> GetUpcomingEventsAsync();
    Task<IEnumerable<Events>> GetEventsByUserIdAsync(Guid userId);
    Task<IEnumerable<Events>> GetEventsByDateRangeAsync(DateTime start, DateTime end);
}

