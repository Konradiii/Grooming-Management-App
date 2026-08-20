using Grooming_Management_App.DTOs.DogOwnerDTO;

namespace Grooming_Management_App.Services.DogOwner;

public interface IDogOwnerReaderService
{
    Task<GetDogOwnerDto> GetDogOwnerAsync(int id, int salonId, CancellationToken ct);
    
    Task<List<GetDogOwnerDto>> GetAllDogOwnersAsync(int salonId, CancellationToken ct);
}