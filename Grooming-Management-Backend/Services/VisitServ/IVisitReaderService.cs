using Grooming_Management_App.DTOs.VisitDTO;

namespace Grooming_Management_App.Services.VisitServ;

public interface IVisitReaderService
{
    Task<List<GetAllVisitsDto>> GetAllVisitsAsync(int salonId, VisitFilterDto filter, CancellationToken ct);
    
    Task<GetVisitDetailsDto> GetVisitAsync(int salonId, int visitId, CancellationToken ct);
}