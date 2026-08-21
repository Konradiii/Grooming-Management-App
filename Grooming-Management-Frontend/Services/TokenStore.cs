using System.Text.Json;
using Blazored.LocalStorage;

namespace Grooming_Management_Frontend.Services;

public class TokenStore(ILocalStorageService localStorage)
{
    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";

    public event Action? OnChange;

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public string? Role { get; private set; }

    public bool IsLoggedIn => AccessToken != null;
    public bool IsOwner => Role == "Owner";

    public async Task SetTokensAsync(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;

        await localStorage.SetItemAsStringAsync(AccessTokenKey, accessToken);
        await localStorage.SetItemAsStringAsync(RefreshTokenKey, refreshToken);

        ReadRoleFromToken();
        NotifyChanged();
    }

    public async Task LoadFromStorageAsync()
    {
        AccessToken = await localStorage.GetItemAsStringAsync(AccessTokenKey);
        RefreshToken = await localStorage.GetItemAsStringAsync(RefreshTokenKey);

        ReadRoleFromToken();
        NotifyChanged();
    }

    public async Task ClearAsync()
    {
        AccessToken = null;
        RefreshToken = null;
        Role = null;

        await localStorage.RemoveItemAsync(AccessTokenKey);
        await localStorage.RemoveItemAsync(RefreshTokenKey);

        NotifyChanged();
    }

    public void ApplyTo(HttpClient client)
    {
        if (AccessToken != null)
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);
        }
    }

    private void NotifyChanged() => OnChange?.Invoke();

    private void ReadRoleFromToken()
    {
        Role = null;

        if (AccessToken == null) return;

        var parts = AccessToken.Split('.');
        if (parts.Length < 2) return;

        try
        {
            var payload = parts[1];
            var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=')
                .Replace('-', '+').Replace('_', '/');

            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            using var doc = JsonDocument.Parse(json);

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Name.EndsWith("/role") || prop.Name == "role")
                {
                    Role = prop.Value.GetString();
                    return;
                }
            }
        }
        catch
        {
            // token nieczytelny — rola zostaje null
        }
    }
}