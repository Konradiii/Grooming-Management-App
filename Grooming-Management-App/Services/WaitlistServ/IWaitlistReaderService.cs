using Grooming_Management_App.DTOs.WaitlistDTO;

namespace Grooming_Management_App.Services.WaitlistServ;

public interface IWaitlistReaderService
{


    
    Task<List<GetWaitlistDto>> GetAllWaitlistAsync(int salonId, CancellationToken ct);
}