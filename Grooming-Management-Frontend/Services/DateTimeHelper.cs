namespace Grooming_Management_Frontend.Services;

public static class Dates
{
    private static readonly TimeZoneInfo PolishTime =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    /// Czas wpisany przez użytkownika (polski) → UTC do wysłania na backend
    public static DateTime ToUtc(DateTime polish)
        => TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(polish, DateTimeKind.Unspecified), PolishTime);

    /// UTC z backendu → czas polski do wyświetlenia
    public static DateTime ToLocal(DateTime utc)
        => TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utc, DateTimeKind.Utc), PolishTime);

    /// UTC → sformatowany tekst w czasie polskim
    public static string Format(DateTime utc, string format = "dd.MM.yyyy HH:mm")
        => ToLocal(utc).ToString(format);
}