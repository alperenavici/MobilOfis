using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.DTOs;

public class UpdateStatusDto
{
    [Required(ErrorMessage = "Kullanıcı ID zorunludur.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Aktiflik durumu zorunludur.")]
    public bool IsActive { get; set; }
}

