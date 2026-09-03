using Grooming_Management_App.DTOs.SalonDTO;

namespace Grooming_Management_App.Services.SalonServ;

public interface ISalonService
{
    Task<GetSalonDto> GetSalonAsync(int salonId, CancellationToken ct);
    Task UpdateSalonAsync(UpdateSalonDto dto, int salonId, CancellationToken ct);
    
    Task<GetSmsBalanceDto> GetSmsBalanceAsync(int salonId, CancellationToken ct);
    
}