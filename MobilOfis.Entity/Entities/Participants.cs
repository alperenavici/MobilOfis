namespace MobilOfis.Entity;

public class Participants
{
    public Guid ParticipantId { get; set; }
    public Guid EventId { get; set; }
    public string UserId { get; set; }
    public virtual User User { get; set; }
    public virtual Events Event { get; set; }
    
    public virtual ICollection<User> Users { get; set; }
    public virtual ICollection<Events> Events { get; set; }
}