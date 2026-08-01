namespace Grooming_Management_App.DTOs.VisitDTO;

public class AddVisitDto
{
    public DateTime Date { get; set; }
    public int DogId { get; set; }
    public int GroomerId { get; set; }
    public int ServiceBreedId { get; set; }
    public string? Notes { get; set; }
}