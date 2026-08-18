using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class SalonConfiguration : IEntityTypeConfiguration<Salon>
{
    public void Configure(EntityTypeBuilder<Salon> builder)
    {
     
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(s => s.Street)
            .IsRequired(false)
            .HasMaxLength(200);
        
        builder.Property(s => s.BuildingNumber)
            .IsRequired(false)
            .HasMaxLength(20);

        builder.Property(s => s.ApartmentNumber)
            .IsRequired(false)
            .HasMaxLength(20);

        builder.Property(s => s.PostalCode)
            .IsRequired(false)
            .HasMaxLength(10);

        builder.Property(s => s.City)
            .IsRequired(false)
            .HasMaxLength(100);
        
        builder.Property(s => s.MinBookingHoursAhead)
            .IsRequired()
            .HasDefaultValue(24);

        builder.Property(s => s.MaxBookingDaysAhead)
            .IsRequired()
            .HasDefaultValue(90);

        builder.Property(s => s.SubscriptionStatus)
            .IsRequired();
        
        builder.Property(s => s.SubscriptionValidUntil)
            .IsRequired(false);

    }
    
}