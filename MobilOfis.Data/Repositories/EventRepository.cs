using Microsoft.EntityFrameworkCore;
using MobilOfis.Core.IRepositories;
using MobilOfis.Data.Context;
using MobilOfis.Entity;

namespace MobilOfis.Data.Repositories;

public class EventRepository : GenericRepository<Events>, IEventRepository
{
    public EventRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Events>> GetUpcomingEventsAsync()
    {
        return await _dbContext.Events
            .Include(e => e.CreatedByUser)
            .Include(e => e.Participants)
            .ThenInclude(p => p.User)
            .Where(e => e.StartTime >= DateTime.UtcNow)
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Events>> GetEventsByUserIdAsync(Guid userId)
    {
        // Kullanıcının katıldığı veya oluşturduğu etkinlikler
        return await _dbContext.Events
            .Include(e => e.CreatedByUser)
            .Include(e => e.Participants)
            .ThenInclude(p => p.User)
            .Where(e => e.CreatedByUserId == userId || 
                       e.Participants.Any(p => p.UserId == userId))
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Events>> GetEventsByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _dbContext.Events
            .Include(e => e.CreatedByUser)
            .Include(e => e.Participants)
            .ThenInclude(p => p.User)
            .Where(e => e.StartTime <= end && e.EndTime >= start)
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }
}

