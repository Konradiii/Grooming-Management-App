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
        
        var alreadySent = await ctx.Notifications
            .AnyAsync(n => n.VisitId == visitId 
                           && n.SalonId == salonId 
                           && n.Type == NotificationTypeEnum.ManualReady 
                           && n.Status == NotificationStatusEnum.Sent, ct);
        
        if (alreadySent)
        {
            throw new ConflictException("Ready-for-pickup notification was already sent for this visit");
        }

        var visit = await ctx.Visits
            .Include(v => v.Dog)
            .Include(v => v.DogOwner)
            .Include(v => v.Salon)
            .Where(v => v.Id == visitId)
            .Where(v => v.SalonId == salonId)
            .FirstOrDefaultAsync(ct);

        if (visit == null)
        {
            throw new NotFoundException("Visit not found");
        }
        
        if (visit.Status == StatusEnum.Cancelled || visit.Status == StatusEnum.NoShow)
        {
            throw new ConflictException("Cannot send ready-for-pickup notification for a cancelled or no-show visit");
        }

        var dogName = visit.Dog.Name;
        var nazwaSalonu = visit.Salon.Name;

        string msg = $"Państwa pies {dogName} jest gotowy do odbioru. Zapraszamy za {timeToPickUpDogInMin} min. Salon Groomerski - {nazwaSalonu} ";

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

    public async Task SendVisitReminderAsync(int salonId, int visitId, CancellationToken ct)
    {
        
        var alreadySent = await ctx.Notifications
            .AnyAsync(n => n.VisitId == visitId 
                           && n.SalonId == salonId 
                           && n.Type == NotificationTypeEnum.Automatic 
                           && n.Status == NotificationStatusEnum.Sent, ct);
        
        if (alreadySent)
        {
            throw new ConflictException("Visit reminder notification was already sent for this visit");
        }
        

        var visit = await ctx.Visits
            .IgnoreQueryFilters()
            .Include(v => v.Dog)
            .Include(v => v.DogOwner)
            .Include(v => v.Salon)
            .Where(v => v.Id == visitId)
            .Where(v => v.SalonId == salonId)
            .FirstOrDefaultAsync(ct);
        

            
        if (visit == null)
        {
            throw new NotFoundException("Visit not found");
        }


        
        var msg = $"Przypominamy o wizycie Państwa pupila {visit.Dog.Name}, dnia jutrzejszego: {visit.Date.Day} o godzinie {visit.Date:HH:mm} w salonie {visit.Salon.Name}. Do zobaczenia!";


        var phoneNumber = visit.DogOwner.Phone;
        
        var response = await smsService.SendSmsAsync(phoneNumber , msg, ct);

        var notification = new Notification
        {
            PhoneNumber = phoneNumber,
            Type = NotificationTypeEnum.Automatic,
            Status = response.Success ? NotificationStatusEnum.Sent : NotificationStatusEnum.Failed,
            MessageText = msg,
            ScheduledTime = DateTime.UtcNow,
            SentAt = response.Success ? DateTime.UtcNow : null,
            AttemptCount = 1,
            ErrorMessage = response.ErrorMessage,
            SalonId = salonId,
            VisitId = visitId,
            DogOwnerId = visit.DogOwnerId,

        };
        ctx.Notifications.Add(notification);
        await ctx.SaveChangesAsync(ct);
    }

}






















