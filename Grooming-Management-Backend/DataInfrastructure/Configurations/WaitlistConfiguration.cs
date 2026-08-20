using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class WaitlistConfiguration : IEntityTypeConfiguration<Waitlist>
{
    public void Configure(EntityTypeBuilder<Waitlist> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.Property(e=>e.CreatedAt)
            .IsRequired();
        
        builder.ToTable(t => t.HasCheckConstraint("CK_Waitlist_Priority_Range", "[Priority] >= 1 AND [Priority] <= 3"));
        
        builder.HasOne(b => b.Salon)
            .WithMany(b => b.Waitlists)
            .HasForeignKey(b => b.SalonId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.HasOne(b => b.DogOwner)
            .WithMany(b => b.Waitlists)
            .HasForeignKey(b => b.DogOwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.HasOne(b => b.Dog)
            .WithMany(b => b.Waitlists)
            .HasForeignKey(b => b.DogId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        
        
    }
    
}