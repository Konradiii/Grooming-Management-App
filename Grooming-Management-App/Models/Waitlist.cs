namespace Grooming_Management_App.Models;

public class Waitlist
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Priority { get; set; }
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    
    public int DogOwnerId { get; set; }
    public DogOwner DogOwner { get; set; }
    
    public int? DogId { get; set; }
    public Dog? Dog { get; set; }
    
}