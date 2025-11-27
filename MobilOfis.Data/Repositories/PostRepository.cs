using Microsoft.EntityFrameworkCore;
using MobilOfis.Core.IRepositories;
using MobilOfis.Data.Context;
using MobilOfis.Entity;

namespace MobilOfis.Data.Repositories;

public class PostRepository : GenericRepository<Post>, IPostRepository
{
    private readonly ApplicationDbContext _context;

    public PostRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Post>> GetLatestPostsAsync(int page, int pageSize)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Post?> GetByIdWithUserAsync(Guid postId)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PostId == postId);
    }

    public async Task AddLikeAsync(PostLike like)
    {
        await _context.PostLikes.AddAsync(like);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveLikeAsync(Guid postId, Guid userId)
    {
        var like = await _context.PostLikes
            .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);
        
        if (like != null)
        {
            _context.PostLikes.Remove(like);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddCommentAsync(PostComment comment)
    {
        await _context.PostComments.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<PostComment>> GetCommentsByPostIdAsync(Guid postId)
    {
        return await _context.PostComments
            .Include(pc => pc.User)
            .Where(pc => pc.PostId == postId)
            .OrderBy(pc => pc.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> IsPostLikedByUserAsync(Guid postId, Guid userId)
    {
        return await _context.PostLikes
            .AnyAsync(pl => pl.PostId == postId && pl.UserId == userId);
    }

    public async Task<int> GetLikeCountAsync(Guid postId)
    {
        return await _context.PostLikes.CountAsync(pl => pl.PostId == postId);
    }

    public async Task<int> GetCommentCountAsync(Guid postId)
    {
        return await _context.PostComments.CountAsync(pc => pc.PostId == postId);
    }
}

