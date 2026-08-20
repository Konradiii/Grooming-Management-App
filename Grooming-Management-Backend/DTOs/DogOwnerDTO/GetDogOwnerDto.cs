namespace Grooming_Management_App.DTOs.DogOwnerDTO;

public class GetDogOwnerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    
    public int DogsCount { get; set; }
}