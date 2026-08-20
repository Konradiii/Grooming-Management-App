using Grooming_Management_App.Enums;

namespace Grooming_Management_App.DTOs.GroomerDTO;

public class GetGroomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public ActiveStatusEnum ActiveStatus { get; set; }
}