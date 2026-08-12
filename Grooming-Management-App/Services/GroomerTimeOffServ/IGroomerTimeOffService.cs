using Grooming_Management_App.DTOs.GroomerTimeOffDTO;

namespace Grooming_Management_App.Services.GroomerTimeOffServ;

public interface IGroomerTimeOffService
{
    Task<int> CreateGroomerTimeOffAsync(int salonId, CreateGroomerTimeOffDto dto, CancellationToken ct);
    
    Task<GetGroomerTimeOffDto> GetGroomerTimeOffAsync(int salonId, int timeOffId, CancellationToken ct);
    
    Task<List<GetGroomerTimeOffDto>> GetAllGroomerTimeOffsAsync(int salonId, int? groomerId, DateOnly? dateFrom, DateOnly? dateTo, CancellationToken ct);
    
    Task DeleteGroomerTimeOffAsync(int salonId, int timeOffId, CancellationToken ct);
}