using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Grooming_Management_App.DTOs.AuthDTO;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_Frontend.Services;

public class ApiClient(IHttpClientFactory factory, TokenStore tokenStore)
{
    
    public async Task<bool> RefreshTokenAsync() => await TryRefreshAsync();
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    
    

    public async Task<T?> GetAsync<T>(string url)
    {
        var response = await SendAsync(() =>
        {
            var client = factory.CreateClient("Api");
            tokenStore.ApplyTo(client);
            return client.GetAsync(url);
        });
        
        if (!response.IsSuccessStatusCode)
            return default;

        if (response.StatusCode == HttpStatusCode.NoContent)
            return default;

        var json = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string url, T body)
    {
        return await SendAsync(() =>
        {
            var client = factory.CreateClient("Api");
            tokenStore.ApplyTo(client);
            return client.PostAsJsonAsync(url, body, JsonOptions);
        });
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string url, T body)
    {
        return await SendAsync(() =>
        {
            var client = factory.CreateClient("Api");
            tokenStore.ApplyTo(client);
            return client.PutAsJsonAsync(url, body, JsonOptions);
        });
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await SendAsync(() =>
        {
            var client = factory.CreateClient("Api");
            tokenStore.ApplyTo(client);
            return client.DeleteAsync(url);
        });
    }

    public static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            if (!string.IsNullOrWhiteSpace(problem?.Title))
                return ErrorMessages.Translate(problem.Title);
        }
        catch
        {
            // odpowiedź nie jest ProblemDetails
        }

        return "Wystąpił błąd. Spróbuj ponownie.";
    }

    private async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        var response = await send();

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        var refreshed = await TryRefreshAsync();
        if (!refreshed)
            return response;

        return await send();
    }

    private async Task<bool> TryRefreshAsync()
    {
        if (tokenStore.RefreshToken == null)
            return false;

        var client = factory.CreateClient("Api");

        var response = await client.PostAsJsonAsync(
            $"api/Auth/RefreshToken?refreshToken={Uri.EscapeDataString(tokenStore.RefreshToken)}",
            new { });

        if (!response.IsSuccessStatusCode)
        {
            await tokenStore.ClearAsync();
            return false;
        }

        var tokens = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);
        if (tokens == null)
            return false;

        await tokenStore.SetTokensAsync(tokens.AccessToken, tokens.RefreshToken, tokens.RequiresPasswordChange);
        return true;
    }
    
}