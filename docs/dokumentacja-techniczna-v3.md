# Grooming Management App — dokumentacja techniczna v3

**Data:** 01.09.2026
**Zakres:** uprawnienia pracowników, wymuszona zmiana hasła, wizyty poza cennikiem, integracja Stripe, migracja na PostgreSQL, wdrożenie na Azure, konwencja stref czasowych, spłata długu technicznego

> Ten dokument **zastępuje** dokumentację v2 w częściach, których dotyczy.
> Rzeczy nieopisane tutaj (Fazy 1–3, podstawowe konwencje) pozostają zgodne z poprzednimi wersjami.

---

# 0. NAJWAŻNIEJSZE ZMIANY W SKRÓCIE

| Obszar | Stan |
|---|---|
| **Uprawnienia pracowników** | ✅ nowa funkcja — widoczność wizyt, prawo dodawania |
| **Wymuszona zmiana hasła** | ✅ pełny przepływ od logowania po blokadę tras |
| **Wizyty poza cennikiem** | ✅ przebudowa modelu — usługa + cena ręcznie ALBO pozycja cennika |
| **Stripe** | ✅ Checkout, webhooki, portal klienta, ekran subskrypcji |
| **Baza danych** | ✅ **SQL Server → PostgreSQL** |
| **Hosting** | ✅ **wdrożone na Azure** (App Service ×2 + PostgreSQL Flexible Server) |
| **Strefy czasowe** | ✅ konwencja UTC w bazie, konwersja na granicach |
| **Kody błędów** | ✅ `ErrorCodes` + słownik tłumaczeń we froncie |
| **Walidacja formatowa** | ✅ e-mail, telefon, kod pocztowy, puste pola |
| **Nakładanie wizyt** | ✅ ostrzeżenie zamiast blokady |
| **SMS** | ❌ nadal `MockSmsService` — provider niezintegrowany |
| **Testy automatyczne** | ❌ nadal brak |

---

# 1. INFRASTRUKTURA — CO SIĘ ZMIENIŁO

## 1.1 PostgreSQL zamiast SQL Servera

```csharp
// Program.cs
builder.Services.AddDbContext<GroomingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ConfigureWarnings(w => w.Ignore(
            CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));
```

**Migracje zostały wygenerowane od zera** — stary katalog `Migrations/` skasowany, bo migracje EF Core są specyficzne dla providera.

### Pułapki napotkane przy migracji

**`CHECK CONSTRAINT` ze składnią SQL Servera.** W `WaitlistConfiguration` był literał `"[Priority] >= 1 AND [Priority] <= 3"` — nawiasy kwadratowe to składnia MSSQL. PostgreSQL wymaga cudzysłowów:

```csharp
builder.ToTable(t => t.HasCheckConstraint(
    "CK_Waitlist_Priority_Range",
    @"""Priority"" >= 1 AND ""Priority"" <= 3"));
```

**Ostrzeżenie o `DogOwner`/`Notification`.** `Notification` celowo nie ma query filtra (czyta go `ReminderScheduler` bez `HttpContext`), ale wymaga `DogOwner`, który filtr ma. EF ostrzega o możliwych niepełnych wynikach. **Świadomie wyciszone** — nigdy nie sięgamy z `Notification` do `DogOwner` przez nawigację.

**Ostrzeżenie o zajętym pliku przy `dotnet ef`.** Jeśli backend chodzi, build się nie powiedzie. Zatrzymać przed migracjami.

## 1.2 Wdrożenie na Azure

```
Grupa zasobów: grooming-app (Poland Central)
├── grooming-db          Azure Database for PostgreSQL Flexible Server, Burstable B1ms
├── grooming-api         App Service, Linux, .NET 10, plan B1 (~13 USD/mies.)
└── grooming-web         App Service, TEN SAM plan B1
```

**Oba App Service dzielą jeden plan** — płacisz raz.

**Adresy** mają losowy człon (opcja „bezpieczna unikatowa nazwa hosta"):
- backend: `grooming-api-eddjdfaecvb2gbew.polandcentral-01.azurewebsites.net`
- frontend: `grooming-web-fndnhse0brfudeec.polandcentral-01.azurewebsites.net`

### Zmienne środowiskowe (podwójne podkreślenie = zagnieżdżenie)

**Backend:**
```
ConnectionStrings__DefaultConnection
JwtSettings__SecretKey
Stripe__SecretKey
Stripe__PriceId
Stripe__WebhookSecret
Stripe__SuccessUrl
Stripe__CancelUrl
```

**Frontend:**
```
ApiBaseUrl        ← ZE SLASHEM na końcu
```

### Pułapki napotkane przy wdrożeniu

**Konflikt `appsettings.json` przy publikowaniu.** Frontend referencuje backend, więc oba pliki konfiguracyjne trafiają do tego samego katalogu wyjściowego. Błąd `NETSDK1152`. Obejście w `Grooming-Management-Backend.csproj`:

```xml
<ItemGroup>
    <Content Update="appsettings.json">
        <CopyToPublishDirectory>Never</CopyToPublishDirectory>
    </Content>
    <Content Update="appsettings.Development.json">
        <CopyToPublishDirectory>Never</CopyToPublishDirectory>
    </Content>
</ItemGroup>
```

Nieszkodliwe, bo ustawienia produkcyjne są w zmiennych środowiskowych.

**Ręczne polecenie startowe blokuje aplikację.** Ustawienie `dotnet Grooming-Management-Frontend.dll` w Konfiguracja → Ustawienia ogólne sprawiło, że Azure pominął wstrzykiwane zmienne (m.in. `ASPNETCORE_URLS`) i aplikacja nasłuchiwała na `localhost:5000` zamiast `0.0.0.0:8080`. **Rozwiązanie: zostawić pole puste**, Azure radzi sobie sam.

**`web.config` w katalogu wdrożenia** — artefakt dla IIS, na Linuxie zbędny.

### Procedura aktualizacji

1. Commit lokalnie
2. Jeśli była migracja: `dotnet ef database update --connection "<produkcyjny>"`
3. Publish backendu z Ridera → `grooming-api`
4. Publish frontendu → `grooming-web`

> **Kolejność ma znaczenie.** Kod przed migracją = `Invalid column name` na produkcji.

## 1.3 Rozdzielenie środowisk

- `appsettings.Development.json` → **lokalna** baza
- Zmienne w Azure → **produkcyjna** baza
- Migracje na produkcję: jawnie, przez `--connection`

---

# 2. STREFY CZASOWE — KONWENCJA

To była najbardziej rozproszona zmiana w projekcie. Warto zrozumieć zasadę, zanim dotknie się czegokolwiek związanego z datami.

## 2.1 Zasada

> **Baza i backend trzymają wyłącznie UTC. Konwersja żyje wyłącznie we froncie, na granicach wejścia i wyjścia.**

Wyjątek: **treść SMS-a**, bo trafia do użytkownika z pominięciem frontendu.

## 2.2 Pomocnik we froncie

`Grooming-Management-Frontend/Services/Dates.cs`:

```csharp
public static class Dates
{
    private static readonly TimeZoneInfo PolishTime =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

    public static DateTime ToUtc(DateTime polish)
        => TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(polish, DateTimeKind.Unspecified), PolishTime);

    public static DateTime ToLocal(DateTime utc)
        => TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utc, DateTimeKind.Utc), PolishTime);

    public static string Format(DateTime utc, string format = "dd.MM.yyyy HH:mm")
        => ToLocal(utc).ToString(format);
}
```

> **`SpecifyKind` na `Unspecified` jest konieczne** — `MudDatePicker` zwraca datę z `Kind = Local`, a wtedy `ConvertTimeToUtc` użyłby strefy systemowej zamiast polskiej.

> **Nie używać `ToLocalTime()`** — Blazor Server renderuje po stronie serwera, a serwer na Azure chodzi w UTC. Konwersja nic by nie zrobiła.

## 2.3 Pomocnik w backendzie

`Extensions/DateTimeExtensions.cs`:

```csharp
public static DateTime AsUtc(this DateTime value)
    => DateTime.SpecifyKind(value, DateTimeKind.Utc);

public static DateTime? AsUtc(this DateTime? value)
    => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
```

Potrzebne, bo **deserializacja z query stringu gubi `DateTimeKind`**, a Npgsql wymaga jawnego UTC dla kolumn `timestamptz`.

Stosowane w kontrolerach przyjmujących daty:
```csharp
filter.DateFrom = filter.DateFrom.AsUtc();
filter.DateTo = filter.DateTo.AsUtc();
```

Dotyczy `VisitController` i `EarningsController` (cztery metody).

## 2.4 Konflikt `DateTime` z `DateOnly`/`TimeOnly`

`GroomerSchedule` i `GroomerTimeOff` opisują **czas lokalny salonu** przez `DateOnly` + `TimeOnly`. Złożenie ich przez `ToDateTime()` daje `timestamp without time zone`, a `Visit.Date` to `timestamptz`. **Npgsql odrzuca porównanie dwóch różnych typów.**

Rozwiązanie: konwersja przed porównaniem.

**W `VisitService`** (trzy metody: `AddVisitAsync`, `EditVisitAsync`, `CreateVisitWithNewDogAsync`):
```csharp
private static readonly TimeZoneInfo PolishTime =
    TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

// przed zapytaniem o GroomerTimeOffs:
var startLocal = TimeZoneInfo.ConvertTimeFromUtc(startTime, PolishTime);
var endLocal = TimeZoneInfo.ConvertTimeFromUtc(endTime, PolishTime);
```

**W `GroomerTimeOffService`** — odwrotny kierunek:
```csharp
var startUtc = TimeZoneInfo.ConvertTimeToUtc(dto.StartDate.ToDateTime(dto.StartTime), PolishTime);
var endUtc = TimeZoneInfo.ConvertTimeToUtc(dto.EndDate.ToDateTime(dto.EndTime), PolishTime);
```

**W `AvailabilityService`** — cała arytmetyka slotów w czasie lokalnym, UTC tylko na granicy zapytania:
```csharp
var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PolishTime);
var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(dayStartLocal, PolishTime);
```

## 2.5 Migracja typów kolumn

Schemat powstał przy włączonym `EnableLegacyTimestampBehavior`, więc wszystkie dwanaście kolumn `DateTime` miało typ `timestamp without time zone`. Migracja **`FixTimestampTypes`** zmieniła je na `timestamp with time zone`.

**Weryfikacja:**
```sql
SELECT table_name, column_name, data_type
FROM information_schema.columns
WHERE table_schema = 'public' AND data_type LIKE 'timestamp%';
```

⚠️ Przy istniejących danych konwersja przesuwa wartości. Zrobione, gdy baza zawierała tylko dane testowe.

---

# 3. UPRAWNIENIA PRACOWNIKÓW

## 3.1 Model

```csharp
public class Groomer
{
    public bool CanSeeAllVisits { get; set; }    // domyślnie true
    public bool CanCreateVisits { get; set; }    // domyślnie true
}
```

Konfiguracja: `HasDefaultValue(true)` na obu, **plus jawne ustawienie w `CreateGroomerAsync`** — bo `HasDefaultValue` ratuje tylko istniejące wiersze przy migracji, nie nowe obiekty tworzone w kodzie.

## 3.2 Egzekwowanie

**`GetAllVisitsAsync`** — filtr, gdy rola `Groomer` i brak uprawnienia:
```csharp
if (currentUser.Role == RoleEnum.Groomer)
{
    var me = await GetCurrentGroomerAsync(salonId, ct);

    if (me == null)
        return new List<GetAllVisitsDto>();   // niejasna tożsamość = pokaż nic

    if (!me.CanSeeAllVisits)
        query = query.Where(v => v.GroomerId == myId || v.AssistantGroomerId == myId);
}
```

**`AddVisitAsync` / `CreateVisitWithNewDogAsync`** — na samym początku:
```csharp
if (currentUser.Role == RoleEnum.Groomer)
{
    var me = await GetCurrentGroomerAsync(salonId, ct);

    if (me == null || !me.CanCreateVisits)
        throw new ForbiddenException(ErrorCodes.NoPermissionToCreateVisits);
}
```

> **403 przy tworzeniu, 404 przy odczycie.** Groomer wie, że próbuje dodać wizytę — nie ma czego ukrywać. Ale przy odczycie cudzej wizyty 403 potwierdziłby jej istnienie.

## 3.3 Nowe endpointy

| Endpoint | Role | Zwraca |
|---|---|---|
| `GET /api/Groomer/basic` | Owner, Groomer | Lista bez danych rozliczeniowych |
| `GET /api/Groomer/me` | Owner, Groomer | Profil zalogowanego (`null` dla właściciela) |

**`GetGroomerBasicDto`** celowo bez `SettlementType` i `SettlementRate` — groomer nie widzi stawek kolegów.

> **Powód powstania:** `GroomerController` ma `[Authorize(Roles = "Owner")]` na klasie. Groomer dostawał 403 przy pobieraniu listy, przez co kalendarz nie miał czym kolorować wizyt, a dialog dodawania wizyty miał pustą listę groomerów.

## 3.4 Nowa klasa bazowa dla stron

`Components/OwnerOnlyPage.cs` — dziedziczy po `AuthenticatedPage`, przekierowuje groomerów na `/`:

```csharp
public abstract class OwnerOnlyPage : AuthenticatedPage
{
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!firstRender) return;
        if (!TokenStore.IsLoggedIn) return;
        if (TokenStore.RequiresPasswordChange) return;

        if (!TokenStore.IsOwner)
        {
            Navigation.NavigateTo("/");
            return;
        }

        StateHasChanged();
    }
}
```

> **Dwa wczesne `return` są konieczne** — bazowa metoda mogła już przekierować na `/login` albo `/zmiana-hasla`.

Używają jej: `/groomerzy`, `/groomerzy/{id}`, `/cennik`, `/raporty`, `/ustawienia`, `/uslugi`, `/czarna-lista`.

---

# 4. WYMUSZONA ZMIANA HASŁA

## 4.1 Przepływ

1. Właściciel tworzy konto groomera → `RequiresPasswordChange = true`, hasło tymczasowe
2. `LoginAsync` zwraca flagę w `LoginResponseDto`
3. `Login.razor` przekierowuje na `/zmiana-hasla`
4. `AuthenticatedPage` zawraca z każdej innej trasy
5. Po zmianie hasła backend **rewokuje wszystkie refresh tokeny** i zwraca nowy komplet
6. Front zapisuje nowe tokeny z flagą `false`

## 4.2 Kluczowe szczegóły

**Flaga musi być wypełniana w czterech miejscach:** `LoginAsync`, `RefreshTokenAsync`, `ChangePasswordAsync`, `RegisterSalonAsync`. Pominięcie `RefreshTokenAsync` sprawiłoby, że po 30 minutach użytkownik wymyka się z ekranu.

**`TokenStore` zapisuje flagę do localStorage** — bez tego odświeżenie strony (F5) ją gubi.

**`ApiClient.TryRefreshAsync` musi przekazać flagę:**
```csharp
await tokenStore.SetTokensAsync(tokens.AccessToken, tokens.RefreshToken, tokens.RequiresPasswordChange);
```
Brak trzeciego argumentu = ciche wyzerowanie flagi przy każdym automatycznym odświeżeniu.

**`ChangePassword.razor` nie dziedziczy po `AuthenticatedPage`** — zapętliłby się na własnym przekierowaniu. Ma skopiowaną, okrojoną logikę wczytania tokenów.

**Przycisk „Wyloguj się"** na tym ekranie — jedyne legalne wyjście dla kogoś, kto nie pamięta hasła tymczasowego.

---

# 5. JWT — NOWE CLAIMY

```csharp
string GenerateAccessToken(int userId, int salonId, RoleEnum role,
    string? fullName = null, string? email = null,
    SubscriptionStatusEnum? subscriptionStatus = null);
```

| Claim | Zastosowanie |
|---|---|
| `fullName` | Nagłówek aplikacji (groomer: imię i nazwisko, właściciel: nazwa salonu) |
| `email` | `CustomerEmail` przy tworzeniu sesji Stripe |
| `subscriptionStatus` | Paski ostrzegawcze w `MainLayout` |

## 5.1 Pułapka: mapowanie nazw claimów

ASP.NET Core mapuje standardowe claimy na długie URI. `email` staje się:
```
http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress
```

**`FindFirst("email")` zwróci `null`.** Poprawny odczyt:
```csharp
public string Email => acs.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
```

Własne nazwy (`userId`, `salonId`, `fullName`, `subscriptionStatus`) nie są mapowane — te odczytuje się bez zmian.

> `ResolveDisplayName(user)` w `AuthenticationService`: groomer → imię i nazwisko, właściciel → nazwa salonu, awaryjnie → e-mail. Wymaga `.Include(u => u.Groomer).Include(u => u.Salon)`.

## 5.2 Pułapka: `.Include()` encji z query filtrem

`LoginAsync` z `.Include(u => u.Groomer)` wybucha `NullReferenceException` — `Groomer` ma global query filter odwołujący się do `_currentUser.SalonId`, a przy logowaniu nikt nie jest jeszcze zalogowany.

```csharp
var user = await ctx.Users
    .IgnoreQueryFilters()      // ← konieczne
    .Include(u => u.Groomer)
    .Include(u => u.Salon)
    .Where(u => u.Email == normalizedEmail)
    .FirstOrDefaultAsync(ct);
```

To samo w `RefreshTokenAsync` (endpoint `[AllowAnonymous]`).

## 5.3 Normalizacja e-maila

```csharp
var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
```

Stosowana przy rejestracji, logowaniu i sprawdzaniu duplikatów. **Musi być spójna we wszystkich trzech** — inaczej konto zapisane wielkimi literami nie zaloguje się.

⚠️ **`RegisterGroomerAccountAsync` nadal jej nie ma** — patrz sekcja 14.

---

# 6. WIZYTY POZA CENNIKIEM

## 6.1 Problem, który to rozwiązuje

Do tej pory `Visit.ServiceBreedId` było wymagane — żeby umówić psa, salon musiał najpierw zdefiniować pozycję cennika dla jego rasy. W praktyce kolejność jest odwrotna: najpierw dzwoni klient, potem uzupełnia się cennik.

Cennik miał być **podpowiedzią**, a stał się **bramką**.

## 6.2 Model

```csharp
public class Visit
{
    // opcja B — pozycja cennika (cena i czas ze snapshotu)
    public int? ServiceBreedId { get; set; }
    public ServiceBreed? ServiceBreed { get; set; }

    // opcja A — sama usługa, cena wpisywana ręcznie
    public int? ServiceId { get; set; }
    public Service? Service { get; set; }
}
```

**Dokładnie jedno z dwóch jest wypełnione.**

| | Opcja A — usługa | Opcja B — cennik |
|---|---|---|
| `ServiceId` | wybrane | `null` |
| `ServiceBreedId` | `null` | wybrane |
| Cena | z DTO | snapshot z cennika |
| Czas | z DTO (wymagany) | snapshot, DTO nadpisuje |
| Walidacja rasy | brak | `BreedMismatch` |

## 6.3 Walidacja

```csharp
// dokładnie jedno z dwóch źródeł usługi
if ((dto.ServiceBreedId == null) == (dto.ServiceId == null))
{
    throw new ConflictException(ErrorCodes.ServiceRequired);
}
```

Zwięzły zapis „dokładnie jedno" — prawdziwy, gdy oba `null` albo oba wypełnione.

## 6.4 Projekcje

Nazwa usługi z dwóch źródeł:
```csharp
ServiceName = v.ServiceBreed != null
    ? v.ServiceBreed.Service.Name
    : v.Service!.Name,
BreedName = v.Dog.Breed.Name,      // ZAWSZE z psa, nie z ServiceBreed
```

> `BreedName` musi pochodzić z `Dog.Breed`, bo przy opcji A `ServiceBreed` jest `null`.

## 6.5 Frontend

`AddVisitDialog` ma `MudRadioGroup` przełączający tryb:
- **Z cennika** → istniejący select z `GetServiceBreedDto`, pokazuje nazwę, rasę, cenę i czas
- **Wpisz usługę i cenę** → select z `GetServiceDto` (słownik salonu) + pole ceny

Przy braku pozycji cennika dla rasy komunikat podpowiada wyjście: *„Brak pozycji cennika dla tej rasy — możesz wpisać usługę i cenę ręcznie"*.

**Wybór z cennika podstawia czas trwania** do `DurationPicker` bezwarunkowo (usunięty warunek `InitialEnd == null`).

## 6.6 Ekran `/uslugi` i domyślne usługi

Nowy ekran (`OwnerOnlyPage`) — lista, dodawanie, zmiana nazwy, aktywacja/dezaktywacja. Filtrowanie po stronie klienta.

**`RegisterSalonAsync` tworzy pięć domyślnych usług:**
```csharp
var defaultServices = new[]
{
    "Strzyżenie", "Kąpiel", "Obcinanie pazurów",
    "Kompleksowa pielęgnacja", "Trymowanie"
};

foreach (var name in defaultServices)
{
    newSalon.Services.Add(new Service { Name = name, Status = ActiveStatusEnum.Active });
}
```

> **Przez kolekcję nawigacyjną**, bo `newSalon` nie ma jeszcze `Id`. EF ustawi klucze w odpowiedniej kolejności.

**Nie w `DbSeeder`** — seeder działa tylko lokalnie i tworzy jeden salon testowy.

## 6.7 Słownik ras

Migracja **`SeedBreeds`** wstawia 102 rasy (100 najczęstszych w salonach groomerskich + „Mieszaniec" + „Rasa nieznana").

`DbSeeder` nie tworzy już ras — pobiera Labradora z bazy:
```csharp
var labrador = await context.Breeds.FirstOrDefaultAsync(b => b.Name == "Labrador Retriever");

if (labrador == null)
{
    Console.WriteLine("[SEEDER] Brak ras w bazie — pomijam seedowanie danych testowych.");
    return;
}
```

> `FirstOrDefaultAsync` zamiast `FirstAsync` — seeder to dane testowe, nie powód do niewstania serwera.

---

# 7. NAKŁADANIE WIZYT

## 7.1 Decyzja produktowa

Groomer może mieć kilka psów naraz w różnych fazach (jeden na stole, drugi w suszarce). **Nakładanie ma być normą, bez limitu.**

Walidacja kolizji przestaje być regułą biznesową, a staje się ostrzeżeniem.

## 7.2 Implementacja

DTO (`AddVisitDto`, `EditVisitDto`, `CreateVisitWithNewDogDto`):
```csharp
public bool IgnoreOverlap { get; set; }
```

Serwis:
```csharp
if (!dto.IgnoreOverlap)
{
    var visitOverlaps = await ctx.Visits
        .Where(v => v.GroomerId == dto.GroomerId)
        .Where(v => v.SalonId == salonId)
        .Where(v => v.Status != StatusEnum.Cancelled && v.Status != StatusEnum.NoShow)
        .AnyAsync(v => startTime < v.Date.AddMinutes(v.EstimatedDuration)
                       && endTime > v.Date, ct);

    if (visitOverlaps)
        throw new ConflictException(ErrorCodes.VisitOverlaps);
}
```

> **Blokady czasu (`GroomerTimeOff`) pozostają bezwzględne** — urlop to urlop.
> **`duplicateExists` też zostaje** — dotyczy psa, nie groomera. Jeden pies nie może być w dwóch miejscach.

## 7.3 Frontend

Nowa metoda w `ApiClient` zwracająca kod obok komunikatu:
```csharp
public static async Task<(string Code, string Message)> ReadErrorWithCodeAsync(HttpResponseMessage response)
```

W `AddVisitDialog.SaveAsync`:
```csharp
var (code, message) = await ApiClient.ReadErrorWithCodeAsync(response);

if (code == "VISIT_OVERLAPS" && !ignoreOverlap)
{
    isSaving = false;
    var confirmed = await ConfirmOverlapAsync();
    if (!confirmed) return;

    ignoreOverlap = true;
    await SaveAsync();      // ponowna próba z flagą
    return;
}
```

> **Porównujemy kod, nie przetłumaczony tekst** — inaczej zmiana komunikatu psułaby logikę.

**Nowy komponent `ConfirmDialog.razor`** — osobny od `MessageDialog`, bo tamten informuje, a ten pyta. Parametry: `Message`, `ConfirmText`, `CancelText`, `ConfirmColor`.

---

# 8. STRIPE — INTEGRACJA

## 8.1 Konfiguracja

Produkt „Grooming Management App", cena cykliczna **89 zł / miesiąc**, PLN.

⚠️ **Product tax code jest wymagany** przy włączonych Managed Payments — bez niego tworzenie sesji zwraca błąd.

```json
"Stripe": {
  "SecretKey": "sk_test_...",
  "PriceId": "price_...",
  "WebhookSecret": "whsec_...",
  "SuccessUrl": "https://.../ustawienia?platnosc=ok",
  "CancelUrl": "https://.../ustawienia?platnosc=anulowana"
}
```

## 8.2 `StripeService`

**Trzy odpowiedzialności:**

```csharp
Task<string> CreateCheckoutSessionAsync(int salonId, string salonName, string email, CancellationToken ct);
Task<string> CreatePortalSessionAsync(string customerId, CancellationToken ct);
Task HandleWebhookAsync(string json, string signature, CancellationToken ct);
```

**`salonId` w metadanych** — kluczowe, bo webhook przychodzi bez kontekstu aplikacji:
```csharp
ClientReferenceId = salonId.ToString(),
Metadata = new Dictionary<string, string> { ["salonId"] = salonId.ToString() },
SubscriptionData = new SessionSubscriptionDataOptions
{
    Metadata = new Dictionary<string, string> { ["salonId"] = salonId.ToString() }
}
```

`SubscriptionData.Metadata` przenosi identyfikator na samą subskrypcję, więc kolejne miesięczne faktury też go mają.

## 8.3 Obsługiwane zdarzenia

| Zdarzenie | Działanie |
|---|---|
| `checkout.session.completed` | Zapis `ProviderCustomerId` i `ProviderSubscriptionId` |
| `invoice.paid` | `RegisterPaymentAsync` — przedłużenie o miesiąc, status `Active` |
| `invoice.payment_failed` | `MarkPaymentFailedAsync` |
| `customer.subscription.updated` | Zapis `SubscriptionCancelAtPeriodEnd` |
| `customer.subscription.deleted` | Wyzerowanie `ProviderSubscriptionId` |

## 8.4 Pułapka: kolejność zdarzeń

**Stripe nie gwarantuje kolejności.** W praktyce `invoice.paid` przychodzi **przed** `checkout.session.completed` — czyli zanim salon ma zapisany `ProviderCustomerId`.

Bez zabezpieczenia pierwsza płatność przepada.

**Fallback na metadane subskrypcji:**
```csharp
var salonId = await subscriptionService.GetSalonIdByCustomerIdAsync(invoice.CustomerId, ct);

if (salonId == null)
    salonId = await ResolveSalonIdFromSubscriptionAsync(invoice, ct);
```

```csharp
private async Task<int?> ResolveSalonIdFromSubscriptionAsync(Invoice invoice, CancellationToken ct)
{
    var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;
    if (string.IsNullOrEmpty(subscriptionId)) return null;

    var subService = new Stripe.SubscriptionService();
    var subscription = await subService.GetAsync(subscriptionId, cancellationToken: ct);

    if (subscription.Metadata?.TryGetValue("salonId", out var raw) == true
        && int.TryParse(raw, out var salonId))
    {
        // dowiąż, żeby kolejne faktury trafiały pierwszą drogą
        await subscriptionService.LinkProviderIdsAsync(salonId, invoice.CustomerId, subscriptionId, ct);
        return salonId;
    }

    return null;
}
```

## 8.5 Idempotencja

`RegisterPaymentAsync` sprawdza `ProviderId` przed zapisem i rzuca `ConflictException`. Webhook **łapie ten wyjątek i zwraca 200**:

```csharp
catch (ConflictException)
{
    logger.LogInformation("Invoice {InvoiceId} already processed, ignoring", invoice.Id);
}
```

> Stripe gwarantuje dostarczenie **co najmniej raz**. Zwrócenie błędu spowodowałoby ponawianie w nieskończoność.

## 8.6 Weryfikacja podpisu

Endpoint jest `[AllowAnonymous]` — uwierzytelnienie odbywa się przez podpis:

```csharp
try
{
    stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);
}
catch (StripeException ex)
{
    logger.LogWarning(ex, "Invalid Stripe webhook signature");
    throw new UnauthorizedException(ErrorCodes.InvalidCredentials);
}
```

Bez tego każdy mógłby przedłużyć sobie subskrypcję zwykłym POST-em.

## 8.7 Anulowanie subskrypcji

```csharp
public bool SubscriptionCancelAtPeriodEnd { get; set; }   // na Salon
```

Portal Stripe domyślnie anuluje **na koniec okresu** — subskrypcja nadal istnieje z flagą `cancel_at_period_end`, a `customer.subscription.deleted` przyjdzie dopiero po wygaśnięciu.

Dlatego obsługujemy `customer.subscription.updated` i zapisujemy wartość flagi (nie ustawiamy na sztywno — przy wznowieniu wraca `false`).

Front pokazuje ostrzeżenie z datą wygaśnięcia.

## 8.8 Ekran subskrypcji

Blok w `/ustawienia`, nie osobna strona. Zawiera:
- Alert zależny od statusu (`Suspended` / `PastDue` / `Trial`)
- Status i data ważności
- Przycisk zależny od `Status == Active`: „Zarządzaj płatnościami" (portal) albo „Opłać abonament" (checkout)
- Historia płatności z linkami do faktur

**Przekierowanie przez JS interop:**
```csharp
await JS.InvokeVoidAsync("open", url, "_blank");
```
`NavigationManager.NavigateTo` z zewnętrznym URL bywa kapryśne w Blazor Server.

**`url.Trim('"')`** — endpoint zwraca stringa przez `Ok(url)`, więc przychodzi w cudzysłowach JSON-a.

## 8.9 Odświeżenie tokenu po płatności

Status subskrypcji jest w JWT, więc po opłaceniu pasek ostrzegawczy zniknąłby dopiero po 30 minutach. Rozwiązanie w `SalonSettings.razor`:

```csharp
await LoadDataAsync();

var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

if (query["platnosc"] == "ok"
    && subscription?.Status == SubscriptionStatusEnum.Active
    && TokenStore.SubscriptionStatus != "Active")
{
    await Api.RefreshTokenAsync();
}
```

Odświeża token **tylko gdy baza już wie o płatności, a token jeszcze nie** — czyli po dotarciu webhooka.

Wymaga wystawienia metody w `ApiClient`:
```csharp
public async Task<bool> RefreshTokenAsync() => await TryRefreshAsync();
```

## 8.10 Testowanie lokalne

```powershell
.\stripe.exe listen --forward-to https://localhost:7250/api/Subscription/webhook --skip-verify
```

⚠️ **`whsec_` zmienia się przy każdym uruchomieniu CLI** — trzeba podmienić w konfiguracji i zrestartować backend.

Karta testowa: `4242 4242 4242 4242`, dowolna data w przyszłości, dowolne CVC.

---

# 9. KODY BŁĘDÓW

## 9.1 Decyzja

Komunikaty wyjątków trafiały wprost na ekran użytkownika po angielsku. Wybrano **kody błędów** zamiast tłumaczenia komunikatów w serwisach — żeby backend pozostał neutralny językowo i żeby dodanie drugiego języka nie wymagało jego dotykania.

## 9.2 Backend

`Exceptions/ErrorCodes.cs` — stałe pogrupowane w sekcje:
```csharp
public const string DogNotFound = "DOG_NOT_FOUND";
public const string VisitOverlaps = "VISIT_OVERLAPS";
public const string ServiceRequired = "SERVICE_REQUIRED";
// ... ~50 kodów
```

Użycie:
```csharp
throw new NotFoundException(ErrorCodes.DogNotFound);
```

> **Stałe zamiast gołych stringów** — literówka nie skompiluje się, a IDE podpowie listę.

## 9.3 Frontend

`Services/ErrorMessages.cs` — słownik:
```csharp
public static string Translate(string? code)
{
    if (string.IsNullOrWhiteSpace(code))
        return "Wystąpił nieoczekiwany błąd";

    return Translations.TryGetValue(code, out var text) ? text : code;
}
```

Podpięte w `ApiClient.ReadErrorAsync`.

## 9.4 Zasada

> **Nowy `throw` = nowa stała + wpis w słowniku, w jednym commicie.**

Brak wpisu = użytkownik widzi `SALON_NOT_FOUND` na ekranie.

**Wyjątki, które NIE dostają kodów:** `InvalidOperationException` z konfiguracji i podobne — nie trafiają do użytkownika, `GlobalExceptionHandler` zamieni je na ogólne 500.

## 9.5 Pułapka napotkana przy przepisywaniu

Kilkukrotnie podmieniono komunikat na **kod pasujący do typu wyjątku, ale nie do warunku**:
- duplikat telefonu → `BreedNotFound` zamiast `PhoneTaken`
- istniejąca usługa → `ServiceNotFound` zamiast `ServiceNameTaken`
- niezgodne hasła → `InvalidCredentials` zamiast `PasswordsDoNotMatch`

Kompilator tego nie łapie. **Przy podmianie sprawdzać, czy kod pasuje do warunku, a nie tylko do typu wyjątku.**

---

# 10. WALIDACJA FORMATOWA

`Exceptions/Validate.cs` — statyczna klasa pomocnicza:

```csharp
public static void NotEmpty(string? value, string errorCode);
public static void Email(string? value);
public static void PolishPhone(string? value);
public static void PolishPostalCode(string? value);      // pusty = OK, pole opcjonalne
public static string? NormalizePostalCode(string? value); // → format 00-000
```

**E-mail bez regexa** — sprawdzenie obecności `@`, kropki w domenie, braku spacji. Pełna walidacja RFC 5322 to potwór, a uproszczone regexy odrzucają poprawne adresy.

**Telefon przez wyciągnięcie cyfr** — 9 cyfr albo 11 z prefiksem `48`. Dzięki temu `+48 123 456 789`, `123-456-789` i `123456789` przechodzą tak samo.

**Kod pocztowy zwraca przy pustym** — cały adres jest opcjonalny, więc pusty kod nie może blokować rejestracji.

## 10.1 Zasada `.Trim()` przy zapisie

Wszędzie, gdzie sprawdzasz duplikat i zapisujesz, **obie operacje muszą używać tej samej wartości**:

```csharp
var trimmedName = newName.Trim();

var exists = await ctx.Services.AnyAsync(s => s.Name == trimmedName && s.SalonId == salonId, ct);
// ...
var newService = new Service { Name = trimmedName, ... };
```

Sprawdzanie surowego i zapisywanie przyciętego = duplikaty w bazie.

---

# 11. LOGOWANIE

`Console.WriteLine` zastąpione przez `ILogger<T>` w czterech miejscach: oba schedulery, `MockSmsService`, `GlobalExceptionHandler`.

## 11.1 Składnia strukturalna

```csharp
logger.LogError(ex, "Failed to send reminder for visit {VisitId} in salon {SalonId}",
    visit.Id, visit.SalonId);
```

**Bez `$`** — placeholdery są strukturalne, logger zapisze wartości jako osobne pola do filtrowania.

**Wyjątek jako pierwszy parametr**, nie w treści komunikatu — inaczej tracisz stack trace.

## 11.2 `GlobalExceptionHandler` — dwa poziomy

```csharp
if (status == StatusCodes.Status500InternalServerError)
    logger.LogError(exception, "Unhandled exception on {Method} {Path}", ...);
else
    logger.LogInformation("Handled {ExceptionType} on {Method} {Path}: {Message}", ...);
```

`NotFoundException` i `ConflictException` to normalna praca aplikacji — logowanie ich jako błędy utopiłoby prawdziwe problemy.

## 11.3 Wyłączenie EventLog

Na Windowsie logger próbował pisać do dziennika zdarzeń po zamknięciu hosta, rzucał `ObjectDisposedException` **poza** `try/catch` i kładł aplikację.

```json
"Logging": {
  "EventLog": { "LogLevel": { "Default": "None" } }
}
```

## 11.4 `BackgroundService` — poprawna obsługa anulowania

```csharp
while (!stoppingToken.IsCancellationRequested)
{
    try
    {
        // ... cykl ...
    }
    catch (OperationCanceledException)
    {
        break;                              // ← PRZED ogólnym catch
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Cycle failed");
    }

    try
    {
        await Task.Delay(_interval, stoppingToken);
    }
    catch (OperationCanceledException)
    {
        break;                              // ← Task.Delay też rzuca
    }
}
```

> **`Task.Delay` poza głównym `try`** — inaczej po wyjątku pętla kręciłaby się bez przerwy.
> **`OperationCanceledException` przed `Exception`** — kolejność `catch` ma znaczenie.

Domyślne `BackgroundServiceExceptionBehavior.StopHost` sprawia, że jeden nieobsłużony wyjątek zabija całą aplikację.

---

# 12. SPRZĄTANIE TOKENÓW

Nowy `TokenCleanupScheduler`, interwał 24h:

```csharp
var cutoff = DateTime.UtcNow.AddDays(-30);

var deleted = await ctx.RefreshTokens
    .Where(t => t.ExpiresAt < cutoff)
    .ExecuteDeleteAsync(stoppingToken);
```

**`ExecuteDeleteAsync`** — jedno `DELETE` w SQL, bez ładowania encji.

**Margines 30 dni** po wygaśnięciu, nie natychmiast — zostawia ślad historii logowań.

**Warunek tylko po `ExpiresAt`** — token zrewokowany, ale technicznie ważny, zostaje, żeby próba użycia dawała jasny błąd „już zrewokowany".

---

# 13. POZOSTAŁE ZMIANY

## 13.1 `ApiClient` przestaje połykać błędy

```csharp
if (response.StatusCode == HttpStatusCode.NotFound)
    return default;

if (!response.IsSuccessStatusCode)
{
    var message = await ReadErrorAsync(response);
    throw new ApiException(response.StatusCode, message);
}
```

Nowy typ `ApiException(HttpStatusCode statusCode, string message)`.

> **404 nadal zwraca `default`** — „nie znaleziono" to normalna odpowiedź przy pobieraniu pojedynczego zasobu, a strony obsługują to przez `== null`.

**Skutek:** strony mają już `try/catch` z komunikatem, więc od razu zaczęły pokazywać prawdziwy powód (403, 402, 500) zamiast pustej listy.

**Powód zmiany:** dwie długie diagnozy w tej sesji (kolory w kalendarzu, pusta lista groomerów) wynikały z cichego `return default` przy 403.

## 13.2 Ujednolicone rejestracje DI

`Extensions/ServiceCollectionExtensions.cs`:

```csharp
public static IServiceCollection AddScopedWithInterfaces<TImplementation>(
    this IServiceCollection services,
    params Type[] interfaces)
    where TImplementation : class
{
    services.AddScoped<TImplementation>();

    foreach (var iface in interfaces)
        services.AddScoped(iface, sp => sp.GetRequiredService<TImplementation>());

    return services;
}
```

Użycie:
```csharp
builder.Services.AddScopedWithInterfaces<VisitService>(
    typeof(IVisitReaderService), typeof(IVisitWriterService));
```

**Wszystkie interfejsy wskazują na tę samą instancję** w obrębie żądania. Poprzedni wzorzec (`AddScoped<IReader, X>()` + `AddScoped<IWriter, X>()`) tworzył dwie.

⚠️ **Przy okazji wykryto zduplikowany `AddDbContext`** — drugi nadpisywał pierwszy.

## 13.3 Wykres w raportach

MudBlazor 9 zmienił nazwę parametru: **`XAxisLabels` → `ChartLabels`**. Stary atrybut wsiąkał bez błędu, przez co słupki nie miały podpisów.

```razor
<MudChart ChartType="ChartType.Bar"
          ChartSeries="Series"
          ChartLabels="ChartLabels"
          Width="100%" Height="350px" />
```

`ChartSeries<double>` z generykiem jest poprawne dla v9.

**Sortowanie po dacie w obu miejscach** — etykiety i słupki muszą mieć tę samą kolejność:
```csharp
Data = byDay.OrderBy(d => d.Day).Select(d => (double)d.Earnings).ToArray()
private string[] ChartLabels => byDay.OrderBy(d => d.Day).Select(d => d.Day.ToString("dd.MM")).ToArray();
```

## 13.4 `DbSeeder` z hashowanym hasłem

```csharp
public static async Task SeedAsync(GroomingDbContext context, IPasswordHasher passwordHasher)
{
    private const string DefaultPassword = "Test1234!";
    var hashedPassword = passwordHasher.HashPassword(DefaultPassword);
    // ...
}
```

Wywołanie w `Program.cs`:
```csharp
var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
await DbSeeder.SeedAsync(context, passwordHasher);
```

**Konta testowe:** `owner@test.com` i `anna@test.com`, hasło `Test1234!`. Anna ma `RequiresPasswordChange = true`.

Uzupełniono też brakujące pola: subskrypcja salonu, uprawnienia groomerów, `SettlementType`, snapshot rozliczenia na wizycie, `Status` na usługach.

## 13.5 Wyszukiwanie w listach

`MudTable` z `Filter` i polem w `ToolBarContent`:

```razor
<MudTable Items="dogs" Filter="FilterDog" ...>
    <ToolBarContent>
        <MudTextField @bind-Value="searchText"
                      Placeholder="Szukaj po imieniu, rasie lub właścicielu"
                      Adornment="Adornment.Start"
                      AdornmentIcon="@Icons.Material.Filled.Search"
                      Immediate="true" Clearable="true" Class="mt-0" />
    </ToolBarContent>
```

Filtrowanie po stronie klienta — przy kilkuset rekordach wystarczy.

**Wyszukiwanie po telefonie ignoruje formatowanie:**
```csharp
var digits = new string(searchText.Where(char.IsDigit).ToArray());
return ... || (digits.Length > 0 && owner.Phone.Contains(digits));
```

## 13.6 Ustawienia przypomnień

```csharp
public class Salon
{
    public bool RemindersEnabled { get; set; }      // domyślnie true
    public int ReminderHoursBefore { get; set; }    // domyślnie 24
}
```

Sekcja w `/ustawienia` — **obecnie wyszarzona** (`Disabled`, opacity 0.6, chip „Wkrótce"), bo SMS-y nie działają.

⚠️ **`ReminderScheduler` nadal ma sztywne 24h i nie sprawdza `RemindersEnabled`.** Encja, DTO i UI gotowe, sam scheduler nie.

Select ogranicza wybór do 2/6/12/24/48 godzin — bo okno wyszukiwania w schedulerze ma szerokość interwału (20 min), więc drobne wartości mogłyby wypadać.

## 13.7 Telefon salonu

```csharp
public string? Phone { get; set; }   // Salon, HasMaxLength(20)
```

Wymagany przy rejestracji, opcjonalny przy edycji (istniejące salony go nie mają).

**Powód:** kontakt z klientem oraz sygnał przy powtórnych rejestracjach (nie wymuszamy unikalności — właściciel dwóch salonów to realna sytuacja).

## 13.8 Czarna lista tylko dla właściciela

`BlacklistController` → `[Authorize(Roles = "Owner")]` na klasie.
`Blacklist.razor` → `OwnerOnlyPage`.

> `IsBlockedAsync` jest wołane wewnętrznie z `VisitService`, więc groomer nadal dostanie blokadę przy dodawaniu wizyty — tylko nie zobaczy samej listy.

## 13.9 `MinBookingHoursAhead` / `MaxBookingDaysAhead`

Pola **nie są już aktualizowane** w `UpdateSalonAsync`:

```csharp
// MinBookingHoursAhead i MaxBookingDaysAhead celowo nieaktualizowane —
// pola istnieją dla AvailabilityService, ale nie są wystawione w UI.
// Wrócą, gdy powstanie portal klienta.
```

**Powód:** formularz ich nie wysyłał, więc DTO przychodziło z zerami i walidacja `MaxBookingDaysAhead <= 0` blokowała zapis nazwy salonu.

---

# 14. ZNANE BRAKI I DŁUG TECHNICZNY

## 14.1 Blokuje sprzedaż

| Obszar | Stan |
|---|---|
| **SMS** | `MockSmsService` — brak integracji z providerem |
| **`ReminderScheduler`** | Sztywne 24h, ignoruje ustawienia salonu |
| **Stripe produkcja** | Webhook niezarejestrowany, `SuccessUrl`/`CancelUrl` niepoprawne, klucze testowe |
| **Formalne** | Brak działalności, regulaminu, polityki prywatności, umowy powierzenia (RODO) |

## 14.2 Przed pierwszym klientem

- **`RegisterGroomerAccountAsync` nie normalizuje e-maila** — konto zapisane z wielkimi literami nie zaloguje się, bo `LoginAsync` szuka po `ToLowerInvariant()`. **To realny bug blokujący logowanie groomerom.**
- **Widok mobilny** — nieprzetestowany, a groomer pracuje przy stole z telefonem
- **AdBlock a Application Insights** — podejrzenie, że bloker reklam uniemożliwia załadowanie strony. Niezweryfikowane.
- **Domena** — aplikacja na adresach `azurewebsites.net`
- **Pełny test end-to-end** — po wszystkich zmianach nieprzeprowadzony

## 14.3 Dług techniczny

**Nierozwiązane:**
- **Testy automatyczne** — nadal zero
- **Wydzielenie DTO do projektu `Shared`** — frontend referencuje cały backend z EF Core, Stripe.net i połączeniem do bazy
- **GitHub Actions** — wdrożenie nadal ręczne, dwa razy Publish
- **`AttemptCount` w `Notification`** — pole istnieje, logika ponawiania nie (odłożone do integracji SMS)

**Rozwiązane w tej sesji:**
- ✅ `Microsoft.EntityFrameworkCore.SqlServer` usunięty
- ✅ Ostrzeżenie EF o `DogOwner`/`Notification` wyciszone z komentarzem
- ✅ Sprzątanie wygasłych `RefreshToken`
- ✅ `ApiClient` rzuca wyjątkiem zamiast połykać
- ✅ Indeksy na `SalonId` — zweryfikowane, EF tworzy je automatycznie
- ✅ Ujednolicone rejestracje DI
- ✅ Konwencja stref czasowych
- ✅ Kody błędów z tłumaczeniami
- ✅ Walidacja formatowa

**Świadomie odłożone:**
- Raporty popularnych usług, ras, obciążenia groomerów — brak danych, żeby cokolwiek pokazać
- Portal klienta — kod w historii gita
- Zdjęcia przed/po

---

# 15. NOWA CHECKLISTA POWTARZAJĄCYCH SIĘ BŁĘDÓW

Do listy z poprzednich wersji dochodzą:

| Błąd | Objaw |
|---|---|
| **`var` przy przypisaniu do pola klasy** | Kompiluje się, pole zostaje puste, cicha awaria |
| **`.Include()` encji z query filtrem bez zalogowanego** | `NullReferenceException` z `CurrentUserService` |
| **`return x;` gdy `x == null` w `ActionResult<T>`** | 204 zamiast 200, front wywala się na parsowaniu JSON |
| **Claim mapowany na długie URI** | `FindFirst("email")` zwraca `null` |
| **Kod błędu pasujący do typu, nie do warunku** | Użytkownik widzi komunikat o czymś innym |
| **Sprawdzanie surowego, zapis przyciętego** | Duplikaty w bazie mimo walidacji |
| **`Task.Delay` poza `try` w `BackgroundService`** | `TaskCanceledException` kładzie hosta |
| **`DateTime` z `DateOnly.ToDateTime()` w jednym porównaniu** | Npgsql odrzuca — dwa różne typy kolumn |
| **Migracja utworzona, ale niezastosowana** | `Invalid column name` przy działającym kodzie |
| **Wartość domyślna parametru ukrywa pominięty argument** | Kompiluje się, claim pusty |

---

# 16. STAN NA DZIŚ — PODSUMOWANIE

**Aplikacja jest kompletna funkcjonalnie i działa na Azure.**

Salon może: zarejestrować się, dodać pracowników z kontami i uprawnieniami, prowadzić kartotekę klientów i psów, umawiać wizyty (z cennika albo poza nim, z nakładaniem), zarządzać grafikami i blokadami, rozliczać pracowników, przeglądać raporty, opłacić abonament i zarządzać subskrypcją.

**Czego brakuje do sprzedaży:** SMS-y, formalności prawne, Stripe w trybie live, widok mobilny.

**Kolejność, która ma sens:**
1. Pokaz aplikacji domain expertowi (pytania o cennik czekają od tygodnia)
2. Formalności — wniosek CEIDG, bo trwa niezależnie
3. Widok mobilny
4. SMSAPI
5. Stripe live

---

**Konta testowe (lokalnie):** `owner@test.com`, `anna@test.com` — hasło `Test1234!`
