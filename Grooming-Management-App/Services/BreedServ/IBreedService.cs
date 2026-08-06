using Grooming_Management_App.DTOs.Breed;

namespace Grooming_Management_App.Services.Breed;

public interface IBreedService
{
    Task<GetBreedDto> GetBreedAsync(int Id, CancellationToken cancellationToken);
    Task<List<GetBreedDto>> GetAllBreedsAsync(CancellationToken ct);
}