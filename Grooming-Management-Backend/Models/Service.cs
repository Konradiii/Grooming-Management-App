using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Models;

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public ActiveStatusEnum Status { get; set; }
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    public List<ServiceBreed> ServiceBreeds { get; set; } = new();

}