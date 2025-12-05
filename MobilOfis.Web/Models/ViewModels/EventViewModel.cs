using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.ViewModels;

public class EventViewModel
{
    public Guid EventId { get; set; }
    
    [Required(ErrorMessage = "Başlık gereklidir")]
    [Display(Name = "Başlık")]
    public string Title { get; set; } = string.Empty;
    
    [Display(Name = "Açıklama")]
    public string Description { get; set; } = string.Empty;
    
    public string EventType { get; set; } = "General";
    public string EventTypeDisplay => EventType switch
    {
        "Meeting" => "Toplantı",
        "Training" => "Eğitim",
        "Social" => "Sosyal Etkinlik",
        "Other" => "Diğer",
        _ => EventType
    };
    

    
    [Required(ErrorMessage = "Başlangıç zamanı gereklidir")]
    [Display(Name = "Başlangıç Zamanı")]
    public DateTime StartTime { get; set; }
    public DateTime StartDate => StartTime;
    
    [Required(ErrorMessage = "Bitiş zamanı gereklidir")]
    [Display(Name = "Bitiş Zamanı")]
    public DateTime EndTime { get; set; }
    public DateTime EndDate => EndTime;
    
    [Display(Name = "Konum")]
    public string Location { get; set; } = string.Empty;
    
    public Guid CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public string CreatedByName => CreatedByUserName ?? string.Empty;
    
    public int ParticipantCount { get; set; }
    public bool IsUserParticipant { get; set; }
    public bool IsPastEvent { get; set; }
    public List<ParticipantViewModel> Participants { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public DateTime CreatedAt => CreatedDate;
}

public class ParticipantViewModel
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Name => UserName;
    public string? ProfilePictureUrl { get; set; }
}

