using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Grooming_Management_App.Enums;
using Microsoft.IdentityModel.Tokens;

namespace Grooming_Management_App.Services.TokenServ;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateAccessToken(int userId, int salonId, RoleEnum role, string? fullName = null)
    {
        var claims = new List<Claim>
        {
            new Claim("userId", userId.ToString()),
            new Claim("salonId", salonId.ToString()),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim("fullName", fullName ?? string.Empty),
        };
        var secretKey = configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var expirationMinutes = configuration.GetValue<int>("JwtSettings:ExpirationMinutes");
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(12);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    public DateTime GetRefreshTokenExpiration()
    {
        var days = configuration.GetValue<int>("JwtSettings:RefreshTokenExpirationDays");
        return DateTime.UtcNow.AddDays(days);
    }
}