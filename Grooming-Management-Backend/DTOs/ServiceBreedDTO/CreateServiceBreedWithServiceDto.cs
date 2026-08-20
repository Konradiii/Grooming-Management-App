namespace Grooming_Management_App.DTOs.ServiceBreedDTO;

public class CreateServiceBreedWithServiceDto
{
    public string ServiceName { get; set; }
    public int BreedId { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
}