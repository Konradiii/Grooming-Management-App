namespace Grooming_Management_Frontend.Services;

public class TokenStore
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }

    public bool IsLoggedIn => AccessToken != null;

    public void SetTokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
    }
}