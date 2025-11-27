namespace MobilOfis.Web.Models.ViewModels;

public class EventListViewModel
{
    public List<EventViewModel> Events { get; set; } = new();
    public string ViewMode { get; set; } = "list"; // list, calendar, card
}

