using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class GroomerScheduleConfiguration : IEntityTypeConfiguration<GroomerSchedule>
{
    public void Configure(EntityTypeBuilder<GroomerSchedule> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .IsRequired();
            
        builder.Property(g => g.DayOfWeek)
            .IsRequired();

        builder.Property(g => g.StartTime)
            .IsRequired();

        builder.Property(g => g.EndTime)
            .IsRequired();
        

        builder.HasOne(g => g.Salon)
            .WithMany(x => x.GroomerSchedules)
            .HasForeignKey(x => x.SalonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.Groomer)
            .WithMany(x => x.GroomerSchedules)
            .HasForeignKey(x => x.GroomerId)
            .OnDelete(DeleteBehavior.Restrict);

    }
    
}