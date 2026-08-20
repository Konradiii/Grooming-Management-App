namespace Grooming_Management_App.DTOs.DogDTO;

public class UpdateDogDto
{
    public string Name { get; set; }
    public int? AgeInMonths { get; set; } 
    public string? Notes { get; set; }
    public int BreedId { get; set; }
    public int DogOwnerId { get; set; }
}