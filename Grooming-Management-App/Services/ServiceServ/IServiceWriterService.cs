namespace Grooming_Management_App.Services.ServiceServ;

public interface IServiceWriterService
{
    Task ActivateServiceAsync(int salonId, int serviceId ,CancellationToken ct);
    
    Task DeactivateServiceAsync(int salonId, int serviceId, CancellationToken ct);
    
    Task<int> AddServiceAsync(int salonId, string newName, CancellationToken ct);
    
    Task EditNameServiceAsync(int salonId, int serviceId, string newName, CancellationToken ct);
}