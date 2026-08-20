using Blazored.LocalStorage;

namespace Grooming_Management_Frontend.Services;

public class TokenStore(ILocalStorageService localStorage)
{
    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }

    public bool IsLoggedIn => AccessToken != null;

    public async Task SetTokensAsync(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;

        await localStorage.SetItemAsStringAsync(AccessTokenKey, accessToken);
        await localStorage.SetItemAsStringAsync(RefreshTokenKey, refreshToken);
    }

    public async Task LoadFromStorageAsync()
    {
        AccessToken = await localStorage.GetItemAsStringAsync(AccessTokenKey);
        RefreshToken = await localStorage.GetItemAsStringAsync(RefreshTokenKey);
    }

    public async Task ClearAsync()
    {
        AccessToken = null;
        RefreshToken = null;

        await localStorage.RemoveItemAsync(AccessTokenKey);
        await localStorage.RemoveItemAsync(RefreshTokenKey);
    }
}