using Grooming_Management_App.DTOs.BlacklistDto;

namespace Grooming_Management_App.Services.BlacklistServ;

public interface IBlacklistService
{
    Task<List<GetAllBlacklistDto>> GetAllClientsOfBlacklistAsync(int salonId, string? phoneNumber, CancellationToken ct);
    
    Task<GetDetailsBlackListDto> GetDetailsBlackListAsync(int salonId, int id, CancellationToken ct);
    
    Task<int> AddToBlacklistByDogOwnerAsync(int salonId, CreateBlacklistByDogOwnerDto dto, CancellationToken ct);
    
    Task<int> AddToBlacklistByDogAsync(int salonId, CreateBlacklistByDogDto dto, CancellationToken ct);
    
    Task DeleteRecordFromBlacklistAsync(int salonId, int id, CancellationToken ct);
    
    Task<bool> IsBlockedAsync(int salonId, int dogOwnerId, int? dogId, CancellationToken ct);
}