namespace MobilOfis.Entity;

public class Notifications
{
    public Guid NotificationId { get; set; }
    public Guid RecipientUserId { get; set; }
    public string Message { get; set; }
    public DateTime SendDate { get; set; }
    public bool IsRead { get; set; }
    public string RelatedEntityType { get; set; }
    
    public virtual ICollection<User> Users { get; set; }
    
}