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

    public async Task<Dictionary<string, int>> GetTrendingHashtagsAsync(int count)
    {
        var recentPosts = await _context.Posts
            .Where(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            .Select(p => p.Content)
            .ToListAsync();

        var hashtagCounts = new Dictionary<string, int>();

        foreach (var content in recentPosts)
        {
            if (string.IsNullOrEmpty(content)) continue;

            var words = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (word.StartsWith("#") && word.Length > 1)
                {
                    var hashtag = word.TrimEnd('.', ',', '!', '?').Substring(1); // Remove # and punctuation
                    if (string.IsNullOrEmpty(hashtag)) continue;
                    
                    if (hashtagCounts.ContainsKey(hashtag))
                    {
                        hashtagCounts[hashtag]++;
                    }
                    else
                    {
                        hashtagCounts[hashtag] = 1;
                    }
                }
            }
        }

        return hashtagCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(count)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}

