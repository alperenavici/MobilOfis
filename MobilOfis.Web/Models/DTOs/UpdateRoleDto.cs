using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.DTOs;

public class UpdateRoleDto
{
    [Required(ErrorMessage = "Kullanıcı ID zorunludur.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Rol zorunludur.")]
    public string Role { get; set; }
}

