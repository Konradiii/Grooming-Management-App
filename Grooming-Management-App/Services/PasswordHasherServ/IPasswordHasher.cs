namespace Grooming_Management_App.Services.PasswordHasherServ;

public interface IPasswordHasher
{
    string HashPassword(string password);
    
    bool VerifyHashedPasswordAsync(string providedPassword, string hashedPassword);

}