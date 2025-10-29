namespace MobilOfis.Entity;

public class Participants
{
    public Guid ParticipantId { get; set; }
    public Guid EventId { get; set; }
    public string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
    public virtual Events Event { get; set; }
    
    public virtual ICollection<ApplicationUser> Users { get; set; }
    public virtual ICollection<Events> Events { get; set; }
}