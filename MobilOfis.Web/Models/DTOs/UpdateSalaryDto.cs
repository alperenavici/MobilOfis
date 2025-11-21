namespace MobilOfis.Web.Models.DTOs;

public class UpdateSalaryDto
{
    public Guid UserId { get; set; }
    public decimal NewSalary { get; set; }
}

