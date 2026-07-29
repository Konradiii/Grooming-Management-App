using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class ServiceBreedConfiguraiton : IEntityTypeConfiguration<ServiceBreed>
{
    public void Configure(EntityTypeBuilder<ServiceBreed> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(e=>e.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Duration)
            .IsRequired();
        
        builder.HasOne(d=>d.Salon)
            .WithMany(s => s.ServiceBreeds)
            .HasForeignKey(d => d.SalonId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(d => d.Breed)
            .WithMany(s => s.ServiceBreeds)
            .HasForeignKey(d => d.BreedId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e=> e.Service)
            .WithMany(e=>e.ServiceBreeds)
            .HasForeignKey(e=>e.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);


    }
    
}