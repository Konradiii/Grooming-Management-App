using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Services.TokenServ;

public interface ITokenService
{

    string GenerateAccessToken(int userId, int salonId, RoleEnum role, string? fullName = null);    
    string GenerateRefreshToken();

    string HasherSH256(string token);



}