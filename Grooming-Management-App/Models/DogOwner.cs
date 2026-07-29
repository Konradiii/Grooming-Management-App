namespace Grooming_Management_App.Models;

public class DogOwner
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone  { get; set; }
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    
    public List<Dog> Dogs { get; set; } 
    
    public List<Visit> Visits { get; set; } = new ();

}