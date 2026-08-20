using Grooming_Management_App.DTOs.GroomerTimeOffDTO;

namespace Grooming_Management_App.Services.GroomerTimeOffServ;

public interface IGroomerTimeOffWriterService
{
    Task<int> CreateGroomerTimeOffAsync(int salonId, CreateGroomerTimeOffDto dto, CancellationToken ct);
    
    Task DeleteGroomerTimeOffAsync(int salonId, int timeOffId, CancellationToken ct);
}