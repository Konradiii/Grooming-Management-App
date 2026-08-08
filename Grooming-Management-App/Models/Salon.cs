namespace Grooming_Management_App.Models;

public class Salon
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public List<User> Users { get; set; } = new();
    public List<Groomer> Groomers { get; set; } = new();
    public List<DogOwner> DogOwners { get; set; } = new();
    public List<Dog> Dogs { get; set; } = new();
    public List<Service> Services { get; set; } = new();
    public List<ServiceBreed> ServiceBreeds { get; set; } = new();
    public List<Visit> Visits { get; set; } = new ();
    public List<Blacklist> Blacklists { get; set; } = new ();
}