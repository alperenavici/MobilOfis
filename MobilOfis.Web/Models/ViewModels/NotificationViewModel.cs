using System;

namespace MobilOfis.Web.Models.ViewModels;

public class NotificationViewModel
{
    public Guid NotificationId { get; set; }
    public Guid RecipientUserId { get; set; }
    public string Title { get; set; } = "Bildirim";
    public string Message { get; set; } = string.Empty;
    public DateTime SendDate { get; set; }
    public DateTime CreatedAt => SendDate;
    public bool IsRead { get; set; }
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    
    public string Type => RelatedEntityType ?? "info";
    public string TypeIcon => GetTypeIcon();
    public string TypeColor => GetTypeColor();
    
    private string GetTypeIcon()
    {
        return RelatedEntityType switch
        {
            "Leave" => "calendar-check",
            "Event" => "calendar-event",
            "User" => "person",
            "Department" => "diagram-3",
            "Post" => "heart-fill",
            _ => "bell"
        };
    }
    
    private string GetTypeColor()
    {
        return RelatedEntityType switch
        {
            "Leave" => "success",
            "Event" => "info",
            "User" => "primary",
            "Department" => "warning",
            "Post" => "danger",
            _ => "secondary"
        };
    }
}

