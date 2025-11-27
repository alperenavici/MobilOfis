namespace MobilOfis.Web.Models.DTOs;

public class SalaryInfoDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal? CurrentSalary { get; set; }
    public DateTime? LastUpdateDate { get; set; }
}

