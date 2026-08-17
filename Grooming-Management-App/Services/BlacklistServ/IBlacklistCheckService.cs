namespace Grooming_Management_App.Services.BlacklistServ;

public interface IBlacklistCheckService
{
    Task<bool> IsBlockedAsync(int salonId, int dogOwnerId, int? dogId, CancellationToken ct);

}