using Grooming_Management_App.Enums;

namespace Grooming_Management_Frontend.Services;

public static class EnumLabels
{
    public static string Status(StatusEnum status) => status switch
    {
        StatusEnum.Scheduled => "Zaplanowana",
        StatusEnum.Completed => "Ukończona",
        StatusEnum.Cancelled => "Odwołana",
        StatusEnum.NoShow => "Nie przyszedł",
        _ => status.ToString()
    };

    public static string ActiveStatus(ActiveStatusEnum status) => status switch
    {
        ActiveStatusEnum.Active => "Aktywny",
        ActiveStatusEnum.Inactive => "Nieaktywny",
        _ => status.ToString()
    };

    public static string DayOfWeek(DayOfWeekEnum day) => day switch
    {
        DayOfWeekEnum.Monday => "Poniedziałek",
        DayOfWeekEnum.Tuesday => "Wtorek",
        DayOfWeekEnum.Wednesday => "Środa",
        DayOfWeekEnum.Thursday => "Czwartek",
        DayOfWeekEnum.Friday => "Piątek",
        DayOfWeekEnum.Saturday => "Sobota",
        DayOfWeekEnum.Sunday => "Niedziela",
        _ => day.ToString()
    };

    public static string SubscriptionStatus(SubscriptionStatusEnum status) => status switch
    {
        SubscriptionStatusEnum.Trial => "Okres próbny",
        SubscriptionStatusEnum.Active => "Aktywna",
        SubscriptionStatusEnum.PastDue => "Zaległość",
        SubscriptionStatusEnum.Suspended => "Zawieszona",
        _ => status.ToString()
    };
}