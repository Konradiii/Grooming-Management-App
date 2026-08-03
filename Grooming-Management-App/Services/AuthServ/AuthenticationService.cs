using System.Security.Cryptography;
using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.AuthDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Grooming_Management_App.Services.PasswordHasherServ;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.AuthServ;

public class AuthenticationService(GroomingDbContext ctx, IPasswordHasher passwordHasher) : IAuthenticationService
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
        };

        
        var ownerUser= new User
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

        return new LoginResponseDto { };


    }
    
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken ct)
    {

        return null;

    }
    
    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        return null;
    }
    
    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken ct)
    {
        
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct)
    {
        
    }
    
    public async Task LogoutAllDevicesAsync(int userId, CancellationToken ct)
    {
        
    }
}