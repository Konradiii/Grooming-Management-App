using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasOne(s=>s.Salon)
            .WithMany(s=>s.Services)
            .HasForeignKey(s=>s.SalonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    
}