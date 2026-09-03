using Grooming_Management_App.DTOs.SalonDTO;
using Grooming_Management_Frontend.Services;
using Microsoft.AspNetCore.Components;

namespace Grooming_Management_Frontend.Components;

public abstract class AuthenticatedPage : ComponentBase
{
    [Inject] protected TokenStore TokenStore { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected ApiClient Api { get; set; } = default!;

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

        if (TokenStore.RequiresPasswordChange)
        {
            Navigation.NavigateTo("/zmiana-hasla");
            return;
        }

        await LoadSmsBalanceAsync();

        StateHasChanged();
    }

    // Saldo pokazuje pasek w MainLayout. Cichy błąd jest tu w porządku -
    // brak licznika nie może blokować wejścia na stronę.
    private async Task LoadSmsBalanceAsync()
    {
        try
        {
            var balance = await Api.GetAsync<GetSmsBalanceDto>("api/Salon/sms-balance");

            if (balance != null)
            {
                TokenStore.SetSmsRemaining(balance.Remaining);
            }
        }
        catch
        {
            // ignorujemy
        }
    }
}