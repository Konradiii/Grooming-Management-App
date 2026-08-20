using Grooming_Management_App.Enums;

namespace Grooming_Management_App.DTOs.ServiceBreedDTO;

public class GetServiceBreedDto
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
    public string ServiceName { get; set; }
    public string BreedName { get; set; }
    public ActiveStatusEnum Status { get; set; }

}