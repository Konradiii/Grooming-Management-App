using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.DataInfrastructure;

public class GroomingDbContext : DbContext
{
    public GroomingDbContext(DbContextOptions<GroomingDbContext> options) : base(options){}
    
    public DbSet<Breed> Breeds { get; set; }
    public DbSet<Salon> Salons { get; set; }
    public DbSet<User> Users { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GroomingDbContext).Assembly);
    }
    
}