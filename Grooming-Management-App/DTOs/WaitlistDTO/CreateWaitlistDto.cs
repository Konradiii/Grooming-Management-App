namespace Grooming_Management_App.DTOs.WaitlistDTO;

public class CreateWaitlistDto
{
    public int Priority { get; set; }
    public int DogOwnerId { get; set; }
    public int? DogId { get; set; }

}