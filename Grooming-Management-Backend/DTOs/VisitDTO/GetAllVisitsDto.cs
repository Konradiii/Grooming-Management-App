using Grooming_Management_App.Enums;

namespace Grooming_Management_App.DTOs.VisitDTO;

public class GetAllVisitsDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string DogName { get; set; }
    public string GroomerName { get; set; }
    public string ServiceName { get; set; }
    public StatusEnum Status { get; set; }
    public int GroomerId { get; set; }
    public int EstimatedDuration { get; set; }
    public string BreedName { get; set; }
}