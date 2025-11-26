using MobilOfis.Entity;

namespace MobilOfis.Core.IRepositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<User>> GetUsersWithDetailsAsync();
}