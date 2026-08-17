using Grooming_Management_App.DTOs.AuthDTO;

namespace Grooming_Management_App.Services.AuthServ;

public interface IRegistrationService
{
    Task<CreateGroomerAccountResultDto> RegisterGroomerAccountAsync(int salonId, int groomerId, CreateAccountDto dto, CancellationToken ct);
    
    Task<LoginResponseDto> RegisterSalonAsync(RegisterNewSalonDto dto, CancellationToken ct);
}