using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.SubscriptionDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.SubscriptionServ;

public class SubscriptionService(GroomingDbContext ctx) : ISubscriptionService
{
    private const int GracePeriodDays = 7;
    public const int MonthlySmsPackage = 100;
    
    public async Task<DateOnly> RegisterPaymentAsync(int salonId, RegisterPaymentDto dto, CancellationToken ct)
    {
        
        var salon = await ctx.Salons.Where(s => s.Id == salonId).FirstOrDefaultAsync(ct);
        if (salon == null)
        {
            throw new NotFoundException(ErrorCodes.SalonNotFound);
        }
        
        var alreadyProcessed = await ctx.Payments.AnyAsync(p=> p.ProviderId == dto.ProviderId, ct);

        if (alreadyProcessed)
        {
            throw new ConflictException(ErrorCodes.PaymentAlreadyProcessed);
        }
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var periodStart = salon.SubscriptionValidUntil > today
            ? salon.SubscriptionValidUntil.Value
            : today;

        var periodEnd = periodStart.AddMonths(1);
        
        salon.SubscriptionValidUntil = periodEnd;
        salon.SubscriptionStatus = SubscriptionStatusEnum.Active;

        var payment = new Payment
        {
            Amount = dto.Amount,
            Currency = dto.Currency,
            PaymentDate = DateTime.UtcNow,
            ProviderId = dto.ProviderId,
            Status = PaymentStatusEnum.Succeeded,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            InvoiceUrl = dto.InvoiceUrl,
            SalonId = salonId
        };

        ctx.Payments.Add(payment);
        await ctx.SaveChangesAsync(ct);

        return periodEnd;

    }
    
    public async Task MarkPaymentFailedAsync(int salonId, RegisterPaymentDto dto, CancellationToken ct)
    {
            var salon = await ctx.Salons.Where(s => s.Id == salonId).FirstOrDefaultAsync(ct);
            if (salon == null)
            {
                throw new NotFoundException(ErrorCodes.SalonNotFound);
            }

            var alreadyProcessed = await ctx.Payments.AnyAsync(p => p.ProviderId == dto.ProviderId, ct);
            if (alreadyProcessed)
            {
                throw new ConflictException(ErrorCodes.PaymentAlreadyProcessed);
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var periodStart = salon.SubscriptionValidUntil > today
                ? salon.SubscriptionValidUntil.Value
                : today;

            var periodEnd = periodStart.AddMonths(1);

            var payment = new Payment
            {
                Amount = dto.Amount,
                Currency = dto.Currency,
                PaymentDate = DateTime.UtcNow,
                ProviderId = dto.ProviderId,
                Status = PaymentStatusEnum.Failed,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                InvoiceUrl = dto.InvoiceUrl,
                SalonId = salonId
            };

            ctx.Payments.Add(payment);

            if (salon.SubscriptionValidUntil == null || salon.SubscriptionValidUntil < today)
            {
                salon.SubscriptionStatus = SubscriptionStatusEnum.PastDue;
            }

            await ctx.SaveChangesAsync(ct);
    }
    
    public async Task<int> SuspendExpiredSubscriptionsAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var graceCutoff = today.AddDays(-GracePeriodDays);

        var expired = await ctx.Salons
            .Where(s => s.SubscriptionStatus == SubscriptionStatusEnum.PastDue)
            .Where(s => s.SubscriptionValidUntil != null && s.SubscriptionValidUntil < graceCutoff)
            .ToListAsync(ct);

        foreach (var salon in expired)
        {
            salon.SubscriptionStatus = SubscriptionStatusEnum.Suspended;
        }

        await ctx.SaveChangesAsync(ct);
        return expired.Count;
    }
    
    public async Task<int> MarkExpiredSubscriptionsAsPastDueAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var expired = await ctx.Salons
            .Where(s => s.SubscriptionStatus == SubscriptionStatusEnum.Trial
                        || s.SubscriptionStatus == SubscriptionStatusEnum.Active)
            .Where(s => s.SubscriptionValidUntil != null && s.SubscriptionValidUntil < today)
            .ToListAsync(ct);

        foreach (var salon in expired)
        {
            salon.SubscriptionStatus = SubscriptionStatusEnum.PastDue;
        }

        await ctx.SaveChangesAsync(ct);
        return expired.Count;
    }
    
    public async Task LinkProviderIdsAsync(int salonId, string? customerId, string? subscriptionId, CancellationToken ct)
    {
        var salon = await ctx.Salons.FirstOrDefaultAsync(s => s.Id == salonId, ct);

        if (salon == null)
            throw new NotFoundException(ErrorCodes.SalonNotFound);

        salon.ProviderCustomerId = customerId;
        salon.ProviderSubscriptionId = subscriptionId;

        await ctx.SaveChangesAsync(ct);
    }

    
    public async Task<int?> GetSalonIdByCustomerIdAsync(string? customerId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            return null;

        return await ctx.Salons
            .Where(s => s.ProviderCustomerId == customerId)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<GetSubscriptionDto> GetSubscriptionAsync(int salonId, CancellationToken ct)
    {
        var salon = await ctx.Salons
            .Where(s => s.Id == salonId)
            .Select(s => new
            {
                s.SubscriptionStatus,
                s.SubscriptionValidUntil,
                s.ProviderSubscriptionId,
                s.SubscriptionCancelAtPeriodEnd
            })
            .FirstOrDefaultAsync(ct);

        if (salon == null)
            throw new NotFoundException(ErrorCodes.SalonNotFound);

        var payments = await ctx.Payments
            .Where(p => p.SalonId == salonId)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new GetPaymentDto
            {
                Amount = p.Amount,
                Currency = p.Currency,
                PaymentDate = p.PaymentDate,
                Status = p.Status,
                PeriodStart = p.PeriodStart,
                PeriodEnd = p.PeriodEnd,
                InvoiceUrl = p.InvoiceUrl
            })
            .ToListAsync(ct);

        return new GetSubscriptionDto
        {
            Status = salon.SubscriptionStatus,
            ValidUntil = salon.SubscriptionValidUntil,
            HasActiveSubscription = salon.ProviderSubscriptionId != null,
            CancelAtPeriodEnd = salon.SubscriptionCancelAtPeriodEnd,
            Payments = payments
        };
    }
    
    public async Task<string?> GetProviderCustomerIdAsync(int salonId, CancellationToken ct)
    {
        return await ctx.Salons
            .Where(s => s.Id == salonId)
            .Select(s => s.ProviderCustomerId)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task ClearSubscriptionAsync(string? customerId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            return;

        var salon = await ctx.Salons
            .FirstOrDefaultAsync(s => s.ProviderCustomerId == customerId, ct);

        if (salon == null)
            return;

        salon.ProviderSubscriptionId = null;

        await ctx.SaveChangesAsync(ct);
    }
    
    public async Task SetCancelAtPeriodEndAsync(string? customerId, bool value, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            return;

        var salon = await ctx.Salons
            .FirstOrDefaultAsync(s => s.ProviderCustomerId == customerId, ct);

        if (salon == null)
            return;

        salon.SubscriptionCancelAtPeriodEnd = value;
        await ctx.SaveChangesAsync(ct);
    }
    
}