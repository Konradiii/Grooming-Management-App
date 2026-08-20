using Grooming_Management_Frontend.Services;
using Microsoft.AspNetCore.Components;

namespace Grooming_Management_Frontend.Components;

public abstract class AuthenticatedPage : ComponentBase
{
    [Inject] protected TokenStore TokenStore { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !TokenStore.IsLoggedIn)
        {
            await TokenStore.LoadFromStorageAsync();
            StateHasChanged();
        }
    }
}