namespace Grooming_Management_App.DTOs.BlacklistDto;

public class CreateBlacklistByDogOwnerDto
{
    public string Reason { get; set; }
    public int DogOwnerId { get; set; }
}