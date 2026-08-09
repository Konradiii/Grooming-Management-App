namespace Grooming_Management_App.Models;

public class Dog
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? AgeInMonths { get; set; } 
    public string? Notes { get; set; }
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    
    public int BreedId { get; set; }
    public Breed Breed { get; set; }
    
    public int DogOwnerId { get; set; }
    public DogOwner DogOwner { get; set; }
    
    public List<Visit> Visits { get; set; } = new ();
    public List<Blacklist> Blacklists { get; set; } = new ();
    public List<Waitlist> Waitlists { get; set; } = new();


}