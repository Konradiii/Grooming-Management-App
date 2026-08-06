using System.Globalization;
using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.Breed;
using Grooming_Management_App.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.Breed;

public class BreedService(GroomingDbContext ctx) : IBreedService
{

    public async Task<GetBreedDto> GetBreedAsync(int breedId, CancellationToken ct)
    {


        var breed = await ctx.Breeds
            .Where(e => e.Id == breedId)
            .Select(e=> new GetBreedDto
            {
                Id = e.Id,
                Name = e.Name
            }).FirstOrDefaultAsync(ct);

        if (breed == null)
        {
            throw new NotFoundException($"breed with id:{breedId} not found");
        }
        return breed;
    }

    public async Task<List<GetBreedDto>> GetAllBreedsAsync(CancellationToken ct)
    {

        
        return await ctx.Breeds
            .Select(e=> new GetBreedDto
            {
                Id = e.Id,
                Name = e.Name
            }).ToListAsync(ct);

    }
    
}