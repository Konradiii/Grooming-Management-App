using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.EarningDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.EarningServ;

public class EarningsService(GroomingDbContext ctx) : IEarningsReaderService
{
    public async Task<GetEarningForPeriodDto> GetEarningsForPeriodAsync(int salonId, int? groomerId, DateTime dateFrom, DateTime dateTo, CancellationToken ct)
    {
        var earnings = await ctx.Visits
            .Where(v => v.SalonId == salonId)
            .Where(v => groomerId == null || v.GroomerId == groomerId)
            .Where(c => c.Status == StatusEnum.Completed)
            .Where(v => v.Date >= dateFrom && v.Date <= dateTo)
            .SumAsync(v => v.FinalPrice ?? v.ProposedPrice, ct);
        
        return new GetEarningForPeriodDto
        {
            Earnings = earnings,
            DateFrom = dateFrom,
            DateTo = dateTo
        };


    }

    public async Task<List<GetEarningsByGroomerDto>> GetEarningsByGroomerAsync(int salonId, DateTime dateFrom, DateTime dateTo, CancellationToken ct)
    {
        
        var earningsByGroomer = await ctx.Visits
            .Where(v => v.SalonId == salonId)
            .Where(v => v.Date >= dateFrom && v.Date <= dateTo)
            .Where(v => v.Status == StatusEnum.Completed)
            .GroupBy(v => v.GroomerId)
            .Select(g=> new GetEarningsByGroomerDto
            {
                GroomerId = g.Key,
                GroomerFullName = g.First().Groomer.FirstName + " " + g.First().Groomer.LastName,
                Earnings = g.Sum(v => v.FinalPrice ?? v.ProposedPrice),
            })
            .ToListAsync(ct);

        return earningsByGroomer;


    }

    public async Task<List<GetEarningsByDayDto>> GetEarningsByDayAsync(int salonId, DateTime dateFrom, DateTime dateTo, CancellationToken ct)
    {

        var earningsByDays = await ctx.Visits
            .Where(e => salonId == e.SalonId)
            .Where(v => v.Date >= dateFrom && v.Date <= dateTo)
            .Where(v => v.Status == StatusEnum.Completed)
            .GroupBy(v => v.Date.Date)
            .Select(g => new GetEarningsByDayDto
            {
                Day = g.Key,
                Earnings = g.Sum(v => v.FinalPrice ?? v.ProposedPrice),
            }).ToListAsync(ct);
        
        return earningsByDays;

    }
}