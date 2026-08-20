using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class DogOwnerConfiguration : IEntityTypeConfiguration<DogOwner>
{
    public void Configure(EntityTypeBuilder<DogOwner> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e=>e.FirstName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e=>e.LastName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e=>e.Phone)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.HasOne(d => d.Salon)
            .WithMany(p => p.DogOwners)
            .HasForeignKey(d => d.SalonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.User)
            .WithOne(p => p.DogOwner)
            .HasForeignKey<DogOwner>(d => d.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
    
}