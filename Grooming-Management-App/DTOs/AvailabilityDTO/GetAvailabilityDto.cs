namespace Grooming_Management_App.DTOs.AvailabilityDTO;

public class GetAvailabilityDto
{
    public List<string> AvailableSlots { get; set; } = new();
    public int? GroomerId { get; set; }
    public string? GroomerFullName { get; set; }
    public DateOnly Date { get; set; }
    public int ServiceDurationMinutes { get; set; }
}