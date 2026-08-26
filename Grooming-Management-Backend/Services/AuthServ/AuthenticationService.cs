using System.Security.Cryptography;
using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.AuthDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Grooming_Management_App.Services.PasswordHasherServ;
using Grooming_Management_App.Services.TokenServ;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.AuthServ;

public class AuthenticationService(GroomingDbContext ctx, IPasswordHasher passwordHasher, ITokenService tokenService) : ILoginService, IPasswordService, IRegistrationService, ITokenSessionService
{
    public async Task<CreateGroomerAccountResultDto> RegisterGroomerAccountAsync(int salonId, int groomerId, CreateAccountDto dto, CancellationToken ct)
    {
        var groomer = await ctx.Groomers
            .Where(g => g.Id == groomerId && g.SalonId == salonId)
            .FirstOrDefaultAsync(ct);
        if (groomer == null)
        {
            throw new NotFoundException("Groomer not found");
        }

        if (groomer.UserId != null)
        {
            throw new ConflictException("Groomer already has an account");
        }

        var emailTaken = await ctx.Users.Where(u => u.Email == dto.Email).AnyAsync(ct);
        if (emailTaken)
        {
            throw new ConflictException("Email already taken");
        }

        var randomBytes = RandomNumberGenerator.GetBytes(12);
        var temporaryPassword = Convert.ToBase64String(randomBytes);

        var hashedPassword = passwordHasher.HashPassword(temporaryPassword);

        var newRegisteredUser = new User
        {
            Email = dto.Email,
            PasswordHash = hashedPassword,
            Role = RoleEnum.Groomer,
            ActiveStatus = ActiveStatusEnum.Active,
            RequiresPasswordChange = true,
            CreatedAt = DateTime.UtcNow,
            SalonId = salonId,
            Groomer = groomer
        };

        ctx.Users.Add(newRegisteredUser);
        await ctx.SaveChangesAsync(ct);

        return new CreateGroomerAccountResultDto
        {
            Email = dto.Email,
            TemporaryPassword = temporaryPassword,
        };
    }

    public async Task<LoginResponseDto> RegisterSalonAsync(RegisterNewSalonDto dto, CancellationToken ct)
    {
        if (dto.Password != dto.ConfirmPassword)
        {
            throw new ConflictException("Passwords don't match");
        }

        var emailTaken = await ctx.Users.Where(u => u.Email == dto.Email).AnyAsync(ct);
        if (emailTaken)
        {
            throw new ConflictException("Email already taken");
        }

        var hashedPassword = passwordHasher.HashPassword(dto.Password);

        var newSalon = new Salon
        {
            Name = dto.SalonName,
            Street = dto.Street,
            BuildingNumber = dto.BuildingNumber,
            ApartmentNumber = dto.ApartmentNumber,
            PostalCode = dto.PostalCode,
            City = dto.City,
            MinBookingHoursAhead = 0,
            MaxBookingDaysAhead = 550,
            SubscriptionStatus = SubscriptionStatusEnum.Trial,
            SubscriptionValidUntil = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30)
        };

        var ownerUser = new User
        {
            Email = dto.Email,
            PasswordHash = hashedPassword,
            Role = RoleEnum.Owner,
            ActiveStatus = ActiveStatusEnum.Active,
            RequiresPasswordChange = false,
            CreatedAt = DateTime.UtcNow,
            Salon = newSalon
        };
        ctx.Users.Add(ownerUser);
        await ctx.SaveChangesAsync(ct);

        var refreshToken = tokenService.GenerateRefreshToken();
        var accessToken = tokenService.GenerateAccessToken(
            ownerUser.Id, ownerUser.SalonId, ownerUser.Role, newSalon.Name);

        var newTokens = new RefreshToken
        {
            TokenHash = tokenService.HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null,
            User = ownerUser
        };
        ctx.RefreshTokens.Add(newTokens);
        await ctx.SaveChangesAsync(ct);

        return new LoginResponseDto
        {
            RefreshToken = refreshToken,
            AccessToken = accessToken,
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        var user = await ctx.Users
            .IgnoreQueryFilters()
            .Include(u => u.Groomer)
            .Include(u => u.Salon)
            .Where(u => u.Email == dto.Email)
            .FirstOrDefaultAsync(ct);

        if (user == null)
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        if (!passwordHasher.VerifyHashedPassword(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Password or Email not found");
        }

        if (user.ActiveStatus == ActiveStatusEnum.Inactive)
        {
            throw new UnauthorizedException("User is inactive");
        }

        var accessToken = tokenService.GenerateAccessToken(
            user.Id, user.SalonId, user.Role, ResolveDisplayName(user));
        var refreshToken = tokenService.GenerateRefreshToken();

        var newTokens = new RefreshToken
        {
            TokenHash = tokenService.HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null,
            User = user
        };
        ctx.RefreshTokens.Add(newTokens);
        await ctx.SaveChangesAsync(ct);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RequiresPasswordChange = user.RequiresPasswordChange
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        var hashedRefresh = tokenService.HashToken(refreshToken);

        var refreshExists = await ctx.RefreshTokens.Where(e => hashedRefresh == e.TokenHash).FirstOrDefaultAsync(ct);
        if (refreshExists == null)
        {
            throw new UnauthorizedException("Refresh token not found");
        }

        if (refreshExists.RevokedAt != null)
        {
            throw new UnauthorizedException("Refresh token is already revoked");
        }

        if (refreshExists.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token is expired");
        }

        refreshExists.RevokedAt = DateTime.UtcNow;

        var user = await ctx.Users
            .Include(u => u.Groomer)
            .Include(u => u.Salon)
            .Where(e => e.Id == refreshExists.UserId)
            .FirstOrDefaultAsync(ct);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        var newRefreshToken = tokenService.GenerateRefreshToken();
        var newAccessToken = tokenService.GenerateAccessToken(
            refreshExists.UserId, user.SalonId, user.Role, ResolveDisplayName(user));

        var newTokens = new RefreshToken
        {
            TokenHash = tokenService.HashToken(newRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null,
            User = user
        };
        ctx.RefreshTokens.Add(newTokens);
        await ctx.SaveChangesAsync(ct);

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            RequiresPasswordChange = user.RequiresPasswordChange
        };
    }

    public async Task<LoginResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken ct)
    {
        var user = await ctx.Users
            .Include(u => u.Groomer)
            .Include(u => u.Salon)
            .Where(e => e.Id == userId)
            .FirstOrDefaultAsync(ct);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        if (dto.NewPassword != dto.ConfirmNewPassword)
        {
            throw new ConflictException("Passwords do not match");
        }
        if (!passwordHasher.VerifyHashedPassword(dto.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid password");
        }

        user.PasswordHash = passwordHasher.HashPassword(dto.NewPassword);
        user.RequiresPasswordChange = false;

        var activeTokens = await ctx.RefreshTokens
            .Where(e => e.UserId == user.Id)
            .Where(e => e.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        var newRefreshToken = tokenService.GenerateRefreshToken();
        var newAccessToken = tokenService.GenerateAccessToken(
            user.Id, user.SalonId, user.Role, ResolveDisplayName(user));

        var newtoken = new RefreshToken
        {
            TokenHash = tokenService.HashToken(newRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null,
            User = user
        };

        ctx.RefreshTokens.Add(newtoken);
        await ctx.SaveChangesAsync(ct);

        return new LoginResponseDto
        {
            RefreshToken = newRefreshToken,
            AccessToken = newAccessToken,
            RequiresPasswordChange = user.RequiresPasswordChange
        };
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct)
    {
        var hashedToken = tokenService.HashToken(refreshToken);

        var token = await ctx.RefreshTokens
            .Where(e => e.TokenHash == hashedToken)
            .FirstOrDefaultAsync(ct);

        if (token == null)
        {
            throw new NotFoundException("Refresh token not found");
        }

        if (token.RevokedAt != null)
        {
            return;
        }

        token.RevokedAt = DateTime.UtcNow;
        await ctx.SaveChangesAsync(ct);
    }

    public async Task LogoutAllDevicesAsync(int userId, CancellationToken ct)
    {
        var activeTokens = await ctx.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await ctx.SaveChangesAsync(ct);
    }

    private static string ResolveDisplayName(User user)
    {
        if (user.Groomer != null)
            return user.Groomer.FirstName + " " + user.Groomer.LastName;

        return user.Salon?.Name ?? user.Email;
    }
}