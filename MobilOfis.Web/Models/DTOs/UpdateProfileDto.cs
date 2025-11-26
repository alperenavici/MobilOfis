using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.DTOs;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "Kullanıcı ID zorunludur.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    public string PhoneNumber { get; set; } = string.Empty;

    public string ProfilePictureUrl { get; set; } = string.Empty;
}

