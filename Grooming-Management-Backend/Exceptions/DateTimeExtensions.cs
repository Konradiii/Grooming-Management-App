namespace Grooming_Management_App.Exceptions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Oznacza datę jako UTC bez zmiany wartości.
    /// Potrzebne, bo serializacja z query stringu gubi DateTimeKind,
    /// a Npgsql wymaga jawnego UTC dla kolumn timestamptz.
    /// </summary>
    public static DateTime AsUtc(this DateTime value)
        => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    public static DateTime? AsUtc(this DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
}