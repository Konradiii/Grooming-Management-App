using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Models;

public class Salon
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public string? ApartmentNumber { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    
    public string? Phone { get; set; }
    public int MinBookingHoursAhead { get; set; }
    public int MaxBookingDaysAhead { get; set; }
    
    public DateOnly? SubscriptionValidUntil { get; set; }
    public SubscriptionStatusEnum SubscriptionStatus { get; set; }
    public bool SubscriptionCancelAtPeriodEnd { get; set; }
    
    public string? ProviderCustomerId { get; set; }
    public string? ProviderSubscriptionId { get; set; }
    
    public bool RemindersEnabled { get; set; }
    public int ReminderHoursBefore { get; set; }
    
    public List<User> Users { get; set; } = new();
    public List<Groomer> Groomers { get; set; } = new();
    public List<DogOwner> DogOwners { get; set; } = new();
    public List<Dog> Dogs { get; set; } = new();
    public List<Service> Services { get; set; } = new();
    public List<ServiceBreed> ServiceBreeds { get; set; } = new();
    public List<Visit> Visits { get; set; } = new ();
    public List<Blacklist> Blacklists { get; set; } = new ();
    public List<Waitlist> Waitlists { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
    public List<GroomerSchedule> GroomerSchedules { get; set; } = new();
    public List<GroomerTimeOff> GroomerTimeOffs { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();



}