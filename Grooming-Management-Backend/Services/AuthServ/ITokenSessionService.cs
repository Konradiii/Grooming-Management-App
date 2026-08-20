using Grooming_Management_App.DTOs.AuthDTO;

namespace Grooming_Management_App.Services.AuthServ;

public interface ITokenSessionService
{
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct);
    
    Task LogoutAsync(string refreshToken, CancellationToken ct);
    
    Task LogoutAllDevicesAsync(int userId, CancellationToken ct);
}