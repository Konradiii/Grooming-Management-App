using Grooming_Management_Frontend.Services;
using Microsoft.AspNetCore.Components;

namespace Grooming_Management_Frontend.Components;

public abstract class AuthenticatedPage : ComponentBase
{
    [Inject] protected TokenStore TokenStore { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        if (!TokenStore.IsLoggedIn)
        {
            await TokenStore.LoadFromStorageAsync();
        }

        if (!TokenStore.IsLoggedIn)
        {
            Navigation.NavigateTo("/login", forceLoad: false);
            return;
        }

        StateHasChanged();
    }
}