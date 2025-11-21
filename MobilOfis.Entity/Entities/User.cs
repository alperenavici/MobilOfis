namespace MobilOfis.Entity;

public class User
{
    public Guid UserId { get; set; } 
    
    // Kişisel Bilgiler
    public string FirstName { get; set; } // Adı
    public string LastName { get; set; } // Soyadı
    public string Email { get; set; } // E-posta adresi (unique)
    public string PhoneNumber { get; set; } // Telefon numarası
    public string PasswordHash { get; set; } // Şifrelenmiş parola
    public string? ProfilePictureUrl { get; set; } // Profil fotoğrafı URL'si
    
    // İş Bilgileri
    public string? JobTitle { get; set; } // İş ünvanı 
    public DateTime? HireDate { get; set; } // İşe başlama tarihi
    public decimal? Salary { get; set; } // Maaş bilgisi
    
    public Guid? DepartmentId { get; set; } 
    public Guid? ManagerId { get; set; }
    
    // Hesap Durumu
    public bool IsActive { get; set; } // Hesap aktif mi? 
    public string? Role { get; set; } // Kullanıcı rolü 
    
    // Zaman Damgaları
    public DateTime CreatedDate { get; set; } // Hesap oluşturulma tarihi
    public DateTime? UpdatedDate { get; set; } // Son güncellenme tarihi
    public DateTime? LastLoginDate { get; set; } // Son giriş tarihi
    
    // Adres Bilgileri
    public string? Address { get; set; } // Açık adres
    public string? City { get; set; } // Şehir
    public string? Country { get; set; } // Ülke
    public string? PostalCode { get; set; } // Posta kodu
    
    // Ek Bilgiler
    public DateTime? DateOfBirth { get; set; } // Doğum tarihi
    public string? EmergencyContactName { get; set; } // Acil durum kişisi adı
    public string? EmergencyContactPhone { get; set; } // Acil durum kişisi telefonu
    
    public virtual Departments? Department { get; set; } // Kullanıcının departmanı
    public virtual User? Manager { get; set; } // Kullanıcının yöneticisi
    public virtual ICollection<User>? Subordinates { get; set; } // Kullanıcının astları
    
    public virtual ICollection<Leaves>? Leaves { get; set; } // Kullanıcının izin talepleri
    public virtual ICollection<Leaves>? ApprovedLeaves { get; set; } // Yönetici olarak onayladığı izinler
    public virtual ICollection<Notifications>? Notifications { get; set; } // Kullanıcının bildirimleri
    public virtual ICollection<Events>? CreatedEvents { get; set; } // Kullanıcının oluşturduğu etkinlikler
    public virtual ICollection<Participants>? Participants { get; set; } // Kullanıcının katıldığı etkinlikler
}