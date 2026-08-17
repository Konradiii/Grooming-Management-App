using Grooming_Management_App.DTOs.AuthDTO;

namespace Grooming_Management_App.Services.AuthServ;

public interface IPasswordService
{
    Task<LoginResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken ct);

}