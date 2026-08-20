using Grooming_Management_App.Enums;

namespace Grooming_Management_App.DTOs.ServiceDTO;

public class GetServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public ActiveStatusEnum Status { get; set; }
}