using Grooming_Management_App.DTOs.ServiceDTO;
using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Services.ServiceServ;

public interface IServiceService
{
    Task<List<GetServiceDto>> GetAllServicesAsync(int salonId, ActiveStatusEnum? status, CancellationToken ct);
    
    Task<GetServiceDto> GetServiceAsync(int salonId, int serviceId, CancellationToken ct);
    
    Task ActivateServiceAsync(int salonId, int serviceId ,CancellationToken ct);
    
    Task DeactivateServiceAsync(int salonId, int serviceId, CancellationToken ct);
    
    Task<int> AddServiceAsync(int salonId, string newName, CancellationToken ct);
    
    Task EditNameServiceAsync(int salonId, int serviceId, string newName, CancellationToken ct);
}