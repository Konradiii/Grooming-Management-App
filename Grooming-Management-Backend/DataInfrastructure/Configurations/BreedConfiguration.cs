using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class BreedConfiguration : IEntityTypeConfiguration<Breed>
{
    public void Configure(EntityTypeBuilder<Breed> builder)
    {
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(50);
        
    }
}