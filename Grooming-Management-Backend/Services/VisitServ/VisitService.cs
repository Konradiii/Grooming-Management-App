using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.DTOs.VisitDTO;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Grooming_Management_App.Services.BlacklistServ;
using Grooming_Management_App.Services.CurrentUserServ;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.VisitServ;

public class VisitService(GroomingDbContext ctx, IBlacklistCheckService blacklistCheckService, ICurrentUserService currentUser) : IVisitReaderService, IVisitWriterService
{
    // Blokady czasu (GroomerTimeOff) opisują czas lokalny salonu przez DateOnly + TimeOnly,
    // a Visit.Date jest w UTC. Konwersja potrzebna przy porównywaniu.
    private static readonly TimeZoneInfo PolishTime =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    public async Task<List<GetAllVisitsDto>> GetAllVisitsAsync(int salonId, VisitFilterDto filter, CancellationToken ct)
    {
        var query = ctx.Visits
            .Where(v => v.SalonId == salonId)
            .Where(v => filter.GroomerId == null || filter.GroomerId == v.GroomerId)
            .Where(e => filter.DateFrom == null || e.Date >= filter.DateFrom)
            .Where(e => filter.DateTo == null || e.Date <= filter.DateTo)
            .Where(v => filter.DogId == null || v.DogId == filter.DogId)
            .Where(v => filter.Status == null || v.Status == filter.Status);

        if (currentUser.Role == RoleEnum.Groomer)
        {
            var me = await GetCurrentGroomerAsync(salonId, ct);

            if (me == null)
                return new List<GetAllVisitsDto>();

            if (!me.CanSeeAllVisits)
            {
                var myId = me.Id;
                query = query.Where(v => v.GroomerId == myId || v.AssistantGroomerId == myId);
            }
        }

        return await query
            .Select(v => new GetAllVisitsDto
            {
                Id = v.Id,
                Date = v.Date,
                DogName = v.Dog.Name,
                GroomerId = v.GroomerId,
                GroomerName = v.Groomer.FirstName + " " + v.Groomer.LastName,
                ServiceName = v.ServiceBreed != null
                    ? v.ServiceBreed.Service.Name
                    : v.Service!.Name,
                EstimatedDuration = v.EstimatedDuration,
                Status = v.Status,
                BreedName = v.Dog.Breed.Name,
                AssistantGroomerFullName = v.AssistantGroomer != null
                    ? v.AssistantGroomer.FirstName + " " + v.AssistantGroomer.LastName
                    : null,
            })
            .ToListAsync(ct);
    }

    public async Task<GetVisitDetailsDto> GetVisitAsync(int salonId, int visitId, CancellationToken ct)
    {
        var visit = await ctx.Visits
            .Where(v => v.SalonId == salonId)
            .Where(v => v.Id == visitId)
            .Select(v => new GetVisitDetailsDto
            {
                Id = v.Id,
                CreatedAt = v.CreatedAt,
                Date = v.Date,
                EstimatedDuration = v.EstimatedDuration,
                ProposedPrice = v.ProposedPrice,
                FinalPrice = v.FinalPrice,
                Status = v.Status,
                Notes = v.Notes,
                DogName = v.Dog.Name,
                DogOwnerFullName = v.DogOwner.FirstName + " " + v.DogOwner.LastName,
                GroomerFullName = v.Groomer.FirstName + " " + v.Groomer.LastName,
                ServiceName = v.ServiceBreed != null
                    ? v.ServiceBreed.Service.Name
                    : v.Service!.Name,
                BreedName = v.Dog.Breed.Name,
                AssistantGroomerFullName = v.AssistantGroomer != null
                    ? v.AssistantGroomer.FirstName + " " + v.AssistantGroomer.LastName
                    : null,
                GroomerId = v.GroomerId,
                ServiceBreedId = v.ServiceBreedId,
                BreedId = v.Dog.BreedId,
            }).FirstOrDefaultAsync(ct);

        if (visit == null)
        {
            throw new NotFoundException(ErrorCodes.VisitNotFound);
        }

        return visit;
    }

    public async Task<int> AddVisitAsync(int salonId, AddVisitDto dto, CancellationToken ct)
    {
        if (currentUser.Role == RoleEnum.Groomer)
        {
            var me = await GetCurrentGroomerAsync(salonId, ct);

            if (me == null || !me.CanCreateVisits)
                throw new ForbiddenException(ErrorCodes.NoPermissionToCreateVisits);
        }
        
        Console.WriteLine($"BACKEND ODEBRAŁ: {dto.Date:O} Kind={dto.Date.Kind}");

        // dokładnie jedno z dwóch źródeł usługi
        if ((dto.ServiceBreedId == null) == (dto.ServiceId == null))
        {
            throw new ConflictException(ErrorCodes.ServiceRequired);
        }

        if (dto.DurationMinutes is <= 0)
        {
            throw new ConflictException(ErrorCodes.InvalidDuration);
        }

        var dog = await ctx.Dogs
            .Where(d => d.SalonId == salonId)
            .Where(d => d.Id == dto.DogId)
            .FirstOrDefaultAsync(ct);

        if (dog == null)
        {
            throw new NotFoundException(ErrorCodes.DogNotFound);
        }

        var groomer = await ctx.Groomers
            .Where(g => g.SalonId == salonId)
            .Where(g => g.Id == dto.GroomerId)
            .FirstOrDefaultAsync(ct);

        if (groomer == null)
        {
            throw new NotFoundException(ErrorCodes.GroomerNotFound);
        }

        decimal price;
        int duration;

        if (dto.ServiceBreedId != null)
        {
            // opcja B — pozycja cennika
            var serviceBreed = await ctx.ServiceBreeds
                .Where(sb => sb.SalonId == salonId)
                .Where(sb => sb.Id == dto.ServiceBreedId)
                .FirstOrDefaultAsync(ct);

            if (serviceBreed == null)
            {
                throw new NotFoundException(ErrorCodes.ServiceBreedNotFound);
            }

            if (serviceBreed.BreedId != dog.BreedId)
            {
                throw new ConflictException(ErrorCodes.BreedMismatch);
            }

            price = serviceBreed.Price;
            duration = dto.DurationMinutes ?? serviceBreed.Duration;
        }
        else
        {
            // opcja A — sama usługa
            var serviceExists = await ctx.Services
                .AnyAsync(s => s.Id == dto.ServiceId && s.SalonId == salonId, ct);

            if (!serviceExists)
            {
                throw new NotFoundException(ErrorCodes.ServiceNotFound);
            }

            if (dto.Price == null || dto.Price <= 0)
            {
                throw new ConflictException(ErrorCodes.PriceRequired);
            }

            if (dto.DurationMinutes == null)
            {
                throw new ConflictException(ErrorCodes.InvalidDuration);
            }

            price = dto.Price.Value;
            duration = dto.DurationMinutes.Value;
        }

        var duplicateExists = await ctx.Visits
            .AnyAsync(v => v.DogId == dto.DogId
                           && v.Date == dto.Date
                           && v.SalonId == salonId, ct);

        if (duplicateExists)
        {
            throw new ConflictException(ErrorCodes.DuplicateVisit);
        }

        if (dto.AssistantGroomerId != null)
        {
            if (dto.AssistantGroomerId == dto.GroomerId)
                throw new ConflictException(ErrorCodes.AssistantMustDiffer);

            var assistantExists = await ctx.Groomers
                .AnyAsync(g => g.Id == dto.AssistantGroomerId && g.SalonId == salonId, ct);

            if (!assistantExists)
                throw new NotFoundException(ErrorCodes.AssistantNotFound);
        }

        var isBlocked = await blacklistCheckService.IsBlockedAsync(salonId, dog.DogOwnerId, dto.DogId, ct);

        if (isBlocked)
        {
            throw new ConflictException(ErrorCodes.ClientBlacklisted);
        }

        var startTime = dto.Date;
        var endTime = dto.Date.AddMinutes(duration);

        if (!dto.IgnoreOverlap)
        {
            var visitOverlaps = await ctx.Visits
                .Where(v => v.GroomerId == dto.GroomerId)
                .Where(v => v.SalonId == salonId)
                .Where(v => v.Status != StatusEnum.Cancelled && v.Status != StatusEnum.NoShow)
                .AnyAsync(v => startTime < v.Date.AddMinutes(v.EstimatedDuration)
                               && endTime > v.Date, ct);

            if (visitOverlaps)
                throw new ConflictException(ErrorCodes.VisitOverlaps);
        }

        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(startTime, PolishTime);
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(endTime, PolishTime);

        var timeOffOverlaps = await ctx.GroomerTimeOffs
            .Where(t => t.SalonId == salonId)
            .Where(t => t.GroomerId == dto.GroomerId)
            .AnyAsync(t => startLocal < t.EndDate.ToDateTime(t.EndTime)
                           && endLocal > t.StartDate.ToDateTime(t.StartTime), ct);

        if (timeOffOverlaps)
            throw new ConflictException(ErrorCodes.GroomerUnavailable);

        var newVisit = new Visit
        {
            CreatedAt = DateTime.UtcNow,
            Date = dto.Date,
            EstimatedDuration = duration,
            ProposedPrice = price,
            Status = StatusEnum.Scheduled,
            Notes = dto.Notes,
            SalonId = salonId,
            DogId = dto.DogId,
            DogOwnerId = dog.DogOwnerId,
            GroomerId = dto.GroomerId,
            ServiceBreedId = dto.ServiceBreedId,
            ServiceId = dto.ServiceId,
            SettlementType = groomer.SettlementType,
            SettlementRate = groomer.SettlementRate,
            AssistantGroomerId = dto.AssistantGroomerId
        };

        ctx.Visits.Add(newVisit);
        await ctx.SaveChangesAsync(ct);
        return newVisit.Id;
    }

  public async Task EditVisitAsync(int salonId, int visitId, EditVisitDto dto, CancellationToken ct)
{
    if (dto.DurationMinutes <= 0)
    {
        throw new ConflictException(ErrorCodes.InvalidDuration);
    }

    if (dto.ProposedPrice < 0)
    {
        throw new ConflictException(ErrorCodes.InvalidPrice);
    }

    var visit = await ctx.Visits
        .Include(v => v.Dog)
        .Where(v => v.SalonId == salonId && v.Id == visitId)
        .FirstOrDefaultAsync(ct);

    if (visit == null)
    {
        throw new NotFoundException(ErrorCodes.VisitNotFound);
    }

    var groomerExist = await ctx.Groomers
        .Where(g => salonId == g.SalonId && g.Id == dto.GroomerId)
        .AnyAsync(ct);

    if (!groomerExist)
    {
        throw new NotFoundException(ErrorCodes.GroomerNotFound);
    }

    if (dto.ServiceBreedId != null)
    {
        var matchesBreed = await ctx.ServiceBreeds
            .Where(sb => sb.SalonId == salonId && sb.Id == dto.ServiceBreedId)
            .AnyAsync(sb => sb.BreedId == visit.Dog.BreedId, ct);

        if (!matchesBreed)
        {
            throw new ConflictException(ErrorCodes.ServiceBreedMismatch);
        }
    }

    var startTime = dto.Date;
    var endTime = dto.Date.AddMinutes(dto.DurationMinutes);

    if (!dto.IgnoreOverlap)
    {
        var visitOverlaps = await ctx.Visits
            .Where(d => d.SalonId == salonId)
            .Where(d => d.GroomerId == dto.GroomerId)
            .Where(d => d.Id != visitId)
            .Where(d => d.Status != StatusEnum.Cancelled && d.Status != StatusEnum.NoShow)
            .AnyAsync(d => startTime < d.Date.AddMinutes(d.EstimatedDuration)
                           && endTime > d.Date, ct);

        if (visitOverlaps)
        {
            throw new ConflictException(ErrorCodes.VisitOverlaps);
        }
    }

    var startLocal = TimeZoneInfo.ConvertTimeFromUtc(startTime, PolishTime);
    var endLocal = TimeZoneInfo.ConvertTimeFromUtc(endTime, PolishTime);

    var timeOffOverlaps = await ctx.GroomerTimeOffs
        .Where(t => t.SalonId == salonId)
        .Where(t => t.GroomerId == dto.GroomerId)
        .AnyAsync(t => startLocal < t.EndDate.ToDateTime(t.EndTime)
                       && endLocal > t.StartDate.ToDateTime(t.StartTime), ct);

    if (timeOffOverlaps)
    {
        throw new ConflictException(ErrorCodes.GroomerUnavailable);
    }

    visit.Date = dto.Date;
    visit.GroomerId = dto.GroomerId;
    visit.EstimatedDuration = dto.DurationMinutes;
    visit.ServiceBreedId = dto.ServiceBreedId;
    visit.ProposedPrice = dto.ProposedPrice;
    visit.Notes = dto.Notes;

    await ctx.SaveChangesAsync(ct);
}

    public async Task ChangeVisitStatusAsync(int salonId, int visitId, StatusEnum status, CancellationToken ct)
    {
        var visit = await ctx.Visits
            .Where(v => v.SalonId == salonId && v.Id == visitId)
            .FirstOrDefaultAsync(ct);

        if (visit == null)
        {
            throw new NotFoundException(ErrorCodes.VisitNotFound);
        }

        visit.Status = status;
        await ctx.SaveChangesAsync(ct);
    }

    public async Task UpdateFinalPriceAsync(int salonId, int visitId, decimal finalPrice, CancellationToken ct)
    {
        var visit = await ctx.Visits
            .Where(v => v.SalonId == salonId && v.Id == visitId)
            .FirstOrDefaultAsync(ct);

        if (visit == null)
        {
            throw new NotFoundException(ErrorCodes.VisitNotFound);
        }

        visit.FinalPrice = finalPrice;
        await ctx.SaveChangesAsync(ct);
    }

    public async Task<int> CreateVisitWithNewDogAsync(int salonId, CreateVisitWithNewDogDto dto, CancellationToken ct)
    {
        if (currentUser.Role == RoleEnum.Groomer)
        {
            var me = await GetCurrentGroomerAsync(salonId, ct);

            if (me == null || !me.CanCreateVisits)
                throw new ForbiddenException(ErrorCodes.NoPermissionToCreateVisits);
        }

        Validate.NotEmpty(dto.FirstName, ErrorCodes.NameRequired);
        Validate.NotEmpty(dto.LastName, ErrorCodes.NameRequired);
        Validate.NotEmpty(dto.DogName, ErrorCodes.NameRequired);
        Validate.PolishPhone(dto.Phone);

        if ((dto.ServiceBreedId == null) == (dto.ServiceId == null))
        {
            throw new ConflictException(ErrorCodes.ServiceRequired);
        }

        if (dto.DurationMinutes is <= 0)
        {
            throw new ConflictException(ErrorCodes.InvalidDuration);
        }

        var trimmedPhone = dto.Phone.Trim();

        var blacklistedByPhone = await ctx.Blacklists
            .Where(b => b.SalonId == salonId)
            .AnyAsync(b => b.DogOwner.Phone == trimmedPhone, ct);

        if (blacklistedByPhone)
        {
            throw new ConflictException(ErrorCodes.ClientBlacklisted);
        }

        var ownerExists = await ctx.DogOwners
            .AnyAsync(o => o.Phone == trimmedPhone && o.SalonId == salonId, ct);

        if (ownerExists)
        {
            throw new ConflictException(ErrorCodes.PhoneTaken);
        }

        var breedExists = await ctx.Breeds.AnyAsync(b => b.Id == dto.BreedId, ct);

        if (!breedExists)
        {
            throw new NotFoundException(ErrorCodes.BreedNotFound);
        }

        var groomer = await ctx.Groomers
            .Where(g => g.SalonId == salonId)
            .Where(g => g.Id == dto.GroomerId)
            .FirstOrDefaultAsync(ct);

        if (groomer == null)
        {
            throw new NotFoundException(ErrorCodes.GroomerNotFound);
        }

        decimal price;
        int duration;

        if (dto.ServiceBreedId != null)
        {
            var serviceBreed = await ctx.ServiceBreeds
                .Where(s => s.Id == dto.ServiceBreedId && s.SalonId == salonId)
                .FirstOrDefaultAsync(ct);

            if (serviceBreed == null)
            {
                throw new NotFoundException(ErrorCodes.ServiceBreedNotFound);
            }

            if (serviceBreed.BreedId != dto.BreedId)
            {
                throw new ConflictException(ErrorCodes.BreedMismatch);
            }

            price = serviceBreed.Price;
            duration = dto.DurationMinutes ?? serviceBreed.Duration;
        }
        else
        {
            var serviceExists = await ctx.Services
                .AnyAsync(s => s.Id == dto.ServiceId && s.SalonId == salonId, ct);

            if (!serviceExists)
            {
                throw new NotFoundException(ErrorCodes.ServiceNotFound);
            }

            if (dto.Price == null || dto.Price <= 0)
            {
                throw new ConflictException(ErrorCodes.PriceRequired);
            }

            if (dto.DurationMinutes == null)
            {
                throw new ConflictException(ErrorCodes.InvalidDuration);
            }

            price = dto.Price.Value;
            duration = dto.DurationMinutes.Value;
        }

        if (dto.AssistantGroomerId != null)
        {
            if (dto.AssistantGroomerId == dto.GroomerId)
                throw new ConflictException(ErrorCodes.AssistantMustDiffer);

            var assistantExists = await ctx.Groomers
                .AnyAsync(g => g.Id == dto.AssistantGroomerId && g.SalonId == salonId, ct);

            if (!assistantExists)
                throw new NotFoundException(ErrorCodes.AssistantNotFound);
        }

        var startTime = dto.Date;
        var endTime = dto.Date.AddMinutes(duration);

        if (!dto.IgnoreOverlap)
        {
            var visitOverlaps = await ctx.Visits
                .Where(v => v.GroomerId == dto.GroomerId)
                .Where(v => v.SalonId == salonId)
                .Where(v => v.Status != StatusEnum.Cancelled && v.Status != StatusEnum.NoShow)
                .AnyAsync(v => startTime < v.Date.AddMinutes(v.EstimatedDuration)
                               && endTime > v.Date, ct);

            if (visitOverlaps)
            {
                throw new ConflictException(ErrorCodes.VisitOverlaps);
            }
        }

        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(startTime, PolishTime);
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(endTime, PolishTime);

        var timeOffOverlaps = await ctx.GroomerTimeOffs
            .Where(t => t.SalonId == salonId)
            .Where(t => t.GroomerId == dto.GroomerId)
            .AnyAsync(t => startLocal < t.EndDate.ToDateTime(t.EndTime)
                        && endLocal > t.StartDate.ToDateTime(t.StartTime), ct);

        if (timeOffOverlaps)
        {
            throw new ConflictException(ErrorCodes.GroomerUnavailable);
        }

        var owner = new Models.DogOwner
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Phone = trimmedPhone,
            SalonId = salonId
        };

        var dog = new Dog
        {
            Name = dto.DogName.Trim(),
            AgeInMonths = dto.AgeInMonths,
            BreedId = dto.BreedId,
            Notes = dto.DogNotes,
            SalonId = salonId
        };

        owner.Dogs.Add(dog);

        var visit = new Visit
        {
            CreatedAt = DateTime.UtcNow,
            Date = dto.Date,
            EstimatedDuration = duration,
            ProposedPrice = price,
            Status = StatusEnum.Scheduled,
            Notes = dto.Notes,
            SalonId = salonId,
            Dog = dog,
            DogOwner = owner,
            GroomerId = dto.GroomerId,
            ServiceBreedId = dto.ServiceBreedId,
            ServiceId = dto.ServiceId,
            SettlementType = groomer.SettlementType,
            SettlementRate = groomer.SettlementRate,
            AssistantGroomerId = dto.AssistantGroomerId
        };

        ctx.DogOwners.Add(owner);
        ctx.Visits.Add(visit);

        await ctx.SaveChangesAsync(ct);

        return visit.Id;
    }

    private async Task<Groomer?> GetCurrentGroomerAsync(int salonId, CancellationToken ct)
    {
        return await ctx.Groomers
            .FirstOrDefaultAsync(g => g.SalonId == salonId
                                      && g.UserId == currentUser.UserId, ct);
    }
}