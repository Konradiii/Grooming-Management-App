using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15);
        
        builder.Property(x => x.Type)
            .IsRequired();
        
        builder.Property(x => x.Status)
            .IsRequired();
        
        builder.Property(x => x.MessageText)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(x => x.ScheduledTime)
            .IsRequired();
        
        builder.Property(x => x.SentAt)
            .IsRequired(false);
        
        builder.Property(x => x.AttemptCount)
            .IsRequired();
        
        builder.Property(x => x.ErrorMessage)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.HasOne(x => x.Salon)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.SalonId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(x => x.Visit)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(x => x.DogOwner)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.DogOwnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
    
}