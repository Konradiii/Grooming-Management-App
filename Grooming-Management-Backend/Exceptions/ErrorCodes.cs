namespace Grooming_Management_App.Exceptions;

public static class ErrorCodes
{
    // Not found
    public const string DogNotFound = "DOG_NOT_FOUND";
    public const string DogOwnerNotFound = "DOG_OWNER_NOT_FOUND";
    public const string VisitNotFound = "VISIT_NOT_FOUND";
    public const string GroomerNotFound = "GROOMER_NOT_FOUND";
    public const string AssistantNotFound = "ASSISTANT_NOT_FOUND";
    public const string ServiceNotFound = "SERVICE_NOT_FOUND";
    public const string ServiceBreedNotFound = "SERVICE_BREED_NOT_FOUND";
    public const string BreedNotFound = "BREED_NOT_FOUND";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string ScheduleNotFound = "SCHEDULE_NOT_FOUND";
    public const string TimeOffNotFound = "TIME_OFF_NOT_FOUND";
    public const string BlacklistRecordNotFound = "BLACKLIST_RECORD_NOT_FOUND";
    public const string WaitlistRecordNotFound = "WAITLIST_RECORD_NOT_FOUND";
    public const string RefreshTokenNotFound = "REFRESH_TOKEN_NOT_FOUND";
    public const string SalonNotFound = "SALON_NOT_FOUND";

    // Conflict — dane
    public const string EmailTaken = "EMAIL_TAKEN";
    public const string PhoneTaken = "PHONE_TAKEN";
    public const string ServiceNameTaken = "SERVICE_NAME_TAKEN";
    public const string ServiceBreedCombinationExists = "SERVICE_BREED_COMBINATION_EXISTS";
    public const string GroomerAlreadyHasAccount = "GROOMER_ALREADY_HAS_ACCOUNT";
    public const string ReminderAlreadySent = "REMINDER_ALREADY_SENT";
    public const string PaymentAlreadyProcessed = "PAYMENT_ALREADY_PROCESSED";

    // Conflict — reguły biznesowe
    public const string BreedMismatch = "BREED_MISMATCH";
    public const string ClientBlacklisted = "CLIENT_BLACKLISTED";
    public const string DogAlreadyBlacklisted = "DOG_ALREADY_BLACKLISTED";
    public const string ClientAlreadyBlacklisted = "CLIENT_ALREADY_BLACKLISTED";
    public const string ClientAlreadyOnWaitlist = "CLIENT_ALREADY_ON_WAITLIST";
    public const string DuplicateVisit = "DUPLICATE_VISIT";
    public const string VisitOverlaps = "VISIT_OVERLAPS";
    public const string GroomerUnavailable = "GROOMER_UNAVAILABLE";
    public const string ScheduleOverlaps = "SCHEDULE_OVERLAPS";
    public const string TimeOffHasVisits = "TIME_OFF_HAS_VISITS";
    public const string AssistantMustDiffer = "ASSISTANT_MUST_DIFFER";
    public const string InvalidTimeRange = "INVALID_TIME_RANGE";
    public const string InvalidDateRange = "INVALID_DATE_RANGE";
    public const string PasswordsDoNotMatch = "PASSWORDS_DO_NOT_MATCH";
    public const string NotificationAlreadySent = "NOTIFICATION_ALREADY_SENT";
    public const string CannotNotifyCancelledVisit = "CANNOT_NOTIFY_CANCELLED_VISIT";
    public const string InvalidBookingSettings = "INVALID_BOOKING_SETTINGS";

    // Unauthorized
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string UserInactive = "USER_INACTIVE";
    public const string RefreshTokenRevoked = "REFRESH_TOKEN_REVOKED";
    public const string RefreshTokenExpired = "REFRESH_TOKEN_EXPIRED";
    public const string InvalidPassword = "INVALID_PASSWORD";

    // Forbidden
    public const string NoPermissionToCreateVisits = "NO_PERMISSION_TO_CREATE_VISITS";
    
    public const string InvalidEmail = "INVALID_EMAIL";
    public const string InvalidPhone = "INVALID_PHONE";
    public const string NameRequired = "NAME_REQUIRED";
    public const string ReasonRequired = "REASON_REQUIRED";
    public const string PasswordTooShort = "PASSWORD_TOO_SHORT";
    
    public const string ServiceRequired = "SERVICE_REQUIRED";
    public const string PriceRequired = "PRICE_REQUIRED";
    public const string InvalidPostalCode = "INVALID_POSTAL_CODE";
    public const string NoActiveSubscription = "NO_ACTIVE_SUBSCRIPTION";
    public const string SubscriptionSuspended = "SUBSCRIPTION_SUSPENDED";
    public const string InvalidReminderSettings = "INVALID_REMINDER_SETTINGS";
    
    public const string InvalidDuration = "INVALID_DURATION";
    public const string InvalidPrice = "INVALID_PRICE";
    public const string ServiceBreedMismatch = "SERVICE_BREED_MISMATCH";
    public const string InvalidPickupTime = "INVALID_PICKUP_TIME";
    public const string SmsLimitExceeded = "SMS_LIMIT_EXCEEDED";
    public const string InvalidSmsPackage = "INVALID_SMS_PACKAGE";
}