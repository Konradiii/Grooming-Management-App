using Grooming_Management_App.DTOs.EarningDTO;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Extensions;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.EarningServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EarningsController(IEarningsReaderService readerService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("GetByPeriod")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Zwraca łączne zarobki za wybrany okres, opcjonalnie dla jednego pracownika")]
    public async Task<GetEarningForPeriodDto> GetEarningForPeriod(int? groomerId, DateTime dateFrom, DateTime dateTo, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        return await readerService.GetEarningsForPeriodAsync(
            salonId, groomerId, dateFrom.AsUtc(), dateTo.AsUtc(), ct);
    }

    [HttpGet("GetByGroomer")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Zwraca zarobki w podziale na poszczególnych pracowników")]
    public async Task<List<GetEarningsByGroomerDto>> GetEarningByGroomer(DateTime dateFrom, DateTime dateTo, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        return await readerService.GetEarningsByGroomerAsync(
            salonId, dateFrom.AsUtc(), dateTo.AsUtc(), ct);
    }

    [HttpGet("GetByDay")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Zwraca zarobki w podziale na poszczególne dni")]
    public async Task<List<GetEarningsByDayDto>> GetEarningsByDay(DateTime dateFrom, DateTime dateTo, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        return await readerService.GetEarningsByDayAsync(
            salonId, dateFrom.AsUtc(), dateTo.AsUtc(), ct);
    }

    [HttpGet("GetGroomerSettlements")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Zwraca rozliczenia pracowników za okres")]
    public async Task<List<GetGroomerSettlementDto>> GetGroomerSettlements(
        DateTime dateFrom, DateTime dateTo, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        return await readerService.GetGroomerSettlementsAsync(
            salonId, dateFrom.AsUtc(), dateTo.AsUtc(), ct);
    }
}