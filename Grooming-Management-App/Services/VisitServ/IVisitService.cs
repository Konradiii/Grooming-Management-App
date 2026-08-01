using Grooming_Management_App.DTOs.VisitDTO;
using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Services.VisitServ;

public interface IVisitService
{
    
    Task<List<GetAllVisitsDto>> GetAllVisitsAsync(int salonId, VisitFilterDto filter, CancellationToken ct);
    
    Task<GetVisitDetailsDto> GetVisitAsync(int salonId, int visitId, CancellationToken ct);
    
    Task EditVisitAsync(int salonId, int visitId, EditVisitDto dto, CancellationToken ct);
    
    Task AddVisitAsync(int salonId, AddVisitDto dto, CancellationToken ct);
    
    Task ChangeVisitStatusAsync(int salonId, int visitId, StatusEnum status, CancellationToken ct);
    
    Task UpdateFinalPriceAsync(int salonId, int visitId, decimal finalPrice, CancellationToken ct);
    
    
    
}