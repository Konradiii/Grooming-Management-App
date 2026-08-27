namespace Grooming_Management_App.DTOs.SalonDTO;

public class UpdateSalonDto
{
    public string Name { get; set; }
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public string? ApartmentNumber { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public int MinBookingHoursAhead { get; set; }
    public int MaxBookingDaysAhead { get; set; }
    public bool RemindersEnabled { get; set; }
    public int ReminderHoursBefore { get; set; }
}