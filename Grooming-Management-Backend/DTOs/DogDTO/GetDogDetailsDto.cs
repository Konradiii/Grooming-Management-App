namespace Grooming_Management_App.DTOs.DogDTO;

public class GetDogDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? AgeInMonths { get; set; } 
    public string? Notes { get; set; }
    public string DogOwnerFullName { get; set; }
    public string BreedName { get; set; }
    
}