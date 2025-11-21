using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.DTOs;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Kullanıcı ID zorunludur.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Mevcut şifre zorunludur.")]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "Yeni şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string NewPassword { get; set; }
}

