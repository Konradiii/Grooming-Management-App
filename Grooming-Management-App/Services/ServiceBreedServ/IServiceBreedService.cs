using Grooming_Management_App.DTOs.ServiceBreedDTO;
using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Services.ServiceBreedServ;

public interface IServiceBreedService
{
    Task ActivateServiceBreedAsync(int salonId, int serviceBreedId, CancellationToken ct);
    Task DeactivateServiceBreedAsync(int salonId, int serviceBreedId, CancellationToken ct);
    
    Task<List<GetServiceBreedDto>> GetAllServiceBreedsAsync(int salonId, ActiveStatusEnum? status, int? breedId, CancellationToken ct);
    Task<GetServiceBreedDto> GetServiceBreedAsync(int salonId, int serviceBreedId, CancellationToken ct);

    Task<int> AddServiceBreedAsync(int salonId, CreateServiceBreedDto dto, CancellationToken ct);
    Task UpdateServiceBreedAsync(int salonId, int serviceBreedId, UpdateServiceBreedDto dto, CancellationToken ct);
}