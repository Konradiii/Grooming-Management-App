using Grooming_Management_App.DTOs.GroomerScheduleDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Models;

namespace Grooming_Management_App.Services.GroomerScheduleServ;

public interface IGroomerScheduleService
{
    Task<int> CreateGroomerScheduleAsync(int salonId, CreateGroomerScheduleDto dto, CancellationToken ct);
    
    Task<GetGroomerScheduleDto> GetGroomerScheduleAsync(int salonId, int groomerScheduleId, CancellationToken ct);
    
    Task<List<GetGroomerScheduleDto>> GetAllGroomerScheduleAsync(int salonId, int? groomerId, DayOfWeekEnum? day, CancellationToken ct);
    
    Task DeleteGroomerScheduleAsync(int salonId, int groomerScheduleId, CancellationToken ct);
}