using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class DogConfiguration :IEntityTypeConfiguration<Dog>
{
    public void Configure(EntityTypeBuilder<Dog> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.AgeInMonths)
            .IsRequired(false);
        
        builder.Property(p => p.Notes)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.HasOne(p => p.Salon)
            .WithMany(p => p.Dogs)
            .HasForeignKey(p => p.SalonId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.Breed)
            .WithMany(p => p.Dogs)
            .HasForeignKey(p => p.BreedId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.DogOwner)
            .WithMany(p => p.Dogs)
            .HasForeignKey(p => p.DogOwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    
}