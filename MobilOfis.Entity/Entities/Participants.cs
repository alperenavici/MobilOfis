namespace MobilOfis.Entity;

public class Participants
{
    // Composite Primary Key (EventId + UserId)
    public Guid EventId { get; set; } 
    public Guid UserId { get; set; } 
    
    // Navigation Properties - İlişkili Veriler
    public virtual User? User { get; set; } // Katılımcı kullanıcı
    public virtual Events? Event { get; set; } // İlgili etkinlik
}