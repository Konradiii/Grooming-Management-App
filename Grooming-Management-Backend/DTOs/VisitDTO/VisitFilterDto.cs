using Grooming_Management_App.Enums;

namespace Grooming_Management_App.DTOs.VisitDTO;

public class VisitFilterDto
{
    public StatusEnum? Status { get; set; }
    public int? GroomerId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}