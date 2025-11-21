using MobilOfis.Entity.Enums;

namespace MobilOfis.Entity;

public class Leaves
{
    public Guid LeavesId { get; set; } 
    public Guid UserId { get; set; } 
    public DateTime StartDate { get; set; } // İzin başlangıç tarihi
    public DateTime RequestDate { get; set; } // Talebin oluşturulma tarihi
    public DateTime EndDate { get; set; } // İzin bitiş tarihi
    public Status Status { get; set; } // İzin durumu 
    public LeavesType LeavesType { get; set; } // İzin türü 
    public Guid? ManagerApprovalId { get; set; } 
    public string? Reason { get; set; } // İzin nedeni 
    public string? RejectionReason { get; set; } // Red nedeni 
    
    public virtual User? User { get; set; } // İzin sahibi kullanıcı
    public virtual User? ManagerApproval { get; set; } // Onaylayan/Reddeden yönetici
    public virtual ICollection<Notifications>? Notifications { get; set; } // İzinle ilgili bildirimler
}