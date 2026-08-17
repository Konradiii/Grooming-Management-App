using Grooming_Management_App.DTOs.DogDTO;

namespace Grooming_Management_App.Services.DogServ;

public interface IDogReaderService
{
    Task<List<GetDogDto>> GetAllDogsAsync(int salonId, int? dogOwnerId, int? breedId, CancellationToken ct);
    
    Task<GetDogDetailsDto> GetDogDetailsAsync(int salonId, int dogId, CancellationToken ct);

}