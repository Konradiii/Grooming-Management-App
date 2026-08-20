using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Models;

public class GroomerSchedule
{
    public int Id { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    
    public int GroomerId { get; set; }
    public Groomer Groomer { get; set; }
}