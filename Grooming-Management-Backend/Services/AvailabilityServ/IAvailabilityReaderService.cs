using Grooming_Management_App.DTOs.AvailabilityDTO;

namespace Grooming_Management_App.Services.AvailabilityServ;

public interface IAvailabilityReaderService
{
    
    Task<List<GetAvailabilityDto>> GetAvailabilitySlotsAsync(int salonId, DateOnly date, int serviceBreedId, int? groomerId, CancellationToken ct);
    
}