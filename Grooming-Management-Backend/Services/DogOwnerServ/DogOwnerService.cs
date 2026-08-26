using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.DogOwner;
using Grooming_Management_App.DTOs.DogOwnerDTO;
using Grooming_Management_App.Exceptions;
using Microsoft.EntityFrameworkCore;


namespace Grooming_Management_App.Services.DogOwner;

public class DogOwnerService(GroomingDbContext ctx) : IDogOwnerReaderService, IDogOwnerWriterService
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
                Phone = e.Phone,
            })
            .FirstOrDefaultAsync(ct);
        
        if (dogOwner == null)
        {
            throw new NotFoundException(ErrorCodes.DogOwnerNotFound);
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
                Phone = e.Phone,
                DogsCount = e.Dogs.Count()
            }).ToListAsync(ct);

    }

    public async Task<int> CreateDogOwnerAsync(CreateDogOwnerDto dto, int salonId, CancellationToken ct)
    {
        
        Validate.NotEmpty(dto.FirstName, ErrorCodes.NameRequired);
        Validate.NotEmpty(dto.LastName, ErrorCodes.NameRequired);
        Validate.PolishPhone(dto.Phone);
        
        var ownerExists = await ctx.DogOwners
            .AnyAsync(e => e.Phone == dto.Phone && e.SalonId == salonId, ct);

        if (ownerExists)
        {
            throw new ConflictException(ErrorCodes.DogOwnerNotFound);
        }
        
        
        var dogOwner = new Models.DogOwner
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            SalonId = salonId
        };
        
        ctx.DogOwners.Add(dogOwner);
        await ctx.SaveChangesAsync(ct);
        return dogOwner.Id;
        
        
    }

    public async Task EditDogOwnerAsync(EditDogOwnerDto dto, int id, int salonId, CancellationToken ct)
    {

        Validate.NotEmpty(dto.FirstName, ErrorCodes.NameRequired);
        Validate.NotEmpty(dto.LastName, ErrorCodes.NameRequired);
        Validate.PolishPhone(dto.Phone);
        
        var ownerExists = await ctx.DogOwners
            .AnyAsync(e => e.Phone == dto.Phone && e.SalonId == salonId, ct);

        if (ownerExists)
        {
            throw new ConflictException(ErrorCodes.DogOwnerNotFound);
        }
        
        var dogOwner = await ctx.DogOwners
            .Where(d => d.Id == id && d.SalonId == salonId)
            .FirstOrDefaultAsync(ct);
        
        if (dogOwner == null)
        {
            throw new NotFoundException(ErrorCodes.DogOwnerNotFound);
        }


       dogOwner.FirstName = dto.FirstName;
       dogOwner.LastName = dto.LastName;
       dogOwner.Phone = dto.Phone;
       
        ctx.DogOwners.Update(dogOwner);
        await ctx.SaveChangesAsync(ct);

    }
}