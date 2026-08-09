using Grooming_Management_App.Enums;

namespace Grooming_Management_App.Models;

public class Notification
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; }
    public NotificationTypeEnum Type { get; set; }
    public NotificationStatusEnum Status { get; set; }
    public string MessageText { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime? SentAt { get; set; }
    public int AttemptCount { get; set; }
    public string? ErrorMessage { get; set; }
    
    public int SalonId { get; set; }
    public Salon Salon { get; set; }
    
    public int VisitId { get; set; }
    public Visit Visit { get; set; }
    
    public int DogOwnerId { get; set; }
    public DogOwner DogOwner { get; set; }
}   