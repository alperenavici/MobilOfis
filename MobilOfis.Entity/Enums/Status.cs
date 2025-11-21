namespace MobilOfis.Entity.Enums;

public enum Status
{
    Pending,           // Başvuru yapıldı, Manager onayı bekliyor
    ManagerApproved,   // Manager onayladı, HR onayı bekliyor
    Approved,          // HR onayladı, kesinleşti
    Rejected,          // Reddedildi (Manager veya HR tarafından)
}