namespace MobilOfis.Entity;

public class Departments
{
    public Guid DepartmentId { get; set; } 
    public string? DepartmentName { get; set; } 
    
    public Guid? ManagerId { get; set; }
    public virtual User? Manager { get; set; }
    
    public virtual ICollection<User>? Users { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
}