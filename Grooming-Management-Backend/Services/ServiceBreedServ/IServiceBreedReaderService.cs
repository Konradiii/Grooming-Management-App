using Grooming_Management_App.DTOs.ServiceBreedDTO;
using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Services.ServiceBreedServ;

public interface IServiceBreedReaderService
{
    Task<List<GetServiceBreedDto>> GetAllServiceBreedsAsync(int salonId, ActiveStatusEnum? status, int? breedId, CancellationToken ct);
    Task<GetServiceBreedDto> GetServiceBreedAsync(int salonId, int serviceBreedId, CancellationToken ct);
}