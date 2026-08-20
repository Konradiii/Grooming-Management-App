using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public RoleEnum Role { get; set; }
    public ActiveStatusEnum ActiveStatus { get; set; }
    public bool RequiresPasswordChange { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }

    public Groomer? Groomer { get; set; }
    public DogOwner? DogOwner { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = new();

}