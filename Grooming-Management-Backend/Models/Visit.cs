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
    
    public int? AssistantGroomerId { get; set; }
    public Groomer? AssistantGroomer { get; set; }
    
    public SettlementTypeEnum SettlementType { get; set; }
    public decimal SettlementRate { get; set; }
    
    //
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    
    public int DogId { get; set; }
    public Dog Dog { get; set; }
    
    public int DogOwnerId { get; set; }
    public DogOwner DogOwner { get; set; }
    
    public int GroomerId { get; set; }
    public Groomer Groomer { get; set; }
    
    // opcja B - pozycja cennika (usługa + rasa + cena)
    public int? ServiceBreedId { get; set; }
    public ServiceBreed? ServiceBreed { get; set; }
    
    // opcja A - sama usługa, cena wpisywana ręcznie
    public int? ServiceId { get; set; }
    public Service? Service { get; set; }
    
    public List<Notification> Notifications { get; set; } = new();
}