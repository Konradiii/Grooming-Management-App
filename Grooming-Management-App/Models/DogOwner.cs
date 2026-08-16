namespace Grooming_Management_App.Models;

public class DogOwner
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone  { get; set; }
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    
    public int? UserId { get; set; }
    public User? User { get; set; }

    public List<Dog> Dogs { get; set; } = new();
    
    public List<Visit> Visits { get; set; } = new ();
    public List<Blacklist> Blacklists { get; set; } = new ();
    public List<Waitlist> Waitlists { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();


}