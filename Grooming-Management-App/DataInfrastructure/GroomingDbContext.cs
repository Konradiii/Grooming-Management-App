using Grooming_Management_App.Models;
using Grooming_Management_App.Services.CurrentUserServ;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.DataInfrastructure;

public class GroomingDbContext : DbContext
{
    private readonly ICurrentUserService _currentUser;

    public GroomingDbContext(DbContextOptions<GroomingDbContext> options, ICurrentUserService currentUser) : base(options)
    {
        _currentUser = currentUser;
        
    }
    
    public DbSet<Breed> Breeds { get; set; }
    public DbSet<Salon> Salons { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Groomer> Groomers { get; set; }
    public DbSet<DogOwner> DogOwners { get; set; }
    public DbSet<Dog> Dogs { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<ServiceBreed> ServiceBreeds { get; set; }
    public DbSet<Visit> Visits { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Blacklist> Blacklists { get; set; }
    public DbSet<Waitlist> Waitlists { get; set; } 
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<GroomerSchedule> GroomerSchedules { get; set; }
    public DbSet<GroomerTimeOff> GroomerTimeOffs { get; set; }


    

    



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GroomingDbContext).Assembly);

        modelBuilder.Entity<Dog>().HasQueryFilter(e=> e.SalonId == _currentUser.SalonId);
        modelBuilder.Entity<Groomer>().HasQueryFilter(e=> e.SalonId == _currentUser.SalonId);
        modelBuilder.Entity<DogOwner>().HasQueryFilter(e=> e.SalonId == _currentUser.SalonId);
        modelBuilder.Entity<Service>().HasQueryFilter(e=> e.SalonId == _currentUser.SalonId);
        modelBuilder.Entity<ServiceBreed>().HasQueryFilter(e=> e.SalonId == _currentUser.SalonId);
        modelBuilder.Entity<Visit>().HasQueryFilter(e=> e.SalonId == _currentUser.SalonId);
        modelBuilder.Entity<Blacklist>().HasQueryFilter(e=> e.SalonId == _currentUser.SalonId);
        modelBuilder.Entity<Waitlist>().HasQueryFilter(e=> e.SalonId == _currentUser.SalonId);
        modelBuilder.Entity<GroomerSchedule>().HasQueryFilter(e=> e.SalonId == _currentUser.SalonId);
        modelBuilder.Entity<GroomerTimeOff>().HasQueryFilter(e=> e.SalonId == _currentUser.SalonId);



        
    }
    
}