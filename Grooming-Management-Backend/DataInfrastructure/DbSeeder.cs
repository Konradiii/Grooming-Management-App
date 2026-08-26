using Grooming_Management_App.Enums;
using Grooming_Management_App.Models;
using Grooming_Management_App.Services.PasswordHasherServ;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.DataInfrastructure;

public static class DbSeeder
{
    private const string DefaultPassword = "Test1234!";

    public static async Task SeedAsync(GroomingDbContext context, IPasswordHasher passwordHasher)
    {
        if (await context.Salons.AnyAsync())
            return;

        var hashedPassword = passwordHasher.HashPassword(DefaultPassword);


        var labrador = await context.Breeds
            .FirstAsync(b => b.Name == "Labrador Retriever");
        
        var salon = new Salon
        {
            Name = "Psi Salon Warszawa",
            MinBookingHoursAhead = 0,
            MaxBookingDaysAhead = 550,
            SubscriptionStatus = SubscriptionStatusEnum.Trial,
            SubscriptionValidUntil = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30)
        };
        context.Salons.Add(salon);

        var owner = new User
        {
            Email = "owner@test.com",
            PasswordHash = hashedPassword,
            Role = RoleEnum.Owner,
            ActiveStatus = ActiveStatusEnum.Active,
            RequiresPasswordChange = false,
            CreatedAt = DateTime.UtcNow,
            Salon = salon
        };
        context.Users.Add(owner);

        var groomerWithAccount = new Groomer
        {
            FirstName = "Anna",
            LastName = "Kowalska",
            ActiveStatus = ActiveStatusEnum.Active,
            SettlementType = SettlementTypeEnum.Percentage,
            SettlementRate = 50m,
            CanSeeAllVisits = true,
            CanCreateVisits = true,
            Salon = salon,
            User = new User
            {
                Email = "anna@test.com",
                PasswordHash = hashedPassword,
                Role = RoleEnum.Groomer,
                ActiveStatus = ActiveStatusEnum.Active,
                RequiresPasswordChange = true,
                CreatedAt = DateTime.UtcNow,
                Salon = salon
            }
        };

        var groomerWithoutAccount = new Groomer
        {
            FirstName = "Piotr",
            LastName = "Zieliński",
            ActiveStatus = ActiveStatusEnum.Active,
            SettlementType = SettlementTypeEnum.Hourly,
            SettlementRate = 40m,
            CanSeeAllVisits = true,
            CanCreateVisits = true,
            Salon = salon
        };

        context.Groomers.AddRange(groomerWithAccount, groomerWithoutAccount);

        var dogOwner = new DogOwner
        {
            FirstName = "Jan",
            LastName = "Nowak",
            Phone = "+48123456789",
            Salon = salon
        };
        context.DogOwners.Add(dogOwner);

        var dog = new Dog
        {
            Name = "Rex",
            AgeInMonths = 24,
            Notes = "Spokojny, lubi wodę",
            Salon = salon,
            DogOwner = dogOwner,
            Breed = labrador
        };
        context.Dogs.Add(dog);

        var serviceStrzyzenie = new Service
        {
            Name = "Strzyżenie",
            Status = ActiveStatusEnum.Active,
            Salon = salon
        };
        var serviceKapiel = new Service
        {
            Name = "Kąpiel",
            Status = ActiveStatusEnum.Active,
            Salon = salon
        };

        context.Services.AddRange(serviceStrzyzenie, serviceKapiel);

        foreach (var name in new[] { "Obcinanie pazurów", "Kompleksowa pielęgnacja", "Trymowanie" })
        {
            context.Services.Add(new Service
            {
                Name = name,
                Status = ActiveStatusEnum.Active,
                Salon = salon
            });
        }

        var priceStrzyzenieLabrador = new ServiceBreed
        {
            Service = serviceStrzyzenie,
            Breed = labrador,
            Salon = salon,
            Price = 150m,
            Duration = 60,
            Status = ActiveStatusEnum.Active
        };

        var priceKapielLabrador = new ServiceBreed
        {
            Service = serviceKapiel,
            Breed = labrador,
            Salon = salon,
            Price = 60m,
            Duration = 30,
            Status = ActiveStatusEnum.Active
        };

        context.ServiceBreeds.AddRange(priceStrzyzenieLabrador, priceKapielLabrador);

        var visit = new Visit
        {
            Salon = salon,
            Dog = dog,
            DogOwner = dogOwner,
            Groomer = groomerWithAccount,
            ServiceBreed = priceStrzyzenieLabrador,
            Date = DateTime.UtcNow.AddDays(3),
            EstimatedDuration = priceStrzyzenieLabrador.Duration,
            ProposedPrice = priceStrzyzenieLabrador.Price,
            SettlementType = groomerWithAccount.SettlementType,
            SettlementRate = groomerWithAccount.SettlementRate,
            Status = StatusEnum.Scheduled,
            CreatedAt = DateTime.UtcNow
        };
        context.Visits.Add(visit);

        await context.SaveChangesAsync();
    }
}