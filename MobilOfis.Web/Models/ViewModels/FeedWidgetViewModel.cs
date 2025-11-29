namespace MobilOfis.Web.Models.ViewModels;

public class FeedWidgetViewModel
{
    public string ContainerClass { get; set; } = "feed-widget";
    public int PageSize { get; set; } = 5;
    public int TextareaRows { get; set; } = 2;
    public bool ShowHeader { get; set; } = true;
    public string HeaderTitle { get; set; } = "Akış";
    public string HeaderIconClass { get; set; } = "bi-lightning-charge-fill";
    public string Placeholder { get; set; } = "Neler oluyor?";
    public string? ProfilePictureUrl { get; set; }
}

