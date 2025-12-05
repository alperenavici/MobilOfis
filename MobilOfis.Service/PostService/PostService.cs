using MobilOfis.Core.IRepositories;
using MobilOfis.Core.IServices;
using MobilOfis.Entity;

namespace MobilOfis.Service.PostService;

public class PostService : IPostService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;

    public PostService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
    }

    public async Task<Post> CreatePostAsync(
        Guid userId,
        string content,
        Stream? imageStream,
        string? imageFileName,
        long? imageLength,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content) && imageStream == null)
        {
            throw new ArgumentException("Paylaşım metni veya görselinden en az biri sağlanmalıdır.", nameof(content));
        }

        if (!string.IsNullOrWhiteSpace(content) && content.Length > 1000)
        {
            throw new ArgumentException("Paylaşım metni 1000 karakterden uzun olamaz.", nameof(content));
        }

        string? imagePath = null;
        if (imageStream != null && !string.IsNullOrWhiteSpace(imageFileName) && imageLength.HasValue)
        {
            imagePath = await _fileStorageService.SavePostImageAsync(
                imageStream,
                imageFileName,
                imageLength.Value,
                cancellationToken);
        }

        var post = new Post
        {
            PostId = Guid.NewGuid(),
            UserId = userId,
            Content = content?.Trim() ?? string.Empty,
            ImagePath = imagePath,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Posts.AddAsync(post);
        await _unitOfWork.SaveChangesAsync();

        return await _unitOfWork.Posts.GetByIdWithUserAsync(post.PostId) ?? post;
    }

    public async Task<IEnumerable<Post>> GetLatestPostsAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        return await _unitOfWork.Posts.GetLatestPostsAsync(page, pageSize);
    }

    public async Task<Post?> GetPostByIdAsync(Guid postId)
    {
        return await _unitOfWork.Posts.GetByIdWithUserAsync(postId);
    }
    public async Task LikePostAsync(Guid postId, Guid userId)
    {
        if (await _unitOfWork.Posts.IsPostLikedByUserAsync(postId, userId))
        {
            return; // Already liked
        }

        var like = new PostLike
        {
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Posts.AddLikeAsync(like);

        // Send notification if the liker is not the post owner
        var post = await _unitOfWork.Posts.GetByIdWithUserAsync(postId);
        if (post != null && post.UserId != userId)
        {
            var liker = await _unitOfWork.Users.GetByIdAsync(userId);
            var likerName = liker != null ? $"{liker.FirstName} {liker.LastName}" : "Bir kullanıcı";
            await _notificationService.SendNotificationAsync(post.UserId, $"{likerName} gönderini beğendi.", "Post", postId);
        }
    }

    public async Task UnlikePostAsync(Guid postId, Guid userId)
    {
        await _unitOfWork.Posts.RemoveLikeAsync(postId, userId);
    }

    public async Task AddCommentAsync(Guid postId, Guid userId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Yorum boş olamaz.", nameof(content));
        }

        if (content.Length > 500)
        {
            throw new ArgumentException("Yorum 500 karakterden uzun olamaz.", nameof(content));
        }

        var comment = new PostComment
        {
            CommentId = Guid.NewGuid(),
            PostId = postId,
            UserId = userId,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Posts.AddCommentAsync(comment);

        // Send notification if the commenter is not the post owner
        var post = await _unitOfWork.Posts.GetByIdWithUserAsync(postId);
        if (post != null && post.UserId != userId)
        {
            var commenter = await _unitOfWork.Users.GetByIdAsync(userId);
            var commenterName = commenter != null ? $"{commenter.FirstName} {commenter.LastName}" : "Bir kullanıcı";
            await _notificationService.SendNotificationAsync(post.UserId, $"{commenterName} gönderine yorum yaptı.", "Post", postId);
        }
    }

    public async Task<IEnumerable<PostComment>> GetCommentsAsync(Guid postId)
    {
        return await _unitOfWork.Posts.GetCommentsByPostIdAsync(postId);
    }

    public async Task<bool> IsPostLikedAsync(Guid postId, Guid userId)
    {
        return await _unitOfWork.Posts.IsPostLikedByUserAsync(postId, userId);
    }

    public async Task<int> GetLikeCountAsync(Guid postId)
    {
        return await _unitOfWork.Posts.GetLikeCountAsync(postId);
    }

    public async Task<int> GetCommentCountAsync(Guid postId)
    {
        return await _unitOfWork.Posts.GetCommentCountAsync(postId);
    }

    public async Task<Dictionary<string, int>> GetTrendingHashtagsAsync(int count = 5)
    {
        return await _unitOfWork.Posts.GetTrendingHashtagsAsync(count);
    }
}
