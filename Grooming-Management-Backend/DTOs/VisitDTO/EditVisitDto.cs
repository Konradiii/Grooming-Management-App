namespace Grooming_Management_App.DTOs.VisitDTO;


public class EditVisitDto
{
    public DateTime Date { get; set; }
    public int GroomerId { get; set; }
    public int DurationMinutes { get; set; }
    public int? ServiceBreedId { get; set; }
    public decimal ProposedPrice { get; set; }
    public string? Notes { get; set; }
    public bool IgnoreOverlap { get; set; }
}