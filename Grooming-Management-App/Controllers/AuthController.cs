using Grooming_Management_App.DTOs.AuthDTO;
using Grooming_Management_App.Services.AuthServ;
using Grooming_Management_App.Services.CurrentUserServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Authentication")]
public class AuthController(IAuthenticationService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost("RegisterGroomer")]
    [Authorize(Roles = "Owner")]
    public async Task<CreateGroomerAccountResultDto> RegisterGroomerAccount(int groomerId, CreateAccountDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        return await service.RegisterGroomerAccountAsync(salonId, groomerId, dto, ct);
        
    }
    
    [HttpPost("RegisterSalon")]
    [AllowAnonymous]
    public async Task<LoginResponseDto> RegisterSalon(RegisterNewSalonDto dto, CancellationToken ct)
    {
        return await service.RegisterSalonAsync(dto, ct);
    }
    
    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<LoginResponseDto> Login(LoginDto dto, CancellationToken ct)
    {
        return await service.LoginAsync(dto, ct);
    }
    
    [HttpPost("RefreshToken")]
    [AllowAnonymous]
    public async Task<LoginResponseDto> RefreshToken(string refreshToken, CancellationToken ct)
    {
        return await service.RefreshTokenAsync(refreshToken, ct);
    }
    
    [HttpPost("ChangePassword")]
    [Authorize]
    public async Task<LoginResponseDto> ChangePassword(ChangePasswordDto dto, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        return await service.ChangePasswordAsync(userId, dto, ct);
    }

    [HttpPost("Logout")]
    [Authorize]
    public async Task<IActionResult> Logout(string refreshToken, CancellationToken ct)
    {
        await  service.LogoutAsync(refreshToken, ct);
        return NoContent();
    }

    [HttpPost("LogoutEverywhere")]
    [Authorize]
    public async Task<IActionResult> LogoutAllDevicesAsync(CancellationToken ct)
    {
        var userId =  currentUser.UserId; 
        await service.LogoutAllDevicesAsync(userId, ct);
        return NoContent();
    }
    
}