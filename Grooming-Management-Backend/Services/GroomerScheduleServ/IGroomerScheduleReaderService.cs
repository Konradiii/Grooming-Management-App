using Grooming_Management_App.DTOs.GroomerScheduleDTO;
using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Services.GroomerScheduleServ;

public interface IGroomerScheduleReaderService
{
    Task<GetGroomerScheduleDto> GetGroomerScheduleAsync(int salonId, int groomerScheduleId, CancellationToken ct);
    
    Task<List<GetGroomerScheduleDto>> GetAllGroomerScheduleAsync(int salonId, int? groomerId, DayOfWeekEnum? day, CancellationToken ct);
}