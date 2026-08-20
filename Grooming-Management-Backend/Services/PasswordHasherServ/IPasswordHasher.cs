namespace Grooming_Management_App.Services.PasswordHasherServ;

public interface IPasswordHasher
{
    string HashPassword(string password);
    
    bool VerifyHashedPassword(string providedPassword, string hashedPassword);

}