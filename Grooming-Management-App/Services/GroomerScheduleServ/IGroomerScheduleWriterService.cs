using Grooming_Management_App.DTOs.GroomerScheduleDTO;

namespace Grooming_Management_App.Services.GroomerScheduleServ;

public interface IGroomerScheduleWriterService
{
    Task<int> CreateGroomerScheduleAsync(int salonId, CreateGroomerScheduleDto dto, CancellationToken ct);
    
    Task DeleteGroomerScheduleAsync(int salonId, int groomerScheduleId, CancellationToken ct);

}