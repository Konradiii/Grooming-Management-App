namespace Grooming_Management_App.DTOs.GroomerTimeOffDTO;

public class CreateGroomerTimeOffDto
{
    public int GroomerId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Reason { get; set; }
}