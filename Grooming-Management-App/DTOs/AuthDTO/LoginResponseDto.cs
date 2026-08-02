namespace Grooming_Management_App.DTOs.AuthDTO;

public class LoginResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}