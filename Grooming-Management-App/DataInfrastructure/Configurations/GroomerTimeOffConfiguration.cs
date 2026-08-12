using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class GroomerTimeOffConfiguration : IEntityTypeConfiguration<GroomerTimeOff>
{
    public void Configure(EntityTypeBuilder<GroomerTimeOff> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.StartDate)
            .IsRequired();

        builder.Property(t => t.EndDate)
            .IsRequired();

        builder.Property(t => t.StartTime)
            .IsRequired();

        builder.Property(t => t.EndTime)
            .IsRequired();

        builder.Property(t => t.Reason)
            .IsRequired(false)
            .HasMaxLength(300);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasOne(t => t.Salon)
            .WithMany(s => s.GroomerTimeOffs)
            .HasForeignKey(t => t.SalonId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(t => t.Groomer)
            .WithMany(g => g.GroomerTimeOffs)
            .HasForeignKey(t => t.GroomerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}