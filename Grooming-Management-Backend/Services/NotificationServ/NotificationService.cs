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
            throw new ConflictException(ErrorCodes.NotificationAlreadySent);
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
            throw new NotFoundException(ErrorCodes.VisitNotFound);
        }

        if (visit.Status == StatusEnum.Cancelled || visit.Status == StatusEnum.NoShow)
        {
            throw new ConflictException(ErrorCodes.CannotNotifyCancelledVisit);
        }

        var msg = $"Państwa pies {visit.Dog.Name} jest gotowy do odbioru. " +
                  $"Zapraszamy za {timeToPickUpDogInMin} min. Salon groomerski {visit.Salon.Name}";

        var phoneNumber = visit.DogOwner.Phone;

        var response = await smsService.SendSmsAsync(phoneNumber, msg, ct);

        var notification = new Notification
        {
            PhoneNumber = phoneNumber,
            Type = NotificationTypeEnum.ManualReady,
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

    public async Task SendVisitReminderAsync(int salonId, int visitId, CancellationToken ct)
    {
        var alreadySent = await ctx.Notifications
            .AnyAsync(n => n.VisitId == visitId
                           && n.SalonId == salonId
                           && n.Type == NotificationTypeEnum.Automatic
                           && n.Status == NotificationStatusEnum.Sent, ct);

        if (alreadySent)
        {
            throw new ConflictException(ErrorCodes.ReminderAlreadySent);
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
            throw new NotFoundException(ErrorCodes.VisitNotFound);
        }

        if (visit.Status == StatusEnum.Cancelled || visit.Status == StatusEnum.NoShow)
        {
            throw new ConflictException(ErrorCodes.CannotNotifyCancelledVisit);
        }

        var msg = $"Przypominamy o jutrzejszej wizycie Państwa pupila {visit.Dog.Name} " +
                  $"o godzinie {visit.Date:HH:mm} w salonie {visit.Salon.Name}. Do zobaczenia!";

        var phoneNumber = visit.DogOwner.Phone;

        var response = await smsService.SendSmsAsync(phoneNumber, msg, ct);

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