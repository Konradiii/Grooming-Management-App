using Grooming_Management_App.DTOs.GroomerDTO;

namespace Grooming_Management_App.Services.GroomerServ;

public interface IGroomerReaderService
{
    Task<GetGroomerDto> GetGroomerAsync(int id, int salonId, CancellationToken ct);
    
    Task<List<GetGroomerDto>> GetAllGroomersAsync(int salonId, CancellationToken ct);
    
    Task<List<GetGroomerBasicDto>> GetAllGroomersBasicAsync(int salonId, CancellationToken ct);
    
    Task<GetGroomerBasicDto?> GetCurrentGroomerAsync(int salonId, CancellationToken ct);
}