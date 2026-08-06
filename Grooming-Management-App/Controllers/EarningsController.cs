using Grooming_Management_App.DTOs.EarningDTO;
using Grooming_Management_App.Services.CurrentUserServ;
using Grooming_Management_App.Services.EarningServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EarningsController(IEarningsService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("GetByPeriod")]
    [Authorize(Roles = "Owner")]
    public async Task<GetEarningForPeriodDto> GetEarningForPeriod(int? groomerId, DateTime dateFrom, DateTime dateTo, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var res = await service.GetEarningsForPeriodAsync(salonId, groomerId, dateFrom, dateTo, ct);
        return res;
    }
    [HttpGet("GetByGroomer")]
    [Authorize(Roles = "Owner")]
    public async Task<List<GetEarningsByGroomerDto>> GetEarningByGroomer(DateTime dateFrom, DateTime dateTo, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var res2 = await service.GetEarningsByGroomerAsync(salonId, dateFrom, dateTo, ct);
        return res2;
    }
    [HttpGet("GetByDay")]
    [Authorize(Roles = "Owner")]
    public async Task<List<GetEarningsByDayDto>> GetEarningsByDay(DateTime dateFrom, DateTime dateTo, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        var res2 = await service.GetEarningsByDayAsync(salonId, dateFrom, dateTo, ct);
        return res2;
    }

    
}