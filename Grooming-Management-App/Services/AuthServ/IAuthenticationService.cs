using Grooming_Management_App.DTOs.AuthDTO;

namespace Grooming_Management_App.Services.AuthServ;

public interface IAuthenticationService
{
    Task<CreateGroomerAccountResultDto> RegisterGroomerAccountAsync(int salonId, int groomerId, CreateAccountDto dto, CancellationToken ct);
    
    Task<LoginResponseDto> RegisterSalonAsync(RegisterNewSalonDto dto, CancellationToken ct);
    
    Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken ct);
    
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct);
    
    Task<LoginResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken ct);

    Task LogoutAsync(string refreshToken, CancellationToken ct);
    
    Task LogoutAllDevicesAsync(int userId, CancellationToken ct);
}