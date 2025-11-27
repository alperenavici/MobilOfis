using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MobilOfis.Web.Models.ViewModels;

public class CreatePostViewModel
{
    [MaxLength(1000, ErrorMessage = "Paylaşım metni 1000 karakteri geçemez.")]
    public string? Content { get; set; }

    [Display(Name = "Görsel")]
    public IFormFile? Image { get; set; }
}

