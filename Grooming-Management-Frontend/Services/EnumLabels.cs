using Grooming_Management_App.Enums;
using MudBlazor;

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
    
    public static Color StatusColor(StatusEnum status) => status switch
    {
        StatusEnum.Completed => Color.Success,
        StatusEnum.Cancelled => Color.Default,
        StatusEnum.NoShow => Color.Error,
        _ => Color.Primary
    };
    
    public static string Duration(int minutes)
    {
        if (minutes < 60)
            return $"{minutes} min";

        var hours = minutes / 60;
        var rest = minutes % 60;

        return rest == 0 ? $"{hours} h" : $"{hours} h {rest} min";
    }
}