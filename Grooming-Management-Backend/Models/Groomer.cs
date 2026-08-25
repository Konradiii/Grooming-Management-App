using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Models;

public class Groomer
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public SettlementTypeEnum SettlementType { get; set; }
    public decimal SettlementRate { get; set; }
    
    public ActiveStatusEnum ActiveStatus  { get; set; }
    public bool CanSeeAllVisits { get; set; }
    public bool CanCreateVisits { get; set; }
    
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    
    public int? UserId { get; set; }
    public User? User { get; set; }
    
    public List<Visit> Visits { get; set; } = new ();
    public List<GroomerSchedule> GroomerSchedules { get; set; } = new();
    public List<GroomerTimeOff> GroomerTimeOffs { get; set; } = new();


}