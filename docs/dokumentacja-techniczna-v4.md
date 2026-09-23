# Groomsli — dokumentacja techniczna v4

**Data:** 06.09.2026
**Zakres:** zmiana nazwy na Groomsli, własna domena, landing, dwa plany subskrypcji, karta przed trialem, integracja SMSAPI, przypomnienia per salon, rasy dodawane przez salon, motyw marki

> Ten dokument **zastępuje** dokumentację v3 w częściach, których dotyczy.
> Rzeczy nieopisane tutaj (Fazy 1–4, konwencje, wzorce) pozostają zgodne z poprzednimi wersjami.

---

# 0. NAJWAŻNIEJSZE ZMIANY W SKRÓCIE

| Obszar | Stan |
|---|---|
| **Nazwa produktu** | ✅ **Grooming Management App → Groomsli** |
| **Domena** | ✅ `groomsli.pl` (landing), `app.groomsli.pl`, `api.groomsli.pl` |
| **DNS** | ✅ przeniesiony z LH.pl do Cloudflare |
| **Landing page** | ✅ statyczny HTML na Cloudflare Workers |
| **SMS** | ✅ **SMSAPI zintegrowane i działa na produkcji** |
| **Przypomnienia** | ✅ czytają ustawienia salonu, treść zależna od terminu |
| **Pakiety SMS** | ✅ limit miesięczny + doładowania przez Stripe |
| **Plany** | ✅ Basic 69 zł / 50 SMS, Standard 109 zł / 200 SMS |
| **Stripe** | ✅ **tryb live, produkcyjny webhook** |
| **Karta przed trialem** | ✅ nowy status `AwaitingPayment` |
| **Rasy własne salonu** | ✅ `Breed.SalonId` nullable |
| **Motyw marki** | ✅ paleta Groomsli w całej aplikacji |
| **Formalne** | ❌ brak regulaminu, polityki prywatności, RODO |
| **VAT** | ❌ **niespójny w Stripe, do rozstrzygnięcia z księgową** |
| **Testy automatyczne** | ❌ nadal brak |

---

# 1. NAZWA I DOMENA

## 1.1 Groomsli

Produkt nazywa się **Groomsli**. Nazwa jest w: landingu, nagłówku aplikacji, ekranach logowania i rejestracji, produktach Stripe.

**Do podmiany, jeszcze nie zrobione:**
- nazwa konta w Stripe (Settings → Business) — na stronie płatności widnieje jeszcze „Grooming Management App"
- nazwa nadawcy SMS w SMSAPI
- namespace’y w kodzie zostają `Grooming_Management_App` / `Grooming_Management_Frontend` — świadomie nietknięte

## 1.2 Domeny

Kupione w **LH.pl**: `groomsli.pl` i `groomsly.pl` (druga jako zabezpieczenie przed literówką, na razie nieużywana).

```
groomsli.pl       → landing (Cloudflare Workers)
app.groomsli.pl   → grooming-web  (Azure App Service)
api.groomsli.pl   → grooming-api  (Azure App Service)
```

## 1.3 DNS w Cloudflare

Serwery nazw przeniesione z LH.pl do Cloudflare:
```
ned.ns.cloudflare.com
raegan.ns.cloudflare.com
```

Rekordy:

| Typ | Nazwa | Wartość | Proxy |
|---|---|---|---|
| CNAME | `app` | `grooming-web-fndnhse0brfudeec.polandcentral-01.azurewebsites.net` | **DNS only** |
| CNAME | `api` | `grooming-api-eddjdfaecvb2gbew.polandcentral-01.azurewebsites.net` | **DNS only** |
| TXT | `asuid.app` | identyfikator z Azure | — |
| TXT | `asuid.api` | ten sam identyfikator | — |

> ⚠️ **Proxy Cloudflare (pomarańczowa chmurka) MUSI być wyłączone dla `app` i `api`.**
> Azure ma własne certyfikaty dla tych domen, a Blazor Server potrzebuje stabilnego
> WebSocketu, który proxy potrafi zrywać. Cloudflare pokazuje ostrzeżenie
> „not fully protected" — ignorować.

> **Identyfikator TXT jest wspólny dla całej subskrypcji Azure**, nie per App Service.
> Ten sam ciąg działa dla `asuid.app` i `asuid.api`.

### Pułapki napotkane przy konfiguracji

**CNAME `api` wskazujący na `grooming-web`.** Skopiowany zły adres — `api.groomsli.pl` otwierał frontend, a weryfikacja w Azure zgłaszała mylący błąd o braku rekordu TXT.

**Lokalny DNS trzyma starą odpowiedź.** `nslookup groomsli.pl` pokazywał stare serwery, mimo że propagacja przeszła. Weryfikacja przez `nslookup groomsli.pl 8.8.8.8` omija lokalny cache i pokazuje prawdę.

**Domena usunięta z Workera.** Przy szukaniu przyczyny białej strony przypadkowo usunięta z „Custom Domains and Routes" — trzeba było dodać ponownie.

## 1.4 Landing page

**Azure Static Web Apps nie zadziałało** — subskrypcja ma politykę ograniczającą regiony i odmawia utworzenia zasobu (`RequestDisallowedByAzure`) we wszystkich dostępnych lokalizacjach.

Landing stoi na **Cloudflare Workers** (statyczne zasoby, plan Free). Wdrożenie przez przeciągnięcie pliku w panelu: Workers & Pages → projekt → New deployment.

Plik: pojedynczy `index.html`, ~700 linii, wszystko inline poza fontami z Google.

**Struktura:** hero z widokiem dnia → problem → funkcje → konta pracowników → rozliczenia → SMS → dwie ścieżki startu → cena (dwa plany) → kontakt.

**Piksele:** miejsce w `<head>`, funkcja `track()` na dole, atrybuty `data-track` na przyciskach. Zdarzenia rozdzielone (`cta_hero_rejestracja` vs `cta_hero_pokaz`), żeby dało się zmierzyć, którą drogą idą ludzie.

> **Plik musi trafić jako `index.html` w katalogu głównym.** Wgranie folderu daje białą stronę.

---

# 2. INFRASTRUKTURA — CO SIĘ ZMIENIŁO

## 2.1 Always On

**Było wyłączone na obu App Service.** Skutki, o których nie wiedzieliśmy:

- Azure ubijał proces po 20 minutach bezczynności
- **`ReminderScheduler`, `SubscriptionScheduler` i `TokenCleanupScheduler` nie chodziły** — `BackgroundService` żyje tyle, co proces
- obwody SignalR padały w środku pracy, co objawiało się jako „błąd serwera" przy dłuższym wypełnianiu formularza

Włączone na `grooming-api` i `grooming-web`. Web sockets nie są już osobnym przełącznikiem w portalu — działają domyślnie.

> **Koligacja sesji (ARR affinity) jest wyłączona.** Przy jednej instancji bez znaczenia,
> ale przy skalowaniu w poziomie Blazor Server się rozsypie. Do włączenia przed skalowaniem.

## 2.2 Zmienne środowiskowe — komplet

**`grooming-api`:**
```
ConnectionStrings__DefaultConnection
JwtSettings__SecretKey
JwtSettings__ExpirationMinutes          ← 30
JwtSettings__RefreshTokenExpirationDays ← 7
SmsApi__Token
SmsApi__Sender                          ← jeszcze nie ustawione
Stripe__SecretKey                       ← sk_live_
Stripe__PriceId                         ← Standard
Stripe__BasicPriceId                    ← ❌ DO DODANIA
Stripe__WebhookSecret
Stripe__SuccessUrl                      ← https://app.groomsli.pl/ustawienia?platnosc=ok
Stripe__CancelUrl                       ← https://app.groomsli.pl/ustawienia?platnosc=anulowana
Stripe__SmsPackages__50 / __100 / __200
```

**`grooming-web`:**
```
ApiBaseUrl = https://api.groomsli.pl/     ← ZE SLASHEM
```

> ⚠️ **`appsettings.json` z repozytorium nie jest źródłem prawdy dla produkcji.**
> Każdy klucz dodany lokalnie musi mieć odpowiednik w zmiennych Azure — inaczej
> `GetValue<int>` zwróci zero i nikt tego nie zauważy. Patrz sekcja 3.1.

## 2.3 Koszty

Około 60 euro miesięcznie: PostgreSQL Burstable B1ms (~34 euro) + plan App Service B1 (~13 euro) dzielony przez dwie aplikacje. Baza jest już na najtańszym możliwym tierze.

**Jedna baza obsłuży wszystkie salony** — multi-tenancy przez `SalonId`. Koszt infrastruktury jest w zasadzie stały, przychód rośnie liniowo.

---

# 3. BŁĘDY, KTÓRE KOSZTOWAŁY NAJWIĘCEJ CZASU

## 3.1 🔴 Brak `JwtSettings__ExpirationMinutes` w Azure

**Objaw:** mama wylogowywana w środku pracy, przy zapisie wizyty.

**Przyczyna:** zmiennej nie było w Azure. `configuration.GetValue<int>("JwtSettings:ExpirationMinutes")` przy braku klucza zwraca **zero**, więc `AddMinutes(0)` — tokeny wygasały w chwili wystawienia.

**Dlaczego to nie było widać:** `ApiClient` automatycznie odświeżał token przy 401, więc aplikacja działała. Ale przy rotacji refresh tokenów każde żądanie unieważniało poprzedni token, a przy kilku równoległych zapytaniach jedno wygrywało wyścig i pozostałe lądowały na `ClearAsync()`.

**Poprawka w `TokenService`:**
```csharp
var expirationMinutes = configuration.GetValue<int>("JwtSettings:ExpirationMinutes");

if (expirationMinutes <= 0)
    throw new InvalidOperationException("JwtSettings:ExpirationMinutes is not configured");
```

**Plus semafor w `ApiClient.TryRefreshAsync`** — zapobiega równoległemu odświeżaniu:
```csharp
private static readonly SemaphoreSlim RefreshLock = new(1, 1);
// static, bo ApiClient jest Scoped, a bramka ma być wspólna dla żądań użytkownika
```

## 3.2 🔴 `BreedService` bez `IBreedWriterService`

Zarejestrowane w DI przez `AddScopedWithInterfaces`, ale klasa nie implementowała drugiego interfejsu. `InvalidCastException` **przy tworzeniu kontrolera**, więc padało każde żądanie do `BreedController` — także `GET`.

Kompilator tego nie łapie: rzutowanie zachodzi w czasie działania.

## 3.3 🔴 Wartość domyślna w bazie ≠ konfiguracja

`SalonConfiguration` mówiło `HasDefaultValue(0)`, ale kolumna w bazie miała `DEFAULT 100` — migracja nigdy nie została wygenerowana po zmianie. Nowe salony dostawały 100 SMS-ów mimo `SmsIncluded = 0` w kodzie.

Weryfikacja:
```sql
SELECT column_name, column_default FROM information_schema.columns
WHERE table_name = 'Salons' AND column_name LIKE 'Sms%';
```

Naprawione migracją `FixSmsDefaults`.

## 3.4 🔴 Pole w DTO bez mapowania w projekcji

**Trzeci raz w projekcie.** Wcześniej `Phone` w `GetSalonAsync`, wcześniej `RequiresPasswordChange` w `LoginAsync`, teraz `PlanType` w `GetSubscriptionAsync`.

`Phone` był szczególnie podstępny: pole nie było mapowane przy odczycie, więc front dostawał `null`, odsyłał `null` przy zapisie, a `UpdateSalonAsync` czyścił kolumnę. **Numer kasował się sam przy każdym zapisie ustawień.**

> **Zasada:** dodając pole do DTO, sprawdź projekcję. Kompilator milczy, bo brakująca
> właściwość to po prostu wartość domyślna.

## 3.5 🔴 Faktury zerowe przy trialu

Stripe przy starcie triala wystawia fakturę na 0 zł i wysyła `invoice.paid`. `RegisterPaymentAsync` ustawiał wtedy `Active` i przedłużał subskrypcję o miesiąc, nadpisując datę końca triala.

```csharp
if (invoice.AmountPaid == 0)
{
    logger.LogInformation("Invoice {InvoiceId} has zero amount (trial), ignoring", invoice.Id);
    return;
}
```

## 3.6 🔴 Faktury za doładowanie SMS

Płatność jednorazowa też generuje fakturę i `invoice.paid`. Bez rozróżnienia doładowanie za 15 zł przedłużało abonament o miesiąc.

```csharp
if (invoice.Parent?.SubscriptionDetails?.SubscriptionId == null)
{
    logger.LogInformation("Invoice {InvoiceId} is not subscription-related, ignoring", invoice.Id);
    return;
}
```

To samo w `HandleInvoiceFailedAsync`.

## 3.7 🟡 `IgnoreQueryFilters()` w `RefreshTokenAsync`

Ta sama pułapka co w `LoginAsync`, opisana w v3 — tylko poprawka nigdy tam nie weszła. `.Include(u => u.Groomer)` przy `[AllowAnonymous]` wybucha `NullReferenceException`.

## 3.8 🟡 `MudAutocomplete` czyści wpisany tekst

Przy utracie fokusu bez wyboru z listy tekst znika. `CoerceText="false"` i `CoerceValue="true"` nie pomogły.

**Rozwiązanie:** rozdzielenie na dwa pola — `MudAutocomplete` do wyboru z listy i osobny `MudTextField` na rasę spoza słownika, widoczny gdy nic nie wybrano.

---

# 4. SMS — INTEGRACJA SMSAPI

## 4.1 `SmsApiService`

Zastąpił `MockSmsService` na produkcji. Rejestracja warunkowa:

```csharp
if (builder.Environment.IsDevelopment())
    builder.Services.AddScoped<ISmsService, MockSmsService>();
else
    builder.Services.AddHttpClient<ISmsService, SmsApiService>();
```

**`AddHttpClient`, nie `AddScoped`** — zarządzany `HttpClient` z pulą połączeń.

**Nie rzuca wyjątku przy błędzie**, tylko zwraca `Success = false`, tak samo jak mock. `NotificationService` zapisuje wtedy `Notification` ze statusem `Failed`.

**Normalizacja numeru** — wyciąga cyfry, dokleja `48` przy dziewięciu cyfrach. Groomerki wpisują telefony ze spacjami, myślnikami i plusem.

## 4.2 Stan konta SMSAPI

- Firma **zweryfikowana** — SMS-y idą do dowolnych odbiorców
- **Nazwa nadawcy niezgłoszona** — wiadomości wychodzą z domyślnego numeru SMSAPI
- Filtr IP wyłączony (do włączenia, gdy wysyłka będzie szła tylko z produkcji — wychodzące adresy App Service w Networking)
- Koszt: **0,17 zł za wiadomość**

## 4.3 Pakiety SMS

```csharp
public class Salon
{
    public int SmsIncluded { get; set; }      // z pakietu, resetowany co miesiąc
    public int SmsPurchased { get; set; }     // dokupiony, nie wygasa
    public DateOnly SmsResetDate { get; set; }
}
```

**Zużycie:** najpierw `SmsIncluded`, potem `SmsPurchased`. Tylko przy udanej wysyłce.

```csharp
private static bool HasSmsAvailable(Salon salon)
    => salon.SmsIncluded + salon.SmsPurchased > 0;

private static void DeductSms(Salon salon)
{
    if (salon.SmsIncluded > 0) { salon.SmsIncluded--; return; }
    salon.SmsPurchased--;
}
```

**Reset** w `SubscriptionScheduler` przez `ResetMonthlySmsPackagesAsync`. Pomija salony `Suspended` i `AwaitingPayment`. Niewykorzystane z pakietu przepadają, dokupione zostają.

> **Reset po dacie, nie po płatności.** Salon w trialu nigdy nie trafi do
> `RegisterPaymentAsync`, a opóźniona płatność nie może zostawić salonu bez SMS-ów.

**Endpoint:** `GET /api/Salon/sms-balance` → `{ Remaining, ResetDate }`, dla obu ról.

**Pasek ostrzegawczy** w `MainLayout` przy ≤ 20 wiadomościach, zamykalny. Zamknięcie zapamiętuje stan licznika — przy zmianie salda alert wraca.

## 4.4 Doładowania przez Stripe

Trzy produkty, płatność jednorazowa (`Mode = "payment"`):

| Pakiet | Netto | Brutto (z VAT) |
|---|---|---|
| 50 SMS | 12,50 zł | 15,38 zł |
| 100 SMS | 25,00 zł | 30,75 zł |
| 200 SMS | 50,00 zł | 61,50 zł |

Metadane sesji niosą `type=sms_topup` i `smsCount`. `HandleCheckoutCompletedAsync` rozgałęzia się na tej podstawie.

**Idempotencja przez `ProviderId`** = identyfikator sesji (`cs_live_...`), nie faktury — przy płatności jednorazowej faktury nie ma.

Doładowanie trafia do tabeli `Payments` z `PeriodStart = PeriodEnd = dzień zakupu`.

---

# 5. PRZYPOMNIENIA O WIZYTACH

## 5.1 `ReminderScheduler` czyta ustawienia salonu

Było: sztywne 24 godziny, jedno okno dla wszystkich.
Jest: okno liczone osobno dla każdego salonu z jego `ReminderHoursBefore`.

```csharp
var salons = await ctx.Salons
    .Where(s => s.RemindersEnabled)
    .Where(s => s.SubscriptionStatus != SubscriptionStatusEnum.Suspended
                && s.SubscriptionStatus != SubscriptionStatusEnum.AwaitingPayment)
    .Select(s => new { s.Id, s.ReminderHoursBefore })
    .ToListAsync(stoppingToken);

foreach (var salon in salons)
{
    var windowStart = DateTime.UtcNow.AddHours(salon.ReminderHoursBefore);
    var windowEnd = windowStart.Add(_interval);
    // ...zapytanie o wizyty tego salonu w tym oknie
}
```

**Okno ma szerokość interwału (20 min)**, więc okna kolejnych przebiegów kleją się bez luk — ale osobno dla każdego salonu.

**Brak limitu SMS logowany jako `LogInformation`, nie `LogError`:**
```csharp
catch (ConflictException ex) when (ex.Message == ErrorCodes.SmsLimitExceeded)
{
    logger.LogInformation("Skipping reminder for visit {VisitId} - salon {SalonId} has no SMS left", ...);
}
```

**Przypomnienia domyślnie wyłączone** dla nowych salonów (`RemindersEnabled = false`) — zużywają pakiet, więc salon ma je włączyć świadomie.

## 5.2 Treść zależna od terminu

Wcześniej zawsze „o jutrzejszej wizycie", co przy wyprzedzeniu 2 godzin było nieprawdą.

```csharp
private static string FormatVisitDate(DateTime localVisitTime)
{
    var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PolishTime).Date;
    var visitDay = localVisitTime.Date;

    if (visitDay == todayLocal) return $"dzisiaj o godzinie {localVisitTime:HH:mm}";
    if (visitDay == todayLocal.AddDays(1)) return $"jutro o godzinie {localVisitTime:HH:mm}";

    return $"{localVisitTime:dd.MM} o godzinie {localVisitTime:HH:mm}";
}
```

## 5.3 Powiadomienie o odbiorze

Przycisk „Powiadom o odbiorze" w `VisitDetailsDialog`, z dialogiem `PickupNotificationDialog` pytającym o liczbę minut (domyślnie 30).

`GetVisitDetailsDto.PickupNotificationSent` — sprawdzane w projekcji, przycisk znika po wysłaniu:
```csharp
PickupNotificationSent = ctx.Notifications
    .IgnoreQueryFilters()
    .Any(n => n.VisitId == v.Id && n.SalonId == salonId
              && n.Status == NotificationStatusEnum.Sent
              && n.Type == NotificationTypeEnum.ManualReady),
```

**Walidacja `timeToPickUpDogInMin <= 0`** na górze `SendReadyForPickupNotificationAsync` — bez niej front wysyłał zero i klient dostawał „Zapraszamy za 0 min".

## 5.4 Wizyta bliższa niż wyprzedzenie

Wizyta dodana na za 2 godziny przy ustawieniu „dzień przed" **nie dostanie przypomnienia** — okno jest zawsze w przyszłości. To celowe: klient, który przed chwilą umówił się telefonicznie, nie potrzebuje SMS-a.

---

# 6. PLANY SUBSKRYPCJI

## 6.1 Model

```csharp
public enum PlanTypeEnum { Basic, Standard }
```

| | Basic | Standard |
|---|---|---|
| Cena | 69 zł | 109 zł |
| SMS miesięcznie | 50 | 200 |
| Konta dla pracowników | ❌ | ✅ |
| Wizyty, psy, pracownicy | bez limitu | bez limitu |

```csharp
public static int SmsPackageFor(PlanTypeEnum plan) => plan switch
{
    PlanTypeEnum.Basic => 50,
    PlanTypeEnum.Standard => 200,
    _ => 50
};
```

## 6.2 Konta pracowników tylko w Standard

**Egzekwowanie przy tworzeniu konta** (`RegisterGroomerAccountAsync`), na samej górze:
```csharp
if (salon.PlanType == PlanTypeEnum.Basic)
    throw new ConflictException(ErrorCodes.PlanDoesNotAllowAccounts);
```

**Basic może dodawać groomerów** — tylko bez kont. Grafik i rozliczenia prowadzi właściciel.

**Zejście na Basic dezaktywuje istniejące konta** w `RegisterPaymentAsync`:
```csharp
if (plan == PlanTypeEnum.Basic)
{
    var groomerUserIds = await ctx.Groomers
        .Where(g => g.SalonId == salonId && g.UserId != null)
        .Select(g => g.UserId!.Value)
        .ToListAsync(ct);

    await ctx.Users
        .Where(u => groomerUserIds.Contains(u.Id))
        .ExecuteUpdateAsync(s => s.SetProperty(u => u.ActiveStatus, ActiveStatusEnum.Inactive), ct);
}
```

**Wymagało naprawienia długu z v2:** `ActiveStatus` nie był sprawdzany w `LoginAsync`. Dodane:
```csharp
if (user.ActiveStatus != ActiveStatusEnum.Active)
    throw new UnauthorizedException(ErrorCodes.AccountInactive);
```
To samo w `RefreshTokenAsync`.

**Front:** w `Groomers.razor` opcja „Utwórz konto" zostaje widoczna, ale wyszarzona z etykietą „(plan Standard)" — ukrycie nie informowałoby o istnieniu funkcji.

```csharp
private bool CanCreateAccounts =>
    subscription?.PlanType == PlanTypeEnum.Standard
    && subscription.Status != SubscriptionStatusEnum.AwaitingPayment;
```

## 6.3 Odczyt planu z webhooka

**Cena na fakturze bywa niedostępna** — `invoice.Lines.Data[0].Pricing.PriceDetails.Price` zwracało `null`. Pewniejsze jest pobranie subskrypcji:

```csharp
private async Task<PlanTypeEnum> ResolvePlanAsync(Invoice invoice, CancellationToken ct)
{
    var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;
    if (string.IsNullOrEmpty(subscriptionId)) return PlanTypeEnum.Basic;

    var subscription = await new Stripe.SubscriptionService()
        .GetAsync(subscriptionId, cancellationToken: ct);

    var plan = PlanFromMetadata(subscription.Metadata)
               ?? PlanFromPriceId(subscription.Items?.Data?.FirstOrDefault()?.Price?.Id);

    return plan ?? PlanTypeEnum.Basic;
}
```

> ⚠️ **`CurrentPeriodEnd` przeniosło się na pozycję subskrypcji** w nowszych wersjach
> Stripe.NET: `subscription.Items.Data[0].CurrentPeriodEnd`, nie `subscription.CurrentPeriodEnd`.

---

# 7. KARTA PRZED TRIALEM

## 7.1 Przepływ

1. Rejestracja tworzy salon ze statusem **`AwaitingPayment`**, bez daty ważności, z zerem SMS-ów
2. Front przekierowuje na **`/wybierz-plan`**
3. Wybór planu → Checkout Stripe z `TrialPeriodDays = 30`
4. `checkout.session.completed` → `ApplySubscriptionFromStripeAsync` → `StartTrialAsync`
5. Po 30 dniach Stripe obciąża kartę, `invoice.paid` ustawia `Active`

## 7.2 Nowy status

```csharp
public enum SubscriptionStatusEnum
{
    Trial, Active, PastDue, Suspended,
    AwaitingPayment   // ← na KOŃCU, żeby nie przesunąć numerów w bazie
}
```

Migracja niepotrzebna — kolumna to `int`.

## 7.3 `RegisterSalonAsync`

```csharp
SubscriptionStatus = SubscriptionStatusEnum.AwaitingPayment,
SubscriptionValidUntil = null,
PlanType = PlanTypeEnum.Standard,   // tymczasowo, nadpisane przy wyborze planu
SmsIncluded = 0,
SmsPurchased = 0,
SmsResetDate = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1),
RemindersEnabled = false,
```

## 7.4 Trial ustawiany z `checkout.session.completed`

**Przy trialu nie ma faktury**, więc plan musi jechać w metadanych subskrypcji:

```csharp
SubscriptionData = new SessionSubscriptionDataOptions
{
    TrialPeriodDays = withTrial ? TrialDays : null,
    Metadata = new Dictionary<string, string>
    {
        ["salonId"] = salonId.ToString(),
        ["plan"] = plan.ToString()
    }
}
```

```csharp
public async Task StartTrialAsync(int salonId, PlanTypeEnum plan, DateOnly validUntil, CancellationToken ct)
{
    salon.PlanType = plan;
    salon.SubscriptionStatus = SubscriptionStatusEnum.Trial;
    salon.SubscriptionValidUntil = validUntil;
    salon.SmsIncluded = SmsPackageFor(plan);
    salon.SmsResetDate = today.AddMonths(1);
}
```

**Trial należy się raz** — kontroler sprawdza, czy salon miał już `ProviderCustomerId`:
```csharp
var withTrial = await service.GetProviderCustomerIdAsync(salonId, ct) == null;
```

## 7.5 Blokada zapisu

`SubscriptionMiddleware` blokuje POST dla `AwaitingPayment` tak samo jak dla `Suspended`, ale z innym kodem błędu:

```csharp
if (status == SubscriptionStatusEnum.AwaitingPayment)
    → 402, ErrorCodes.PaymentMethodRequired

if (status == SubscriptionStatusEnum.Suspended)
    → 402, ErrorCodes.SubscriptionSuspended
```

Salon bez karty **widzi całą aplikację, ale nic nie zapisze**. Wyjątek na `/api/Subscription` już istniał, więc Checkout działa.

## 7.6 Front

**`/wybierz-plan`** — nowy ekran po rejestracji, w stylu landingu. Dwie karty planów, przekierowanie na Stripe **w tym samym oknie** (`NavigateTo(url, forceLoad: true)`), dyskretne „Zrobię to później".

Nie dziedziczy po `AuthenticatedPage` — ma skopiowaną logikę wczytania tokenów, tak jak `ChangePassword`.

**Rozróżnienie w `SalonSettings`:** wybór planu pokazuje się na podstawie `HasActiveSubscription` (czyli `ProviderSubscriptionId != null`), nie statusu. Salon w trialu ma już subskrypcję i widzi informację o planie plus datę pierwszej płatności.

**Odświeżenie tokenu po powrocie z płatności:**
```csharp
if (subscription != null && TokenStore.SubscriptionStatus != subscription.Status.ToString())
    await Api.RefreshTokenAsync();
```
Poprzednia wersja sprawdzała tylko przejście na `Active` i nie łapała `AwaitingPayment → Trial`.

---

# 8. STRIPE — TRYB LIVE

## 8.1 Produkty

| Produkt | Cena | `price_` (live) |
|---|---|---|
| Groomsli Standard | 109 zł / mies. | `price_1UBjL6BHmI7JqkrAxQ9EEM6r` |
| Groomsli Basic | 69 zł / mies. | `price_1UCR8oBHmI7JqkrAAz0WuFqY` |
| Pakiet 50 SMS | 12,50 zł | `price_1UBjNbBHmI7JqkrAKYUYFshF` |
| Pakiet 100 SMS | 25 zł | `price_1UBjOIBHmI7JqkrAWqgUCbuu` |
| Pakiet 200 SMS | 50 zł | `price_1UBjOpBHmI7JqkrA94BZJYND` |

Kategoria podatkowa: SaaS – business use. Ceny są niezmienne — zmiana wymaga nowej ceny i podmiany `price_` w konfiguracji.

## 8.2 Webhook produkcyjny

```
https://api.groomsli.pl/api/Subscription/webhook
```

Pięć zdarzeń: `checkout.session.completed`, `invoice.paid`, `invoice.payment_failed`, `customer.subscription.updated`, `customer.subscription.deleted`.

## 8.3 ⚠️ VAT — niespójny, do rozstrzygnięcia

**Stripe Tax jest włączony**, a ustawienie „Include tax in prices" na koncie stoi na **No** — czyli VAT dolicza się na wierzchu.

Skutek: pakiety SMS mają VAT doliczony (12,50 → 15,38 zł), a subskrypcja ma go wliczonego w cenę. **Dwa produkty z jednej firmy traktowane inaczej.**

**Nierozstrzygnięte:** czy Konrad jest podatnikiem VAT (czy składał VAT-R). Jeśli nie, pobieranie podatku od klientów jest problemem. **To jedyna rzecz z całej listy, której nie da się cofnąć commitem.**

Ceny na landingu i w aplikacji podane jako brutto dla pakietów, netto dla subskrypcji — zgodnie z tym, co faktycznie widzi klient.

## 8.4 Testy lokalne

```powershell
cd "C:\Users\konra\OneDrive\Pulpit\stripeCLI\stripe_1.50.6_windows_x86_64"
.\stripe.exe listen --forward-to https://localhost:7250/api/Subscription/webhook --skip-verify
```

⚠️ `whsec_` zmienia się przy każdym uruchomieniu CLI — podmienić w `appsettings.Development.json` i **zrestartować backend**.

⚠️ **Tryby test i live mają osobne katalogi produktów.** Produkcyjne `price_` w konfiguracji lokalnej dają `No such price: ...; a similar object exists in live mode, but a test mode key was used`.

---

# 9. RASY DODAWANE PRZEZ SALON

## 9.1 Model

```csharp
public class Breed
{
    public int? SalonId { get; set; }   // null = słownik globalny (102 rasy z SeedBreeds)
    public Salon? Salon { get; set; }
}
```

> **Bez globalnego query filtra.** `Breed` jest współdzielony między salonami i pojawia
> się w nawigacjach z `Dog` i `ServiceBreed` — globalny filtr komplikowałby każde
> zapytanie. Zamiast tego jawny warunek w dwóch metodach serwisu.

```csharp
public async Task<List<GetBreedDto>> GetAllBreedsAsync(int salonId, CancellationToken ct)
{
    return await ctx.Breeds
        .Where(b => b.SalonId == null || b.SalonId == salonId)
        .OrderBy(b => b.Name)
        .Select(...)
        .ToListAsync(ct);
}
```

## 9.2 Tworzenie

`POST /api/Breed`, rola `Owner,Groomer` — groomer też umawia wizyty i trafia na nieznanego psa.

Sprawdzenie duplikatu obejmuje rasy globalne i własne, porównanie przez `ToLower()` po obu stronach.

## 9.3 Front

W `AddVisitDialog`, przy nowym psie: `MudAutocomplete` do wyboru z listy i **osobne pole tekstowe** „Nie ma tej rasy? Wpisz własną", widoczne gdy nic nie wybrano.

**Rasa powstaje przy zapisie wizyty**, nie przy wpisywaniu — inaczej każde porzucone dodawanie zostawiałoby śmieć w słowniku.

`ResolveBreedIdAsync` najpierw szuka po nazwie w załadowanej liście (ochrona przed duplikatem, gdy ktoś wpisze istniejącą rasę ręcznie), potem tworzy nową.

---

# 10. WYGLĄD I MARKA

## 10.1 Motyw MudBlazor

`Routes.razor` → `<MudThemeProvider Theme="GroomsliTheme" />` z paletą:

```
Primary          #2C6350   (średnia zieleń)
Secondary        #E0A93B   (bursztyn)
AppbarBackground #0E2C24   (głęboka zieleń)
Background       #FCFBF8
BackgroundGray   #F2EDE3
```

Typografia: **Bricolage Grotesque** (nagłówki), **Instrument Sans** (tekst). Oba z Google Fonts, link w `App.razor`.

`Button.TextTransform = "none"` — MudBlazor domyślnie pisze przyciski wersalikami, landing nie.

> ⚠️ **Kalendarz Radzena ma własne style** i motyw MudBlazora go nie dotyka.

## 10.2 Ekrany logowania i rejestracji

Przepisane na własny HTML/CSS zamiast komponentów MudBlazora — paleta i kroje jak na landingu. Na ekranach poniżej 480 px karta rozciąga się na całą wysokość, bez marginesów.

**`Login.razor` nie dziedziczy już po `AuthenticatedPage`** — klasa bazowa przekierowywała na `/login` i próbowała pobierać saldo SMS bez tokenu.

**Rejestracja: adres jest zwijany i opcjonalny.** Formularz z dziesięcioma polami na telefonie odstrasza.

`InputType.Email` i `InputType.Telephone` — właściwa klawiatura na telefonie.

## 10.3 Snackbar zamiast alertów

We wszystkich głównych dialogach (`AddVisitDialog`, `VisitDetailsDialog`, `AddDogDialog`, `EditDogDialog`, `AddDogOwnerDialog`, `EditDogOwnerDialog`, `SalonSettings`, `Groomers`) komunikaty idą przez `ISnackbar`.

**Powód:** alert na dole długiego formularza był poza ekranem — groomerka klikała Zapisz i nie widziała, co jest nie tak.

Pozycja: `Defaults.Classes.Position.TopCenter` w `AddMudServices`.

> ⚠️ **`AddMudServices` może być wywołane tylko raz.** Dwa wywołania (jedno bez
> konfiguracji) sprawiają, że ustawienia nie wchodzą.

> ⚠️ **`MudPopoverProvider` tylko w jednym miejscu** — dwa dają
> `There is already a subscriber to the content with the given section ID`.

## 10.4 Nowe komponenty

**`AgePicker.razor`** — wiek psa w latach i miesiącach, wewnętrznie `int` w miesiącach.

Kluczowy szczegół:
```csharp
protected override void OnParametersSet()
{
    if (_years * 12 + _months == Value) return;   // ← bez tego nie da się wpisywać
    _years = Value / 12;
    _months = Value % 12;
}
```
Parent po każdym `ValueChanged` przerenderowuje dziecko, a to nadpisywałoby wpisywaną wartość.

**`PickupNotificationDialog.razor`** — pytanie o liczbę minut przed wysłaniem SMS-a.

---

# 11. EDYCJA WIZYTY

`EditVisitAsync` obsługuje teraz komplet: termin, groomer, czas trwania, usługa, cena.

```csharp
public class EditVisitDto
{
    public DateTime Date { get; set; }
    public int GroomerId { get; set; }
    public int DurationMinutes { get; set; }
    public int? ServiceBreedId { get; set; }
    public decimal ProposedPrice { get; set; }
    public string? Notes { get; set; }
    public bool IgnoreOverlap { get; set; }
}
```

**Walidacja kolizji** — dług z Fazy 4 spłacony. Wykluczenie edytowanej wizyty:
```csharp
.Where(d => d.Id != visitId)
```

**Okno liczone z `dto.DurationMinutes`, nie `visit.EstimatedDuration`** — inaczej wydłużenie wizyty z 30 min na 3 h sprawdzałoby kolizję dla starych trzydziestu minut.

**`GetVisitDetailsDto` dostało identyfikatory** do wypełnienia formularza: `GroomerId`, `ServiceBreedId`, `BreedId`. To wyjątek od zasady „DTO wyjściowe pokazuje dane, nie surowe FK" — uzasadniony, bo to DTO karmi formularz edycji.

**Plus `DogNotes`** — notatki psa (alergie, „gryzie przy pazurach") widoczne w szczegółach wizyty jako `MudAlert` z `Severity.Warning`.

**Front:** tryb odczytu domyślny, edycja pod przyciskiem. Lista usług zawężona do rasy psa przez `api/ServiceBreed/GetAllServiceBreeds?breedId=...`.

> ⚠️ **Endpoint listy cennika ma jawny segment w trasie:**
> `api/ServiceBreed/GetAllServiceBreeds`, nie `api/ServiceBreed`.

**`ignoreOverlap` jest parametrem metody, nie polem** — jako pole zostawało włączone po jednym potwierdzeniu i kolejny zapis przechodził bez pytania.

---

# 12. STAN NA DZIŚ

## 12.1 ✅ Działa na produkcji

- Cała funkcjonalność z v3
- SMS-y przez SMSAPI: powiadomienia o odbiorze i automatyczne przypomnienia
- Stripe live: subskrypcje i doładowania pakietów
- Domena `groomsli.pl` z landingiem, `app.` i `api.`
- Motyw marki w całej aplikacji

## 12.2 ❌ Do wdrożenia (kod gotowy, nie wypchnięty)

**Trzy migracje na produkcyjną bazę, PRZED Publishem:**
```
AddPlanType
AddSalonToBreed
FixSmsDefaults
```

```powershell
dotnet ef database update --connection "Host=grooming-db.postgres.database.azure.com;Port=5432;Database=GroomingAppDb;Username=groomingadmin;Password=...;SSL Mode=Require"
```

**Zmienna w Azure:** `Stripe__BasicPriceId = price_1UCR8oBHmI7JqkrAAz0WuFqY`

**Publish backendu, potem frontendu.**

**Landing** — wgrać zaktualizowaną wersję na Cloudflare Workers.

## 12.3 🔴 Blokuje sprzedaż

| Obszar | Stan |
|---|---|
| **VAT** | Niespójny w Stripe, status podatkowy nierozstrzygnięty |
| **Formalne** | Brak regulaminu, polityki prywatności, umowy powierzenia (RODO) |
| **Nadawca SMS** | Niezgłoszony — wiadomości z numeru zamiast nazwy salonu |

Stopka landingu linkuje do `/regulamin.html` i `/prywatnosc.html`, których nie ma.

## 12.4 🟡 Zaczęte, niedokończone

- **Różnica planów na landingu i w `ChoosePlan`** — konta pracowników nie są wymienione na listach
- **Konta pracowników na landingu** — sekcja obiecuje je bez wzmianki o planie
- Zmiana planu w trakcie subskrypcji — dziś tylko przez portal Stripe (anulowanie), bez przełączania

## 12.5 🟡 Dług techniczny

- **Testy automatyczne** — nadal zero
- **Kontrola współbieżności na `Visit`** — dwie osoby edytujące tę samą wizytę, druga zmiana nadpisuje pierwszą po cichu. Przy dziesięciu groomerach realne. Rozwiązanie: `ConcurrencyToken`
- **ARR affinity wyłączone** — blokuje skalowanie w poziomie
- **Semafor `TryRefreshAsync` jest statyczny** — wspólny dla wszystkich użytkowników. Przy setkach użytkowników do zamiany na słownik per użytkownik
- **Wydzielenie DTO do projektu `Shared`** — frontend referencuje cały backend z EF Core, Stripe.net i połączeniem do bazy
- **GitHub Actions** — wdrożenie nadal ręczne
- **`AttemptCount` w `Notification`** — pole jest, logika ponawiania nie
- **Snackbar w pozostałych dialogach** (groomerzy, grafiki, blacklista, cennik) — zostawione świadomie, formularze mieszczą się na ekranie
- **Niespójne kody odpowiedzi** w `ServiceBreedController`: `Ok()` przy aktywacji, `NoContent()` przy edycji

## 12.6 ❓ Pytania do mamy (domain expert)

- **Rasy czy rozmiary w cenniku** — wisi od trzech tygodni, jedyna otwarta decyzja architektoniczna
- **Czy 195 zł to realna średnia wizyta** i czy 40% prowizji / 32 zł za godzinę są typowe (liczby użyte na landingu)
- **Treść SMS-a** — „Państwa pupila" brzmi sztywno
- **Ile minut podaje klientowi przy odbiorze** (domyślne 30)
- **Czy nadawca SMS ma być nazwą salonu**, czy wystarczy jedna wspólna („Groomsli"). Per salon wymaga pola `SmsSenderName` i osobnego zgłoszenia u operatorów przy każdym kliencie
- **Drugi kontakt do właściciela psa** — czy potrzebny i czy SMS-y mają iść na oba numery

---

# 13. NOWA CHECKLISTA POWTARZAJĄCYCH SIĘ BŁĘDÓW

Do listy z v3 dochodzą:

| Błąd | Objaw |
|---|---|
| **Brak zmiennej w Azure, `GetValue<int>` zwraca 0** | Cicha awaria, tu: tokeny wygasające natychmiast |
| **Klasa nie implementuje interfejsu zarejestrowanego w DI** | `InvalidCastException` przy tworzeniu kontrolera, pada cały kontroler |
| **`HasDefaultValue` w konfiguracji ≠ `DEFAULT` w bazie** | Nowe wiersze dostają wartość ze starej migracji |
| **Pole w DTO bez mapowania w projekcji** | Wartość domyślna zamiast prawdziwej; przy zapisie kasuje kolumnę |
| **Faktura zerowa przy trialu** | `invoice.paid` ustawia `Active` zamiast `Trial` |
| **Faktura za płatność jednorazową** | Doładowanie SMS przedłuża abonament o miesiąc |
| **Produkcyjne `price_` w konfiguracji lokalnej** | `a similar object exists in live mode` |
| **`MudAutocomplete` bez wyboru z listy** | Wpisany tekst znika przy utracie fokusu |
| **`AddMudServices` wywołane dwa razy** | Konfiguracja nie wchodzi |
| **CNAME wskazujący na zły App Service** | Subdomena otwiera nie tę aplikację; mylący błąd weryfikacji TXT |
| **Lokalny cache DNS** | `nslookup` kłamie; sprawdzać przez `nslookup domena 8.8.8.8` |
| **Always On wyłączone** | `BackgroundService` nie chodzi, obwody SignalR padają |

---

# 14. PROCEDURA URUCHOMIENIA (aktualizacja)

```bash
# backend — PROFIL HTTPS
cd Grooming-Management-Backend
dotnet ef database update
dotnet run --launch-profile https        # https://localhost:7250

# frontend
cd Grooming-Management-Frontend
dotnet run                                # https://localhost:7124

# webhooki Stripe (osobne okno)
cd "C:\Users\konra\OneDrive\Pulpit\stripeCLI\stripe_1.50.6_windows_x86_64"
.\stripe.exe listen --forward-to https://localhost:7250/api/Subscription/webhook --skip-verify
```

**`appsettings.Development.json`** musi zawierać (plik w `.gitignore`):
- `ConnectionStrings:DefaultConnection` — lokalny PostgreSQL
- `JwtSettings:SecretKey`
- `Stripe:SecretKey`, `PriceId`, `BasicPriceId`, `WebhookSecret`, `SuccessUrl`, `CancelUrl`, `SmsPackages` — **wszystko z trybu testowego**
- `SmsApi:Token` — tylko gdy testujesz prawdziwą wysyłkę

**Konta testowe (lokalnie):** `owner@test.com`, `anna@test.com` — hasło `Test1234!`

> ⚠️ **`dotnet ef` nie zadziała przy uruchomionym backendzie** — plik `.exe` jest zablokowany.

---

# 15. ZASADY WSPÓŁPRACY (bez zmian)

- Konrad pisze kod sam; asystent prowadzi przez opis, pytania i recenzję
- Gotowy kod tylko gdy Konrad utknie albo w nowej technologii
- **Wszystko po polsku** — rozmowy, etykiety UI, komunikaty do klienta
- Nazwy klas, metod i komunikaty wyjątków po angielsku
- Commity po angielsku, tryb rozkazujący, z roota repozytorium
- Commit po każdym skończonym kroku, także gdy funkcja jest nieprzetestowana — byle kod się kompilował
