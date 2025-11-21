using System.ComponentModel.DataAnnotations;

namespace MobilOfis.Entity;

public class Events
{
    public Guid EventId { get; set; } 
    public string? Title { get; set; } // Etkinlik başlığı
    public string? Description { get; set; } // Etkinlik açıklaması
    public DateTime StartTime { get; set; } // Etkinlik başlangıç zamanı
    public DateTime EndTime { get; set; } // Etkinlik bitiş zamanı
    public string? Location { get; set; } // Etkinlik yeri 
    public Guid CreatedByUserId { get; set; } 
    
    
    public virtual User? CreatedByUser { get; set; } 
    public virtual ICollection<Participants>? Participants { get; set; } // Etkinliğe katılan kullanıcılar
    
    public DateTime CreatedDate { get; set; } // Etkinlik oluşturulma tarihi
    public DateTime? UpdatedDate { get; set; } // Etkinlik son güncellenme tarihi
}