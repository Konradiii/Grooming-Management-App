using System.Net;
using System.Net.Http.Json;
using Grooming_Management_App.DTOs.AuthDTO;
using Microsoft.AspNetCore.Mvc;

namespace Grooming_Management_Frontend.Services;

public class ApiClient(IHttpClientFactory factory, TokenStore tokenStore)
{
    public async Task<T?> GetAsync<T>(string url)
    {
        var response = await SendAsync(() =>
        {
            var client = factory.CreateClient("Api");
            tokenStore.ApplyTo(client);
            return client.GetAsync(url);
        });

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string url, T body)
    {
        return await SendAsync(() =>
        {
            var client = factory.CreateClient("Api");
            tokenStore.ApplyTo(client);
            return client.PostAsJsonAsync(url, body);
        });
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

        var tokens = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        if (tokens == null)
            return false;

        await tokenStore.SetTokensAsync(tokens.AccessToken, tokens.RefreshToken);
        return true;
    }
    public static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            if (!string.IsNullOrWhiteSpace(problem?.Title))
                return problem.Title;
        }
        catch
        {
            // odpowiedź nie jest ProblemDetails — spadamy do komunikatu ogólnego
        }

        return "Wystąpił błąd. Spróbuj ponownie.";
    }
}