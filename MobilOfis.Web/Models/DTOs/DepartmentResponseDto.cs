namespace MobilOfis.Web.Models.DTOs;

public class DepartmentResponseDto
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
}

