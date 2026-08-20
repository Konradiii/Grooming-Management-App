using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.EstimatedDuration)
            .IsRequired();

        builder.Property(x => x.ProposedPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.FinalPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasOne(x=>x.Salon)
            .WithMany(x=>x.Visits)
            .HasForeignKey(x=>x.SalonId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x=>x.Dog)
            .WithMany(x=>x.Visits)
            .HasForeignKey(x=>x.DogId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x=>x.DogOwner)
            .WithMany(x=>x.Visits)
            .HasForeignKey(x=>x.DogOwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x=> x.Groomer)
            .WithMany(x=>x.Visits)
            .HasForeignKey(x=>x.GroomerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x=> x.ServiceBreed)
            .WithMany(x=>x.Visits)
            .HasForeignKey(x=>x.ServiceBreedId)
            .OnDelete(DeleteBehavior.Restrict);
        
            
        
        
        
    }
    
}