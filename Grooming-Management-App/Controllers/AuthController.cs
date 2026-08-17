using Grooming_Management_App.DTOs.AuthDTO;
using Grooming_Management_App.Services.AuthServ;
using Grooming_Management_App.Services.CurrentUserServ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Authentication")]
public class AuthController(ILoginService loginService, IPasswordService passwordService, IRegistrationService registrationService, ITokenSessionService tokenService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost("RegisterGroomer")]
    [Authorize(Roles = "Owner")]
    [EndpointSummary("Tworzy konto logowania dla istniejącego pracownika - dostępne tylko dla właściciela")]
    public async Task<CreateGroomerAccountResultDto> RegisterGroomerAccount(int groomerId, CreateAccountDto dto, CancellationToken ct)
    {
        var salonId = currentUser.SalonId;
        return await registrationService.RegisterGroomerAccountAsync(salonId, groomerId, dto, ct);
        
    }
    
    [HttpPost("RegisterSalon")]
    [AllowAnonymous]
    [EndpointSummary("Rejestruje nowy salon wraz z pierwszym kontem właściciela")]
    public async Task<LoginResponseDto> RegisterSalon(RegisterNewSalonDto dto, CancellationToken ct)
    {
        return await registrationService.RegisterSalonAsync(dto, ct);
    }
    
    [HttpPost("Login")]
    [AllowAnonymous]
    [EndpointSummary("Loguje użytkownika i zwraca komplet tokenów dostępu")]
    public async Task<LoginResponseDto> Login(LoginDto dto, CancellationToken ct)
    {
        return await loginService.LoginAsync(dto, ct);
    }
    
    [HttpPost("RefreshToken")]
    [AllowAnonymous]
    [EndpointSummary("Wymienia ważny refresh token na nowy komplet tokenów")]
    public async Task<LoginResponseDto> RefreshToken(string refreshToken, CancellationToken ct)
    {
        return await tokenService.RefreshTokenAsync(refreshToken, ct);
    }
    
    [HttpPost("ChangePassword")]
    [Authorize]
    [EndpointSummary("Zmienia hasło zalogowanego użytkownika")]
    public async Task<LoginResponseDto> ChangePassword(ChangePasswordDto dto, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        return await passwordService.ChangePasswordAsync(userId, dto, ct);
    }

    [HttpPost("Logout")]
    [Authorize]
    [EndpointSummary("Wylogowuje z jednego urządzenia - unieważnia podany refresh token")]
    public async Task<IActionResult> Logout(string refreshToken, CancellationToken ct)
    {
        await  tokenService.LogoutAsync(refreshToken, ct);
        return NoContent();
    }

    [HttpPost("LogoutEverywhere")]
    [Authorize]
    [EndpointSummary("Wylogowuje ze wszystkich urządzeń - unieważnia wszystkie aktywne tokeny")]
    public async Task<IActionResult> LogoutAllDevicesAsync(CancellationToken ct)
    {
        var userId =  currentUser.UserId; 
        await tokenService.LogoutAllDevicesAsync(userId, ct);
        return NoContent();
    }
    
}