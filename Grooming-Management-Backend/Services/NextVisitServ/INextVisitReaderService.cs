using Grooming_Management_App.DTOs.NextVisitDTO;

namespace Grooming_Management_App.Services.NextVisitServ;

public interface INextVisitReaderService
{
    Task<List<GetDueDogDto>> GetDueDogsAsync(int salonId, CancellationToken ct);
}