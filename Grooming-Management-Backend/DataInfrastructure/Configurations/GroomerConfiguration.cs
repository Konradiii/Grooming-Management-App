using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class GroomerConfiguration : IEntityTypeConfiguration<Groomer>
{
    public void Configure(EntityTypeBuilder<Groomer> builder)
    {
        builder.HasKey(g => g.Id);
        
        builder.HasOne(g => g.Salon)
            .WithMany(s => s.Groomers)
            .HasForeignKey(g => g.SalonId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(e=>e.SalonId)
            .IsRequired();
        
        builder.HasOne(g => g.User)
            .WithOne(u => u.Groomer)
            .HasForeignKey<Groomer>(g => g.UserId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Property(e=>e.FirstName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(e=> e.ActiveStatus)
            .IsRequired();
        builder.Property(g => g.SettlementRate)
            .HasPrecision(18, 2);
        
        builder.Property(g => g.CanSeeAllVisits).HasDefaultValue(true);
        builder.Property(g => g.CanCreateVisits).HasDefaultValue(true);
        

    }
    
}