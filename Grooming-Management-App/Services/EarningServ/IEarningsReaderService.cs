using Grooming_Management_App.DTOs.EarningDTO;

namespace Grooming_Management_App.Services.EarningServ;

public interface IEarningsReaderService
{

    Task<GetEarningForPeriodDto> GetEarningsForPeriodAsync(int salonId, int? groomerId, DateTime dateFrom, DateTime dateTo, CancellationToken ct);

    Task<List<GetEarningsByGroomerDto>> GetEarningsByGroomerAsync(int salonId, DateTime dateFrom, DateTime dateTo, CancellationToken ct);

    Task<List<GetEarningsByDayDto>> GetEarningsByDayAsync(int salonId, DateTime dateFrom, DateTime dateTo, CancellationToken ct);
}