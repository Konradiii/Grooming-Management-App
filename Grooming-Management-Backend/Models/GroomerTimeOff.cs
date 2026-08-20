namespace Grooming_Management_App.Models;

public class GroomerTimeOff
{
    public int Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    
    public int GroomerId { get; set; }
    public Groomer Groomer { get; set; }
}