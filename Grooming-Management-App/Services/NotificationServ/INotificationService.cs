namespace Grooming_Management_App.Services.NotificationServ;

public interface INotificationService
{
    Task SendReadyForPickupNotificationAsync(int salonId, int visitId, int timeToPickUpDogInMin, CancellationToken ct);
    
    Task SendVisitReminderAsync(int salonId, int visitId, CancellationToken ct);
}