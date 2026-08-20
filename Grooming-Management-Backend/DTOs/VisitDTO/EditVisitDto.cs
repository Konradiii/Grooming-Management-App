namespace Grooming_Management_App.DTOs.VisitDTO;

public class EditVisitDto
{
    public DateTime Date { get; set; }
    public int GroomerId { get; set; }
    public string? Notes { get; set; }
}