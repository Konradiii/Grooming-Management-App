using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date)
            .IsRequired();
        builder.Property(x => x.StartTime)
            .IsRequired();
        builder.Property(x => x.EndTime)
            .IsRequired();
        builder.Property(x => x.IsAvailable)
            .IsRequired();
      
        builder.HasOne(x => x.Salon)
            .WithMany(x=>x.TimeSlots)
            .HasForeignKey(x => x.SalonId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
            
        builder.HasOne(x => x.Groomer)
            .WithMany(x=>x.TimeSlots)
            .HasForeignKey(x => x.GroomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(x => x.Visit)
            .WithMany(x=>x.TimeSlots)
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

    }
    
}