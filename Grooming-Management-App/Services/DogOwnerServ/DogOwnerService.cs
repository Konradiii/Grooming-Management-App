using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.DogOwner;
using Grooming_Management_App.Exceptions;
using Microsoft.EntityFrameworkCore;


namespace Grooming_Management_App.Services.DogOwner;

public class DogOwnerService(GroomingDbContext ctx) : IDogOwnerService
{
    public async Task<GetDogOwnerDto> GetDogOwnerAsync(int id, int salonId, CancellationToken ct)
    {

        var dogOwner = await ctx.DogOwners
            .Where(d => d.Id == id && d.SalonId == salonId)
            .Select(e => new GetDogOwnerDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
            })
            .FirstOrDefaultAsync(ct);
        
        if (dogOwner == null)
        {
            throw new NotFoundException($"DogOwner with id: {id} not found");
        }

        return dogOwner;

    }

    public async Task<List<GetDogOwnerDto>> GetAllDogOwnersAsync(int salonId, CancellationToken ct)
    {

        return await ctx.DogOwners
            .Where(e => salonId == e.SalonId)
            .Select(e => new GetDogOwnerDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
            }).ToListAsync(ct);

    }

    public async Task CreateDogOwnerAsync(CreateDogOwnerDto dto, int salonId, CancellationToken ct)
    {
        var dogOwner = new Models.DogOwner
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            SalonId = salonId
        };
        
        ctx.DogOwners.Add(dogOwner);
        await ctx.SaveChangesAsync(ct);
        
        
    }

    public async Task EditDogOwnerAsync(EditDogOwnerDto dto, int id, int salonId, CancellationToken ct)
    {

        var dogOwner = await ctx.DogOwners
            .Where(d => d.Id == id && d.SalonId == salonId)
            .FirstOrDefaultAsync(ct);
        
        if (dogOwner == null)
        {
            throw new NotFoundException($"DogOwner with id: {id} not found");
        }


       dogOwner.FirstName = dto.FirstName;
       dogOwner.LastName = dto.LastName;
       dogOwner.Email = dto.Email; 
       dogOwner.Phone = dto.Phone;
       
        ctx.DogOwners.Update(dogOwner);
        await ctx.SaveChangesAsync(ct);

    }
}