using MobilOfis.Entity.Enums;

namespace MobilOfis.Entity;

public class Leaves
{
    public Guid LeavesId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime EndDate { get; set; }
    public Status Status { get; set; }
    public LeavesType LeavesType { get; set; }
    public Guid? ManagerApprovalId { get; set; } //fk with userId
    
    public virtual ICollection<Notifications> Notifications { get; set; }
    
}