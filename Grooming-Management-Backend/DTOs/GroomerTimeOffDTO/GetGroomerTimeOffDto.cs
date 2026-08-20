namespace Grooming_Management_App.DTOs.GroomerTimeOffDTO;

public class GetGroomerTimeOffDto
{
    public int Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Reason { get; set; }
    public int GroomerId { get; set; }
    public string GroomerFullName { get; set; }
}