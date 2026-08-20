namespace Grooming_Management_App.DTOs.ServiceBreedDTO;

public class CreateServiceBreedDto
{
    public decimal Price { get; set; }
    public int Duration { get; set; }
    public int ServiceId { get; set; }
    public int BreedId { get; set; }
}