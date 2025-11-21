using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.DTOs;

public class AssignDepartmentManagerDto
{
    [Required(ErrorMessage = "Kullanıcı ID zorunludur.")]
    public Guid UserId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? ManagerId { get; set; }
}

