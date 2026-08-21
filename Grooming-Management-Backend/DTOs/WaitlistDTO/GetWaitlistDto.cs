namespace Grooming_Management_App.DTOs.WaitlistDTO;

public class GetWaitlistDto
{

        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Priority { get; set; }
        public string DogOwnerFullName { get; set; }
        public string? DogName { get; set; }
        public string DogOwnerPhone { get; set; }
        
}