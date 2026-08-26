namespace Grooming_Management_Frontend.Components;

public class OwnerOnlyPage : AuthenticatedPage
{
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender) return;
        if (!TokenStore.IsLoggedIn) return;
        if (TokenStore.RequiresPasswordChange) return;

        if (!TokenStore.IsOwner)
        {
            Navigation.NavigateTo("/");
            return;
        }

        StateHasChanged();
    }
}
    
