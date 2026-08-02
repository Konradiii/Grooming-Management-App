namespace Grooming_Management_App.DTOs.AuthDTO;

public class RegisterNewSalonDto
{
    public string SalonName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}