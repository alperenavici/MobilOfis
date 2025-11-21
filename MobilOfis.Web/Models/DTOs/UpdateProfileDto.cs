using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.DTOs;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "Kullanıcı ID zorunludur.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    public string PhoneNumber { get; set; }

    public string ProfilePictureUrl { get; set; }
}

