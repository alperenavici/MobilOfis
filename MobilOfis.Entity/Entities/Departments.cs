namespace MobilOfis.Entity;

public class Departments
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    
    
    public virtual ICollection<ApplicationUser> Users { get; set; }
    
}