using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Grooming_Management_App.DataInfrastructure.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(p => p.PaymentDate)
            .IsRequired();

        builder.Property(p => p.ProviderId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.ProviderId)
            .IsUnique();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.PeriodStart)
            .IsRequired();

        builder.Property(p => p.PeriodEnd)
            .IsRequired();

        builder.Property(p => p.InvoiceUrl)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.HasOne(p => p.Salon)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.SalonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}