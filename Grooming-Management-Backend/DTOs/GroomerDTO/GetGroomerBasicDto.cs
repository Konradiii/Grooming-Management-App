using Grooming_Management_App.Enums;

public class GetGroomerBasicDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public ActiveStatusEnum ActiveStatus { get; set; }
    public bool CanCreateVisits { get; set; }
    
}