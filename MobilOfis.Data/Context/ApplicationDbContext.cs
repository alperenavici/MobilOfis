using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MobilOfis.Entity;
namespace MobilOfis.Data.Context;

public class ApplicationDbContext:DbContext
{
   public DbSet<User> Users { get; set; }
   public DbSet<Departments> Departments { get; set; }
   public DbSet<Leaves> Leaves { get; set; }
   public DbSet<Notifications> Notifications { get; set; }
   public DbSet<Events> Events { get; set; }
   public DbSet<Participants> Participants { get; set; }

   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
   }
   
   public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
   {
   }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);
      
      // Departments Configuration
      modelBuilder.Entity<Departments>()
         .HasKey(d => d.DepartmentId);
      
      modelBuilder.Entity<Departments>()
         .Property(d => d.DepartmentName)
         .IsRequired()
         .HasMaxLength(100);
      
      modelBuilder.Entity<User>()
         .HasKey(u => u.UserId);
      
      modelBuilder.Entity<User>()
         .HasIndex(u => u.Email)
         .IsUnique();
      
      modelBuilder.Entity<User>()
         .Property(u => u.FirstName)
         .IsRequired()
         .HasMaxLength(100);
      
      modelBuilder.Entity<User>()
         .Property(u => u.LastName)
         .IsRequired()
         .HasMaxLength(100);
      
      modelBuilder.Entity<User>()
         .Property(u => u.Email)
         .IsRequired()
         .HasMaxLength(255);
      
      modelBuilder.Entity<User>()
         .Property(u => u.PasswordHash)
         .IsRequired();
      
      modelBuilder.Entity<User>()
         .Property(u => u.IsActive)
         .HasDefaultValue(true);
      
      modelBuilder.Entity<User>()
         .Property(u => u.CreatedDate)
         .HasDefaultValueSql("NOW()");
      
      // User - Department (Many-to-One)
      modelBuilder.Entity<User>()
         .HasOne(u => u.Department)
         .WithMany(d => d.Users)
         .HasForeignKey(u => u.DepartmentId)
         .OnDelete(DeleteBehavior.SetNull);
      
      // User - Manager/Subordinates (Self-Referencing)
      modelBuilder.Entity<User>()
         .HasOne(u => u.Manager)
         .WithMany(u => u.Subordinates)
         .HasForeignKey(u => u.ManagerId)
         .OnDelete(DeleteBehavior.Restrict);
      
      // Leaves Configuration
      modelBuilder.Entity<Leaves>()
         .HasKey(l => l.LeavesId);
      
      // Leaves - User (Many-to-One)
      modelBuilder.Entity<Leaves>()
         .HasOne(l => l.User)
         .WithMany(u => u.Leaves)
         .HasForeignKey(l => l.UserId)
         .OnDelete(DeleteBehavior.Cascade);
      
      // Leaves - ManagerApproval (Many-to-One)
      modelBuilder.Entity<Leaves>()
         .HasOne(l => l.ManagerApproval)
         .WithMany(u => u.ApprovedLeaves)
         .HasForeignKey(l => l.ManagerApprovalId)
         .OnDelete(DeleteBehavior.Restrict);
      
      // Notifications Configuration
      modelBuilder.Entity<Notifications>()
         .HasKey(n => n.NotificationId);
      
      // Notifications - User (Many-to-One)
      modelBuilder.Entity<Notifications>()
         .HasOne(n => n.RecipientUser)
         .WithMany(u => u.Notifications)
         .HasForeignKey(n => n.RecipientUserId)
         .OnDelete(DeleteBehavior.Cascade);
      
      // Notifications - Leaves (Many-to-One)
      modelBuilder.Entity<Notifications>()
         .HasOne(n => n.Leaves)
         .WithMany(l => l.Notifications)
         .HasForeignKey(n => n.LeavesId)
         .OnDelete(DeleteBehavior.SetNull);
      
      // Events Configuration
      modelBuilder.Entity<Events>()
         .HasKey(e => e.EventId);
      
      // Events - CreatedByUser (Many-to-One)
      modelBuilder.Entity<Events>()
         .HasOne(e => e.CreatedByUser)
         .WithMany(u => u.CreatedEvents)
         .HasForeignKey(e => e.CreatedByUserId)
         .OnDelete(DeleteBehavior.Restrict);
      
      // Participants - Composite Key (Many-to-Many Junction Table)
      modelBuilder.Entity<Participants>()
         .HasKey(p => new { p.EventId, p.UserId });
      
      modelBuilder.Entity<Participants>()
         .HasOne(p => p.Event)
         .WithMany(e => e.Participants)
         .HasForeignKey(p => p.EventId)
         .OnDelete(DeleteBehavior.Cascade);
      
      modelBuilder.Entity<Participants>()
         .HasOne(p => p.User)
         .WithMany(u => u.Participants)
         .HasForeignKey(p => p.UserId)
         .OnDelete(DeleteBehavior.Cascade);
   }
}