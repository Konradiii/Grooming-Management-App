namespace Grooming_Management_App.Enums;

public enum SubscriptionStatusEnum
{
    Trial,      // okres próbny
    Active,     // opłacone
    PastDue,    // wygasło, trwa tydzień karencji
    Suspended   // karencja minęła, dostęp odcięty
}