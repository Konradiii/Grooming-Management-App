using Grooming_Management_App.DTOs.VisitDTO;
using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Services.VisitServ;

public interface IVisitWriterService
{
    Task EditVisitAsync(int salonId, int visitId, EditVisitDto dto, CancellationToken ct);
    
    Task<int> AddVisitAsync(int salonId, AddVisitDto dto, CancellationToken ct);
    
    Task ChangeVisitStatusAsync(int salonId, int visitId, StatusEnum status, CancellationToken ct);
    
    Task UpdateFinalPriceAsync(int salonId, int visitId, decimal finalPrice, CancellationToken ct);
    
    //Task<int> BookVisitByClientAsync(int salonId, int userId, AddVisitDto dto, CancellationToken ct);
}