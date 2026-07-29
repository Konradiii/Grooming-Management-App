namespace Grooming_Management_App.Models;

public class ServiceBreed
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    
    public int ServiceId { get; set; }
    public Service Service { get; set; }
    
    public int BreedId { get; set; }
    public Breed Breed { get; set; }
    
    public List<Visit> Visits { get; set; } = new ();
}