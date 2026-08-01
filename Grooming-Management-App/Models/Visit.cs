using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Models;

public class Visit
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime Date { get; set; }
    public int EstimatedDuration { get; set; }
    public decimal ProposedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public StatusEnum Status { get; set; }
    public string? Notes { get; set; }
    
    //
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    
    public int DogId { get; set; }
    public Dog Dog { get; set; }
    
    public int DogOwnerId { get; set; }
    public DogOwner DogOwner { get; set; }
    
    public int GroomerId { get; set; }
    public Groomer Groomer { get; set; }
    
    public int ServiceBreedId { get; set; }
    public ServiceBreed ServiceBreed { get; set; }
    
    
    
}