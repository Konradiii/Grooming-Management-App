namespace Grooming_Management_App.DTOs.DogDTO;

public class CreateDogWithOwnerDto
{
    // właściciel
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }

    // pies
    public string Name { get; set; }
    public int AgeInMonths { get; set; }
    public int BreedId { get; set; }
    public string? Notes { get; set; }
}