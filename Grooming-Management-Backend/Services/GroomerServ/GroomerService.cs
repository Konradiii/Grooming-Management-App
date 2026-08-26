using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.GroomerDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Grooming_Management_App.Services.CurrentUserServ;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.GroomerServ;

public class GroomerService(GroomingDbContext ctx, ICurrentUserService currentUser) : IGroomerReaderService, IGroomerWriterService
{
    public async Task DeactivateGroomerAsync(int id, int salonId, CancellationToken ct)
    {
        var groomer = await ctx.Groomers
            .Where(g => g.Id == id && g.SalonId == salonId)
            .FirstOrDefaultAsync(ct);

        if (groomer == null)
        {
            throw new NotFoundException(ErrorCodes.GroomerNotFound);
        }

        if (groomer.ActiveStatus == ActiveStatusEnum.Inactive)
        {
            return;
        }
        
        groomer.ActiveStatus = ActiveStatusEnum.Inactive;
        await ctx.SaveChangesAsync(ct);
        
    }
    
    public async Task ActivateGroomerAsync(int id, int salonId, CancellationToken ct)
    {
        
        var groomer = await ctx.Groomers
            .Where(g => g.Id == id && g.SalonId == salonId)
            .FirstOrDefaultAsync(ct);

        if (groomer == null)
        {
            throw new NotFoundException(ErrorCodes.GroomerNotFound);
        }

        if (groomer.ActiveStatus == ActiveStatusEnum.Active)
        {
            return;
        }
        
        groomer.ActiveStatus = ActiveStatusEnum.Active;
        await ctx.SaveChangesAsync(ct);
        
    }
    
    public async Task<GetGroomerDto> GetGroomerAsync(int id, int salonId, CancellationToken ct)
    {
        
        var result = await ctx.Groomers
            .Where(g => g.Id == id && g.SalonId == salonId)
            .Select(e=> new GetGroomerDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                ActiveStatus = e.ActiveStatus,
                SettlementType = e.SettlementType,
                SettlementRate = e.SettlementRate,
                HasAccount = e.UserId != null,
                CanSeeAllVisits = e.CanSeeAllVisits,
                CanCreateVisits = e.CanCreateVisits,
            }).FirstOrDefaultAsync(ct);
        if (result == null)
        {
            throw new NotFoundException(ErrorCodes.GroomerNotFound);
        }
        return result;
        
    }
    public async Task<List<GetGroomerDto>> GetAllGroomersAsync(int salonId, CancellationToken ct)
    {
        
        return await ctx.Groomers
            .Where(g => g.SalonId == salonId)
            .Select(e => new GetGroomerDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                ActiveStatus = e.ActiveStatus,
                SettlementType = e.SettlementType,
                SettlementRate = e.SettlementRate,
                HasAccount = e.UserId != null,
            }).ToListAsync(ct);
        
    }
    
    public async Task<List<GetGroomerBasicDto>> GetAllGroomersBasicAsync(int salonId, CancellationToken ct)
    {
        
        return await ctx.Groomers
            .Where(g => g.SalonId == salonId)
            .Select(e => new GetGroomerBasicDto
            {
                Id = e.Id,
                FullName =e.FirstName + " " + e.LastName,
                ActiveStatus = e.ActiveStatus,
            }).ToListAsync(ct);
        
    }
    
    public async Task EditGroomerAsync(EditGroomerDto dto, int id, int salonId,  CancellationToken ct)
    {
        var edited = await ctx.Groomers
            .Where(g => g.Id == id && g.SalonId == salonId)
            .FirstOrDefaultAsync(ct);
        if (edited == null)
        {
            throw new NotFoundException(ErrorCodes.GroomerNotFound);
        }
        edited.FirstName = dto.FirstName;
        edited.LastName = dto.LastName;
        edited.SettlementType = dto.SettlementType;
        edited.SettlementRate = dto.SettlementRate;
        edited.CanSeeAllVisits = dto.CanSeeAllVisits;
        edited.CanCreateVisits = dto.CanCreateVisits;
        
        await ctx.SaveChangesAsync(ct);
        
    }
    
    public async Task<int> CreateGroomerAsync(CreateGroomerDto dto, int salonId, CancellationToken ct)
    {
        Validate.NotEmpty(dto.FirstName, ErrorCodes.NameRequired);
        Validate.NotEmpty(dto.LastName, ErrorCodes.NameRequired);

        var newGroomer = new Groomer
        {
            SalonId = salonId,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            ActiveStatus = ActiveStatusEnum.Active,
            CanSeeAllVisits = true,
            CanCreateVisits = true,
        };

        ctx.Groomers.Add(newGroomer);
        await ctx.SaveChangesAsync(ct);

        return newGroomer.Id;
    }
    
    public async Task<GetGroomerBasicDto?> GetCurrentGroomerAsync(int salonId, CancellationToken ct)
    {
        return await ctx.Groomers
            .Where(g => g.SalonId == salonId)
            .Where(g => g.UserId == currentUser.UserId)
            .Select(g => new GetGroomerBasicDto
            {
                Id = g.Id,
                FullName = g.FirstName + " " + g.LastName,
                ActiveStatus = g.ActiveStatus,
                CanCreateVisits = g.CanCreateVisits
            })
            .FirstOrDefaultAsync(ct);
    }
    
    
}
