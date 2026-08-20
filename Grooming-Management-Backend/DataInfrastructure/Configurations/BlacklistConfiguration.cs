using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class BlacklistConfiguration : IEntityTypeConfiguration<Blacklist>
{
    public void Configure(EntityTypeBuilder<Blacklist> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .IsRequired();
        
        builder.Property(b => b.CreatedAt)
            .IsRequired();
        
        builder.Property(b=>b.SalonId)
            .IsRequired();
        
        builder.Property(b=> b.Reason)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.HasOne(b=>b.Salon)
            .WithMany(b=>b.Blacklists)
            .HasForeignKey(b=>b.SalonId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(b=>b.DogOwner)
            .WithMany(b=>b.Blacklists)
            .HasForeignKey(b=>b.DogOwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(b=>b.Dog)
            .WithMany(b=>b.Blacklists)
            .HasForeignKey(b=>b.DogId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
    
}