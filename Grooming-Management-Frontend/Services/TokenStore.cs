using System.Text.Json;
using Blazored.LocalStorage;

namespace Grooming_Management_Frontend.Services;

public class TokenStore(ILocalStorageService localStorage)
{
    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";
    private const string RequiresPasswordChangeKey = "requiresPasswordChange";
    

    public event Action? OnChange;

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public string? Role { get; private set; }
    public string? FullName { get; private set; }
    public bool RequiresPasswordChange { get; private set; }

    public bool IsLoggedIn => AccessToken != null;
    public bool IsOwner => Role == "Owner";
    
    public string? SubscriptionStatus { get; private set; }

    public bool IsSuspended => SubscriptionStatus == "Suspended";
    public bool IsPastDue => SubscriptionStatus == "PastDue";
    public bool IsTrial => SubscriptionStatus == "Trial";
    public int? SmsRemaining { get; private set; }

    public bool IsSmsLow => SmsRemaining is > 0 and <= 20;
    public bool IsSmsEmpty => SmsRemaining == 0;

    
    private int? _dismissedAt;

    public bool SmsWarningDismissed => _dismissedAt == SmsRemaining;

    public void DismissSmsWarning()
    {
        _dismissedAt = SmsRemaining;
        NotifyChanged();
    }
    
    public void SetSmsRemaining(int remaining)
    {
        if (SmsRemaining == remaining) return;

        SmsRemaining = remaining;
        NotifyChanged();
    }

    public async Task SetTokensAsync(string accessToken, string refreshToken, bool requiresPasswordChange = false)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        RequiresPasswordChange = requiresPasswordChange;

        await localStorage.SetItemAsStringAsync(AccessTokenKey, accessToken);
        await localStorage.SetItemAsStringAsync(RefreshTokenKey, refreshToken);
        await localStorage.SetItemAsync(RequiresPasswordChangeKey, requiresPasswordChange);

        ReadClaimsFromToken();
        NotifyChanged();
    }

    public async Task LoadFromStorageAsync()
    {
        AccessToken = await localStorage.GetItemAsStringAsync(AccessTokenKey);
        RefreshToken = await localStorage.GetItemAsStringAsync(RefreshTokenKey);
        RequiresPasswordChange = await localStorage.GetItemAsync<bool>(RequiresPasswordChangeKey);

        ReadClaimsFromToken();
        NotifyChanged();
    }

    public async Task ClearAsync()
    {
        AccessToken = null;
        RefreshToken = null;
        Role = null;
        FullName = null;
        SubscriptionStatus = null;
        RequiresPasswordChange = false;
        SmsRemaining = null;
        _dismissedAt = null;
        
        await localStorage.RemoveItemAsync(AccessTokenKey);
        await localStorage.RemoveItemAsync(RefreshTokenKey);
        await localStorage.RemoveItemAsync(RequiresPasswordChangeKey);

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

    private void ReadClaimsFromToken()
    {
        Role = null;
        FullName = null;
        SubscriptionStatus = null;
        

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
                }
                else if (prop.Name == "fullName")
                {
                    var value = prop.Value.GetString();
                    FullName = string.IsNullOrWhiteSpace(value) ? null : value;
                }
                else if (prop.Name == "subscriptionStatus")
                {
                    var value = prop.Value.GetString();
                    SubscriptionStatus = string.IsNullOrWhiteSpace(value) ? null : value;
                }
                
            }
        }
        catch
        {
            // token nieczytelny — rola i nazwa zostają null
        }
    }
}