using Microsoft.AspNetCore.Identity;

namespace MobilOfis.Entity;

public class User:IdentityUser
{
    public Guid? DepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    
    public virtual Departments Department { get; set; }
    public virtual User Manager { get; set; }
    public virtual ICollection<Participants> Subordinates { get; set; }
    
}