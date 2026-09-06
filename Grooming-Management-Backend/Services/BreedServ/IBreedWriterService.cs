
using Grooming_Management_App.DTOs.Breed;

namespace Grooming_Management_App.Services.Breed;

public interface IBreedWriterService
{
    Task<int> CreateBreedAsync(int salonId, CreateBreedDto dto, CancellationToken ct);
}