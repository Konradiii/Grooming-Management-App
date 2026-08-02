using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.DataInfrastructure;

public class GroomingDbContext : DbContext
{
    public GroomingDbContext(DbContextOptions<GroomingDbContext> options) : base(options){}
    
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

    



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GroomingDbContext).Assembly);
    }
    
}