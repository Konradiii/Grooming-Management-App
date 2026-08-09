using Grooming_Management_App.DTOs.WaitlistDTO;

namespace Grooming_Management_App.Services.WaitlistServ;

public interface IWaitlistService
{
    Task<int> AddToWaitlistAsync(int salonId, CreateWaitlistDto dto, CancellationToken ct);
    
    Task RemoveFromWaitlistAsync(int salonId, int id, CancellationToken ct);
    
    Task<List<GetWaitlistDto>> GetAllWaitlistAsync(int salonId, CancellationToken ct);

}