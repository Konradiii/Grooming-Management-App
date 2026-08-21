using Grooming_Management_App.Enums;

namespace Grooming_Management_App.DTOs.VisitDTO;

public class GetVisitDetailsDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime Date { get; set; }
    public int EstimatedDuration { get; set; }
    public decimal ProposedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public StatusEnum Status { get; set; }
    public string? Notes { get; set; }
    public string DogName { get; set; }
    public string DogOwnerFullName { get; set; }
    public string GroomerFullName { get; set; }
    public string ServiceName { get; set; }
    public string BreedName { get; set; }
    public string? AssistantGroomerFullName { get; set; }
}