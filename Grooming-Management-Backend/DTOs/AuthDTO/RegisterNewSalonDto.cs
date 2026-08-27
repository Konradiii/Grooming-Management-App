namespace Grooming_Management_App.DTOs.AuthDTO;

public class RegisterNewSalonDto
{
    public string SalonName { get; set; }
    
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public string? ApartmentNumber { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string Phone { get; set; }
    
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}