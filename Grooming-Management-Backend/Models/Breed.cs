namespace Grooming_Management_App.Models;

public class Breed
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public List<Dog> Dogs { get; set; }
    public List<ServiceBreed> ServiceBreeds { get; set; } = new();

}