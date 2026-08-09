using Grooming_Management_App.DataInfrastructure;
using Grooming_Management_App.Enums;
using Grooming_Management_App.Exceptions;
using Grooming_Management_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Grooming_Management_App.Services.NotificationServ;

public class NotificationService(GroomingDbContext ctx, ISmsService smsService) : INotificationService
{
    public async Task SendReadyForPickupNotificationAsync(int salonId, int visitId, int timeToPickUpDogInMin, CancellationToken ct)
    {

        var visit = await ctx.Visits
            .Include(v => v.Dog)
            .Include(v => v.DogOwner)
            .Where(v => v.Id == visitId)
            .Where(v => v.SalonId == salonId)
            .FirstOrDefaultAsync(ct);

        if (visit == null)
        {
            throw new NotFoundException("Visit not found");
        }

        var dogName = visit.Dog.Name;

        string msg = $"Państwa pies {dogName} jest gotowy do odbioru. Zapraszamy za {timeToPickUpDogInMin} min";

        var phoneNumber = visit.DogOwner.Phone;
        
        var response = await smsService.SendSmsAsync(phoneNumber , msg, ct);

        var notification = new Notification
        {
          
            SalonId =  salonId,
            VisitId =  visitId,
            DogOwnerId = visit.DogOwnerId,
            PhoneNumber =  phoneNumber,
            Type = NotificationTypeEnum.ManualReady,
            Status = response.Success ? NotificationStatusEnum.Sent : NotificationStatusEnum.Failed,
            MessageText = msg,
            ScheduledTime =  DateTime.UtcNow,
            SentAt = response.Success ? DateTime.UtcNow : null,
            AttemptCount = 1,
            ErrorMessage = response.ErrorMessage,
        };
        
        ctx.Notifications.Add(notification);
        await ctx.SaveChangesAsync(ct);



    }

}