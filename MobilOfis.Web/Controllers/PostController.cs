using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilOfis.Core.IServices;
using MobilOfis.Entity;
using MobilOfis.Web.Models.DTOs;
using MobilOfis.Web.Models.ViewModels;

namespace MobilOfis.Web.Controllers;

[Authorize]
public class PostController : Controller
{
    private readonly IPostService _postService;
    private readonly ILogger<PostController> _logger;

    public PostController(IPostService postService, ILogger<PostController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var trendingHashtags = await _postService.GetTrendingHashtagsAsync(5);
        ViewBag.TrendingHashtags = trendingHashtags;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePostViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Content) && model.Image == null)
        {
            ModelState.AddModelError(nameof(model.Content), "Paylaşım metni veya görsel yüklemelisiniz.");
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return BadRequest(new { success = false, errors });
        }

        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { success = false, message = "Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın." });
            }

            await using var imageStream = model.Image?.OpenReadStream();

            var createdPost = await _postService.CreatePostAsync(
                userId,
                model.Content ?? string.Empty,
                imageStream,
                model.Image?.FileName,
                model.Image?.Length,
                HttpContext.RequestAborted);

            var dto = MapToDto(createdPost, userId);
            return Json(new { success = true, post = dto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post oluşturulurken hata oluştu.");
            var message = ex.InnerException?.Message ?? ex.Message;
            return StatusCode(500, new { success = false, message = $"Bir hata oluştu: {message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> List(int page = 1, int pageSize = 10)
    {
        try
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 5, 20);
            var currentUserId = GetCurrentUserId();

            var posts = (await _postService.GetLatestPostsAsync(page, pageSize)).ToList();
            var hasMore = (await _postService.GetLatestPostsAsync(page + 1, 1)).Any();

            var response = new PostListResponseDto
            {
                Posts = posts.Select(post => MapToDto(post, currentUserId)),
                HasMore = hasMore
            };

            return Json(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post listesi alınırken hata oluştu.");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Like(Guid postId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _postService.LikePostAsync(postId, userId);
            var count = await _postService.GetLikeCountAsync(postId);
            return Json(new { success = true, count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Unlike(Guid postId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _postService.UnlikePostAsync(postId, userId);
            var count = await _postService.GetLikeCountAsync(postId);
            return Json(new { success = true, count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Comment(Guid postId, string content)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _postService.AddCommentAsync(postId, userId, content);
            var count = await _postService.GetCommentCountAsync(postId);
            
            // Return the new comment to append to UI
            var comments = await _postService.GetCommentsAsync(postId);
            var newComment = comments.Last(); // Simplification, ideally return created comment from service
            
            return Json(new { success = true, count, comment = new {
                authorName = $"{newComment.User.FirstName} {newComment.User.LastName}",
                authorAvatar = newComment.User.ProfilePictureUrl,
                content = newComment.Content,
                createdAtDisplay = newComment.CreatedAt.ToLocalTime().ToString("dd MMM HH:mm")
            }});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetComments(Guid postId)
    {
        try
        {
            var comments = await _postService.GetCommentsAsync(postId);
            var dtos = comments.Select(c => new {
                authorName = $"{c.User.FirstName} {c.User.LastName}",
                authorAvatar = c.User.ProfilePictureUrl,
                content = c.Content,
                createdAtDisplay = c.CreatedAt.ToLocalTime().ToString("dd MMM HH:mm")
            });
            return Json(new { success = true, comments = dtos });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsed) ? parsed : Guid.Empty;
    }

    private static PostResponseDto MapToDto(Post createdPost, Guid currentUserId)
    {
        var authorName = createdPost.User != null
            ? $"{createdPost.User.FirstName} {createdPost.User.LastName}".Trim()
            : "Bilinmeyen Kullanıcı";

        return new PostResponseDto
        {
            PostId = createdPost.PostId,
            Content = createdPost.Content,
            ImageUrl = createdPost.ImagePath,
            AuthorName = authorName,
            AuthorRole = createdPost.User?.Role,
            AuthorAvatar = createdPost.User?.ProfilePictureUrl,
            CreatedAt = createdPost.CreatedAt,
            CreatedAtDisplay = createdPost.CreatedAt.ToLocalTime().ToString("dd MMM yyyy HH:mm"),
            IsOwner = createdPost.UserId == currentUserId,
            LikeCount = createdPost.Likes?.Count ?? 0,
            CommentCount = createdPost.Comments?.Count ?? 0,
            IsLiked = createdPost.Likes?.Any(l => l.UserId == currentUserId) ?? false
        };
    }
}

