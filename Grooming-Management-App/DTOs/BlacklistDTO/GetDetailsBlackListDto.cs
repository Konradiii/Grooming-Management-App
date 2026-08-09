namespace Grooming_Management_App.DTOs.BlacklistDto;

public class GetDetailsBlackListDto
{
    public int Id { get; set; }
    public string DogOwnerFullName { get; set; }
    public string? DogName { get; set; }
    public string Reason { get; set; }
    public DateTime CreatedAt { get; set; }

}