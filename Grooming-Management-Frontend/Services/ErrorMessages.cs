namespace Grooming_Management_Frontend.Services;

public static class ErrorMessages
{
    private static readonly Dictionary<string, string> Translations = new()
    {
        // Nie znaleziono
        ["DOG_NOT_FOUND"] = "Nie znaleziono psa",
        ["DOG_OWNER_NOT_FOUND"] = "Nie znaleziono klienta",
        ["VISIT_NOT_FOUND"] = "Nie znaleziono wizyty",
        ["GROOMER_NOT_FOUND"] = "Nie znaleziono groomera",
        ["ASSISTANT_NOT_FOUND"] = "Nie znaleziono osoby do pomocy",
        ["SERVICE_NOT_FOUND"] = "Nie znaleziono usługi",
        ["SERVICE_BREED_NOT_FOUND"] = "Nie znaleziono pozycji cennika",
        ["BREED_NOT_FOUND"] = "Nie znaleziono rasy",
        ["USER_NOT_FOUND"] = "Nie znaleziono użytkownika",
        ["SCHEDULE_NOT_FOUND"] = "Nie znaleziono grafiku",
        ["TIME_OFF_NOT_FOUND"] = "Nie znaleziono blokady czasu",
        ["BLACKLIST_RECORD_NOT_FOUND"] = "Nie znaleziono wpisu na czarnej liście",
        ["WAITLIST_RECORD_NOT_FOUND"] = "Nie znaleziono wpisu na liście oczekujących",
        ["REFRESH_TOKEN_NOT_FOUND"] = "Sesja wygasła — zaloguj się ponownie",
        ["SALON_NOT_FOUND"] = "Nie znaleziono salonu",
        ["REMINDER_ALREADY_SENT"] = "Przypomnienie o tej wizycie zostało już wysłane",
        ["PAYMENT_ALREADY_PROCESSED"] = "Ta płatność została już zaksięgowana",

        // Duplikaty
        ["EMAIL_TAKEN"] = "Ten adres e-mail jest już zajęty",
        ["PHONE_TAKEN"] = "Klient z tym numerem telefonu już istnieje",
        ["SERVICE_NAME_TAKEN"] = "Usługa o tej nazwie już istnieje",
        ["SERVICE_BREED_COMBINATION_EXISTS"] = "Ta usługa jest już wyceniona dla tej rasy",
        ["GROOMER_ALREADY_HAS_ACCOUNT"] = "Ten groomer ma już konto",

        // Reguły biznesowe
        ["BREED_MISMATCH"] = "Ta usługa nie jest wyceniona dla rasy tego psa",
        ["CLIENT_BLACKLISTED"] = "Ten klient jest na czarnej liście",
        ["DOG_ALREADY_BLACKLISTED"] = "Ten pies jest już na czarnej liście",
        ["CLIENT_ALREADY_BLACKLISTED"] = "Ten klient jest już na czarnej liście",
        ["CLIENT_ALREADY_ON_WAITLIST"] = "Ten klient jest już na liście oczekujących",
        ["DUPLICATE_VISIT"] = "Ten pies ma już wizytę w tym terminie",
        ["VISIT_OVERLAPS"] = "Groomer ma już wizytę w tym czasie",
        ["GROOMER_UNAVAILABLE"] = "Groomer jest niedostępny w tym czasie",
        ["SCHEDULE_OVERLAPS"] = "Ten przedział nakłada się na istniejący grafik",
        ["TIME_OFF_HAS_VISITS"] = "W tym okresie są zaplanowane wizyty — najpierw je odwołaj lub przełóż",
        ["ASSISTANT_MUST_DIFFER"] = "Pomoc musi być inną osobą niż prowadzący",
        ["INVALID_DURATION"] = "Czas trwania musi być większy od zera",
        ["INVALID_TIME_RANGE"] = "Godzina rozpoczęcia musi być wcześniejsza niż zakończenia",
        ["INVALID_DATE_RANGE"] = "Data początkowa nie może być późniejsza niż końcowa",
        ["PASSWORDS_DO_NOT_MATCH"] = "Hasła nie są takie same",
        ["NOTIFICATION_ALREADY_SENT"] = "Powiadomienie dla tej wizyty zostało już wysłane",
        ["CANNOT_NOTIFY_CANCELLED_VISIT"] = "Nie można wysłać powiadomienia dla odwołanej wizyty",
        ["INVALID_BOOKING_SETTINGS"] = "Nieprawidłowe ustawienia rezerwacji",

        // Uwierzytelnianie
        ["INVALID_CREDENTIALS"] = "Nieprawidłowy e-mail lub hasło",
        ["USER_INACTIVE"] = "To konto zostało dezaktywowane",
        ["REFRESH_TOKEN_REVOKED"] = "Sesja wygasła — zaloguj się ponownie",
        ["REFRESH_TOKEN_EXPIRED"] = "Sesja wygasła — zaloguj się ponownie",
        ["INVALID_PASSWORD"] = "Nieprawidłowe hasło",

        // Uprawnienia
        ["NO_PERMISSION_TO_CREATE_VISITS"] = "Nie masz uprawnień do dodawania wizyt",
        ["NOTIFICATION_ALREADY_SENT"] = "Powiadomienie o gotowości do odbioru zostało już wysłane",
    };

    public static string Translate(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return "Wystąpił nieoczekiwany błąd";

        return Translations.TryGetValue(code, out var text) ? text : code;
    }
}