using Grooming_Management_App.DTOs.ServiceDTO;
using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Services.ServiceServ;

public interface IServiceReaderService
{
    Task<List<GetServiceDto>> GetAllServicesAsync(int salonId, ActiveStatusEnum? status, CancellationToken ct);
    
    Task<GetServiceDto> GetServiceAsync(int salonId, int serviceId, CancellationToken ct);
}