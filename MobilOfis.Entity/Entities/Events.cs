using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Entity;

public class Events
{
    public Guid EventId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; }
    public string CreatedByUserId { get; set; }
    
    public virtual User CreatedByUser { get; set; }
    public virtual ICollection<Participants> Participants { get; set; }
    
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}