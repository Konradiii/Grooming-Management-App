namespace Grooming_Management_App.Models;

public class TimeSlot
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; }
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    
    public int GroomerId { get; set; }
    public Groomer Groomer { get; set; }
    
    public int? VisitId { get; set; }
    public Visit? Visit { get; set; }
}