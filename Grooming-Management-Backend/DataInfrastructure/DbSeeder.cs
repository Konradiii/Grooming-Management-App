using Grooming_Management_App.Enums;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.DataInfrastructure;

public static class DbSeeder
{
    public static async Task SeedAsync(GroomingDbContext context)
    {
        if (await context.Salons.AnyAsync())
            return;

        var breeds = new List<Breed>
        {
            new() { Name = "Yorkshire Terrier" },
            new() { Name = "Owczarek Niemiecki" },
            new() { Name = "Labrador Retriever" },
            new() { Name = "Cavalier King Charles Spaniel" }
        };
        context.Breeds.AddRange(breeds);

        var salon = new Salon { Name = "Psi Salon Warszawa" };
        context.Salons.Add(salon);

        var owner = new User
        {
            Email = "owner@test.com",
            PasswordHash = "TEMP_NIE_ZAHASHOWANE",
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
            Salon = salon,
            User = new User
            {
                Email = "anna@test.com",
                PasswordHash = "TEMP_NIE_ZAHASHOWANE",
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
            Breed = breeds[2]
        };
        context.Dogs.Add(dog);

        var serviceStrzyzenie = new Service { Name = "Strzyżenie", Salon = salon };
        var serviceKapiel = new Service { Name = "Kąpiel", Salon = salon };
        context.Services.AddRange(serviceStrzyzenie, serviceKapiel);

        var priceStrzyzenieLabrador = new ServiceBreed
        {
            Service = serviceStrzyzenie,
            Breed = breeds[2],
            Salon = salon,
            Price = 150m,
            Duration = 60
        };

        var priceKapielLabrador = new ServiceBreed
        {
            Service = serviceKapiel,
            Breed = breeds[2],
            Salon = salon,
            Price = 60m,
            Duration = 30
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
            Status = StatusEnum.Scheduled,
            CreatedAt = DateTime.UtcNow
        };
        context.Visits.Add(visit);
        
        foreach (var entry in context.ChangeTracker.Entries<Groomer>())
        {
            Console.WriteLine($"Groomer tracked: FirstName={entry.Entity.FirstName}, LastName={entry.Entity.LastName ?? "NULL"}");
        }

        await context.SaveChangesAsync();
    }
}