using Microsoft.EntityFrameworkCore;
using MobilOfis.Core.IRepositories;
using MobilOfis.Data.Context;
using MobilOfis.Entity;

namespace MobilOfis.Data.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public async Task<IEnumerable<User>> GetUsersWithDetailsAsync()
    {
        return await _dbContext.Users
            .Include(u => u.Department)
            .Include(u => u.Manager)
            .OrderBy(u => u.FirstName)
            .ToListAsync();
    }
    public override async Task<User?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Users
            .Include(u => u.Department)
            .Include(u => u.Manager)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }
}

