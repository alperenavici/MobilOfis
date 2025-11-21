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
      modelBuilder.Entity<Participants>()
         .HasKey(p => new { p.EventId, p.UserId });  
      
      modelBuilder.Entity<Participants>()
         .HasOne(p => p.Event) 
         .WithMany(e => e.Participants) 
         .HasForeignKey(p => p.EventId);
   }
   
   
}