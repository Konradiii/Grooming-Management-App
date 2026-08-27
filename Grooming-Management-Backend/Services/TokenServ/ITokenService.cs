using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Services.TokenServ;

public interface ITokenService
{

    string GenerateAccessToken(int userId, int salonId, RoleEnum role, string? fullName = null, string? email = null);
    string GenerateRefreshToken();
    string HashToken(string token);
    DateTime GetRefreshTokenExpiration();
    

}