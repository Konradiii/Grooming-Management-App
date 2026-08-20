using Grooming_Management_App.Enums;

namespace Grooming_Management_App.DTOs.GroomerScheduleDTO;

public class UpdateGroomerScheduleDto
{
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    public int GroomerId { get; set; }
}