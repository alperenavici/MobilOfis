namespace MobilOfis.Entity;

public class Notifications
{
    public Guid NotificationId { get; set; } 
    public Guid RecipientUserId { get; set; } 
    public string? Message { get; set; } // Bildirim mesajı
    public DateTime SendDate { get; set; } // Bildirim gönderim tarihi
    public bool IsRead { get; set; } // Bildirim okundu mu
    public string? RelatedEntityType { get; set; } // İlgili varlık tipi (Leave, Event, vs.)
    public Guid? RelatedEntityId { get; set; } 
    public Guid? LeavesId { get; set; } 
    
    public virtual User? RecipientUser { get; set; } // Bildirimi alan kullanıcı
    public virtual Leaves? Leaves { get; set; } // İlgili izin talebi (varsa)
}