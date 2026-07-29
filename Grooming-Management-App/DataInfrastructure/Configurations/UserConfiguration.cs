using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.Role)
            .IsRequired();
        builder.Property(e=>e.Status)
            .IsRequired();
        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);
        builder.Property(e=>e.RequiresPasswordChange)
            .IsRequired();

        builder.HasOne(e => e.Salon)
            .WithMany(e => e.Users)
            .HasForeignKey(e => e.SalonId)
            .OnDelete(DeleteBehavior.Restrict);

    }
    
}