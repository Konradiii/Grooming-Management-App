using System.Net.Http.Headers;

namespace Grooming_Management_Frontend.Services;

public class AuthTokenHandler(TokenStore tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        if (tokenStore.AccessToken != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenStore.AccessToken);
        }

        return await base.SendAsync(request, ct);
    }
}