using Grooming_Management_App.Enums;

namespace Grooming_Management_App.DTOs.GroomerDTO;

public class EditGroomerDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public SettlementTypeEnum SettlementType { get; set; }
    public decimal SettlementRate { get; set; }
    public bool CanSeeAllVisits { get; set; }
    public bool CanCreateVisits { get; set; }

}