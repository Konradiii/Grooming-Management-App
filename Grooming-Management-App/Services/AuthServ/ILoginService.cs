using Grooming_Management_App.DTOs.AuthDTO;

namespace Grooming_Management_App.Services.AuthServ;

public interface ILoginService
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken ct);

}