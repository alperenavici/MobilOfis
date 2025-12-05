using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Web.Models.DTOs;

public class TokenDto
{
    [Required(ErrorMessage = "Token zorunludur.")]
    public string Token { get; set; } = string.Empty;
}

