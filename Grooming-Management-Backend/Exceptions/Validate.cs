namespace Grooming_Management_App.Exceptions;

public static class Validate
{
    public static void NotEmpty(string? value, string errorCode)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ConflictException(errorCode);
    }

    public static void Email(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ConflictException(ErrorCodes.InvalidEmail);

        var trimmed = value.Trim();
        var at = trimmed.IndexOf('@');

        if (at <= 0 || at != trimmed.LastIndexOf('@'))
            throw new ConflictException(ErrorCodes.InvalidEmail);

        var domain = trimmed[(at + 1)..];

        if (!domain.Contains('.') || domain.StartsWith('.') || domain.EndsWith('.'))
            throw new ConflictException(ErrorCodes.InvalidEmail);

        if (trimmed.Contains(' '))
            throw new ConflictException(ErrorCodes.InvalidEmail);
    }

    public static void PolishPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ConflictException(ErrorCodes.InvalidPhone);

        var digits = new string(value.Where(char.IsDigit).ToArray());

        // 9 cyfr (123456789) albo 11 z prefiksem 48 (48123456789)
        var isValid = digits.Length == 9
                      || (digits.Length == 11 && digits.StartsWith("48"));

        if (!isValid)
            throw new ConflictException(ErrorCodes.InvalidPhone);
    }
}