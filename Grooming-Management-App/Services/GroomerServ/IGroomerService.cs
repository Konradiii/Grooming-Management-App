using Grooming_Management_App.DTOs.GroomerDTO;

namespace Grooming_Management_App.Services.GroomerServ;

public interface IGroomerService
{
    
    Task DeactivateGroomerAsync(int id, int salonId, CancellationToken ct);
    
    Task ActivateGroomerAsync(int id, int salonId, CancellationToken ct);
    
    Task<GetGroomerDto> GetGroomerAsync(int id, int salonId, CancellationToken ct);
    
    Task<List<GetGroomerDto>> GetAllGroomersAsync(int salonId, CancellationToken ct);
    
    Task EditGroomerAsync(EditGroomerDto dto, int id, int salonId,  CancellationToken ct);
    
    Task CreateGroomerAsync(CreateGroomerDto dto, int salonId, CancellationToken ct);
    
}