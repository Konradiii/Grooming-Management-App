# Grooming Management App — dokumentacja techniczna v2

**Data:** 26.08.2026
**Zakres:** aktualizacja dokumentacji z 13.08.2026 — obejmuje dokończenie Fazy 4, całą Fazę 5 (monetyzacja), rozliczenia pracowników oraz **kompletny frontend w Blazor Server**

> Ten dokument **zastępuje** poprzednią dokumentację w częściach, których dotyczy.
> Rzeczy nieopisane tutaj (Faza 1–3, konwencje, wzorce) pozostają zgodne z poprzednią wersją.

---

# 0. NAJWAŻNIEJSZE ZMIANY W SKRÓCIE

| Obszar | Stan |
|---|---|
| **Faza 4** | ✅ ukończona — dostępność liczona w locie, przetestowana |
| **Faza 5** | ✅ logika ukończona — subskrypcje, płatności, middleware, scheduler (brak integracji Stripe) |
| **Rozliczenia pracowników** | ✅ nowa funkcja — godzinowe/procentowe + pomoc na wizycie |
| **Portal klienta** | ⏸️ **świadomie odłożony** — kod napisany, zakomentowany |
| **Frontend** | ✅ nowy projekt Blazor Server, ~15 ekranów, ~16 dialogów |
| **Refaktor ISP** | ✅ interfejsy podzielone na Reader/Writer |
| **Solution** | projekt backendu przemianowany na `Grooming-Management-Backend` |

---

# 1. STRUKTURA SOLUTION (ZMIANA)

```
Grooming-Management-App.sln
├── Grooming-Management-Backend/     (ASP.NET Core Web API, .NET 10)
│   └── RootNamespace: Grooming_Management_App   ← namespace NIE zmieniony
└── Grooming-Management-Frontend/    (Blazor Server, .NET 10)
    └── RootNamespace: Grooming_Management_Frontend
```

**Frontend ma referencję do projektu backendu** — dzięki temu współdzielą DTO i enumy bez generowania klienta HTTP. To główny powód wyboru Blazora.

> **Konsekwencja:** zmiana pola w DTO backendu natychmiast wpływa na frontend.
> Ostrzeżenie `CS0436` o konflikcie typu `Program` jest nieszkodliwe.

---

# 2. ZMIANY W ENCJACH

## 2.1 `Salon` — rozszerzony o subskrypcję

```csharp
public class Salon
{
    // ... pola z poprzedniej dokumentacji ...

    public DateOnly? SubscriptionValidUntil { get; set; }   // null = bezterminowo (konto testowe)
    public SubscriptionStatusEnum SubscriptionStatus { get; set; }
    public string? ProviderCustomerId { get; set; }         // cus_xxx ze Stripe
    public string? ProviderSubscriptionId { get; set; }     // sub_xxx ze Stripe

    public List<Payment> Payments { get; set; } = new();
}
```

> **Nazwy pól celowo bez „Stripe"** — abstrakcja od providera, spójnie z `Payment.ProviderId`.

**Zmiana domyślnych wartości:**
- `MinBookingHoursAhead = 0` (było 24) — salon nie potrzebuje ograniczenia wyprzedzenia
- `MaxBookingDaysAhead = 550` (było 90) — ~1,5 roku, żeby nie blokować odległych rezerwacji

Oba pola **nie są widoczne w ustawieniach salonu** — istnieją tylko dla `AvailabilityService`.

## 2.2 `Visit` — snapshot rozliczenia + pomoc

```csharp
public class Visit
{
    // ... pola z poprzedniej dokumentacji ...

    public SettlementTypeEnum SettlementType { get; set; }   // SNAPSHOT z Groomer
    public decimal SettlementRate { get; set; }              // SNAPSHOT z Groomer

    public int? AssistantGroomerId { get; set; }             // pomoc przy wizycie
    public Groomer? AssistantGroomer { get; set; }
}
```

> **Snapshot rozliczenia** działa tak samo jak `ProposedPrice`: kopiowany z groomera przy tworzeniu
> wizyty i nigdy nieaktualizowany. Podwyżka stawki nie zmienia rozliczeń z przeszłości.

**Konfiguracja relacji z pomocą** (`Groomer` ma teraz DWIE relacje z `Visit`):
```csharp
builder.HasOne(v => v.AssistantGroomer)
    .WithMany()                                   // bez kolekcji po stronie Groomer
    .HasForeignKey(v => v.AssistantGroomerId)
    .OnDelete(DeleteBehavior.Restrict);
```

## 2.3 `Groomer` — rozliczenie

```csharp
public SettlementTypeEnum SettlementType { get; set; }
public decimal SettlementRate { get; set; }       // zł/h albo %, zależnie od typu
```

## 2.4 `DogOwner` — powiązanie z kontem

```csharp
public int? UserId { get; set; }     // nullable — klient może nie mieć konta
public User? User { get; set; }      // DeleteBehavior.SetNull
```

Unikalny indeks z filtrem `[UserId] IS NOT NULL` — jedno konto na jednego klienta,
ale dowolnie wielu klientów bez konta.

## 2.5 `User` — pola, których nie było w poprzedniej dokumentacji

```csharp
public ActiveStatusEnum ActiveStatus { get; set; }
public DateTime CreatedAt { get; set; }
public Groomer? Groomer { get; set; }
public DogOwner? DogOwner { get; set; }
```

> ⚠️ **`ActiveStatus` nie jest sprawdzany w `LoginAsync`** — dezaktywowane konto nadal się zaloguje.
> Do naprawy.

## 2.6 `Payment` — NOWA ENCJA

```csharp
public class Payment
{
    public int Id { get; set; }
    public decimal Amount { get; set; }              // precision 18,2
    public string Currency { get; set; }             // ISO 4217, maxLength 3
    public DateTime PaymentDate { get; set; }
    public string ProviderId { get; set; }           // UNIKALNY INDEKS — ochrona przed podwójnym webhookiem
    public PaymentStatusEnum Status { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public string? InvoiceUrl { get; set; }

    public int SalonId { get; set; }
    public Salon Salon { get; set; }
}
```

> **Unikalny indeks na `ProviderId`** jest kluczowy — Stripe gwarantuje dostarczenie webhooka
> *co najmniej raz*, nie *dokładnie raz*.

## 2.7 Enumy — zmiany

```csharp
// ⚠️ ZMIENIONA NUMERACJA — dopasowana do System.DayOfWeek
public enum DayOfWeekEnum
{
    Sunday = 0, Monday = 1, Tuesday = 2, Wednesday = 3,
    Thursday = 4, Friday = 5, Saturday = 6
}
// Poprzednio: Monaday = 1 ... Sunday = 7 (literówka + zła numeracja)
// Dzięki zmianie działa (DayOfWeekEnum)date.DayOfWeek bez konwersji

public enum RoleEnum { Owner, Groomer, Client }        // Client dodany na KOŃCU
public enum SubscriptionStatusEnum { Trial, Active, PastDue, Suspended }
public enum PaymentStatusEnum { Succeeded, Failed, Refunded }
public enum SettlementTypeEnum { None, Hourly, Percentage }
```

---

# 3. FAZA 4 — DOSTĘPNOŚĆ (UKOŃCZONA)

## 3.1 `IAvailabilityReaderService` / `AvailabilityService`

```csharp
Task<List<GetAvailabilityDto>> GetAvailabilitySlotsAsync(
    int salonId, DateOnly date, int serviceBreedId, int? groomerId, CancellationToken ct);
```

**Kontrakt:**
- Zwraca **jeden element per groomer**. `groomerId` podany → lista jednoelementowa.
- `groomerId = null` → wszyscy aktywni groomerzy salonu.
- Groomer bez wolnych slotów też trafia na listę, z pustą `AvailableSlots`.
- `AvailableSlots` to godziny **startu** wizyty w formacie `"HH:mm"`.

**DTO:**
```csharp
public class GetAvailabilityDto
{
    public int GroomerId { get; set; }
    public string GroomerFullName { get; set; }
    public List<string> AvailableSlots { get; set; } = new();   // ⚠️ = new() OBOWIĄZKOWO
    public DateOnly Date { get; set; }
    public int ServiceDurationMinutes { get; set; }
}
```

**Warunki dostępności slotu `T`:**
1. Cały przedział `[T, T + duration]` mieści się **wewnątrz jednego wpisu** `GroomerSchedule`
2. Nie koliduje z żadną `Visit` (pomijane: `Cancelled`, `NoShow`)
3. Nie koliduje z `GroomerTimeOff`
4. Mieści się w oknie `[teraz + MinBookingHoursAhead, teraz + MaxBookingDaysAhead]`

## 3.2 Kluczowe decyzje implementacyjne

**Stała liczba zapytań (5), niezależna od liczby groomerów.** Grafiki, wizyty i blokady
pobierane raz przez `groomerIds.Contains(...)`, potem grupowane w pamięci.

**Cała arytmetyka w minutach od północy**, nie na `TimeOnly`:
```csharp
private static int ToMinutes(TimeOnly time) => time.Hour * 60 + time.Minute;
```
Powód: `TimeOnly.AddMinutes` zawija się przez północ — wizyta 23:30 + 60 min dałaby 00:30
i warunek nakładania przestałby działać.

**Wzorzec nakładania** (ten sam co w `GroomerScheduleService`):
```
nowyStart < istniejącyEnd  AND  nowyEnd > istniejącyStart
```
Znaki **ostre** — dzięki temu wizyty stykające się (koniec 12:00, start 12:00) nie kolidują.

## 3.3 Przetestowane zachowania ✅

- Grafik 7:00–17:00, usługa 120 min → ostatni slot **15:00** (nie 16:45)
- Wizyta 12:00–14:00 → znika 13 slotów (10:15–13:45), **zostaje 10:00 i 14:00**
- Kluczowa obserwacja: usługa 120 min blokuje 2h wizyty **plus 2h rozbiegu**

---

# 4. WALIDACJE KOLIZJI (NOWE)

## 4.1 W `AddVisitAsync` — dodane

```csharp
// czas trwania: z DTO albo z cennika
var duration = dto.DurationMinutes ?? serviceBreed.Duration;
var startTime = dto.Date;
var endTime = dto.Date.AddMinutes(duration);

// kolizja z inną wizytą tego groomera
var visitOverlaps = await ctx.Visits
    .Where(e => e.GroomerId == dto.GroomerId)
    .Where(d => d.SalonId == salonId)
    .Where(d => d.Status != StatusEnum.Cancelled && d.Status != StatusEnum.NoShow)
    .AnyAsync(d => startTime < d.Date.AddMinutes(d.EstimatedDuration)
                && endTime > d.Date, ct);

// kolizja z blokadą czasu
var timeOffOverlaps = await ctx.GroomerTimeOffs
    .Where(t => t.SalonId == salonId)
    .Where(t => t.GroomerId == dto.GroomerId)
    .AnyAsync(t => startTime < t.EndDate.ToDateTime(t.EndTime)
                && endTime > t.StartDate.ToDateTime(t.StartTime), ct);
```

## 4.2 W `CreateGroomerTimeOffAsync` — dodane

Blokada czasu **nie może** powstać, jeśli groomer ma w tym okresie zaplanowane wizyty.
Świadoma decyzja produktowa: właściciel musi najpierw odwołać lub przełożyć wizyty.

## 4.3 ⚠️ ZNANA DZIURA

**`EditVisitAsync` NIE waliduje kolizji.** Edycją terminu można nałożyć dwie wizyty na siebie.
Do naprawy.

## 4.4 Znane ograniczenie

Blokada **wielodniowa** jest traktowana jako ciągły przedział, a w rzeczywistości to te same
godziny każdego dnia. Blokada 18–20.08 8:00–9:00 zablokuje wizytę 19.08 o 14:00.
Przy blokadach całodniowych i jednodniowych problemu nie ma.

---

# 5. ROZLICZENIA PRACOWNIKÓW (NOWE)

## 5.1 Model

Groomer ma **bieżące** ustawienia (`Groomer.SettlementType`, `SettlementRate`).
Wizyta ma **snapshot** tych wartości z chwili utworzenia.

| Typ | Wyliczenie |
|---|---|
| `Percentage` | `kwota * SettlementRate / 100m` |
| `Hourly` | `EstimatedDuration / 60m * SettlementRate` |
| `None` | `0` |

Kwota = `FinalPrice ?? ProposedPrice`. Liczone tylko dla wizyt `Completed`.

> ⚠️ `60m` i `100m` z sufiksem `m` — bez tego dzielenie całkowite i wizyta 90-minutowa
> policzy się jako godzina.

## 5.2 `GetGroomerSettlementsAsync` w `EarningsService`

```csharp
Task<List<GetGroomerSettlementDto>> GetGroomerSettlementsAsync(
    int salonId, DateTime dateFrom, DateTime dateTo, CancellationToken ct);
```

```csharp
public class GetGroomerSettlementDto
{
    public int GroomerId { get; set; }
    public string GroomerFullName { get; set; }
    public int VisitsCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Settlement { get; set; }
}
```

**Podział pracy:** projekcja i filtrowanie w SQL, grupowanie i liczenie **w pamięci** —
bo `switch` w `CalculateSettlement` nie tłumaczy się na SQL.

## 5.3 Pomoc na wizycie

`Visit.AssistantGroomerId` — nullable. Wersja minimalna: **tylko powiązanie, bez liczenia
wynagrodzenia**.

Walidacja w `AddVisitAsync` i `CreateVisitWithNewDogAsync`:
- pomoc musi być innym groomerem niż prowadzący
- pomoc musi istnieć w tym salonie

> **Powód odłożenia rozliczenia pomocy:** nie wiadomo, czy w branży jest procentowe
> (przypadek mamy Konrada: 25% od psa) czy godzinowe. Do zbadania.

---

# 6. FAZA 5 — MONETYZACJA

## 6.1 Model biznesowy

- **Jeden plan** ze wszystkim (podział na plany dopiero w przyszłości)
- **Stripe**, subskrypcja cykliczna, miesięcznie
- **30 dni triala** przy rejestracji
- Po wygaśnięciu: **tydzień karencji**, potem odcięcie

## 6.2 Cykl życia statusu

```
Trial ──────┐
            ├──► PastDue ──(7 dni)──► Suspended
Active ─────┘         │                    │
   ▲                  │                    │
   └──────── RegisterPaymentAsync ─────────┘
```

**Kto co przestawia:**
- `SubscriptionScheduler` (raz dziennie): `Trial`/`Active` → `PastDue`, potem `PastDue` → `Suspended`
- `RegisterPaymentAsync`: cokolwiek → `Active`

> **Kolejność w schedulerze ma znaczenie.** Najpierw `MarkExpiredSubscriptionsAsPastDueAsync`,
> potem `SuspendExpiredSubscriptionsAsync` — odwrotnie salon przeskoczyłby karencję.

## 6.3 `ISubscriptionService`

```csharp
Task<DateOnly> RegisterPaymentAsync(int salonId, RegisterPaymentDto dto, CancellationToken ct);
Task MarkPaymentFailedAsync(int salonId, RegisterPaymentDto dto, CancellationToken ct);
Task<int> SuspendExpiredSubscriptionsAsync(CancellationToken ct);
Task<int> MarkExpiredSubscriptionsAsPastDueAsync(CancellationToken ct);
```

**Kluczowa reguła w `RegisterPaymentAsync` — od kiedy liczyć nowy okres:**
```csharp
var periodStart = salon.SubscriptionValidUntil > today
    ? salon.SubscriptionValidUntil.Value
    : today;
var periodEnd = periodStart.AddMonths(1);
```
Płacący przed końcem nie traci dni; płacący po wygaśnięciu nie dostaje wstecznej ważności.
`null > today` daje `false`, więc konta bez daty są obsłużone.

**`MarkPaymentFailedAsync`** zapisuje `Payment` ze statusem `Failed`, ale status salonu zmienia
**tylko gdy subskrypcja faktycznie wygasła** — salon opłacony do września z nieudaną próbą
w sierpniu zostaje `Active`.

**Karencja liczona z `SubscriptionValidUntil`**, bez osobnego pola:
```csharp
var graceCutoff = today.AddDays(-GracePeriodDays);   // 7
// szukaj: status == PastDue && SubscriptionValidUntil < graceCutoff
```

## 6.4 `SubscriptionMiddleware`

**Reguła:** czytanie zawsze wolno, **POST zablokowany** dla salonów `Suspended`.

```
1. metoda != POST                    → przepuść
2. ścieżka /api/Auth                 → przepuść (logowanie musi działać)
3. ścieżka /api/Subscription         → przepuść (płatność musi działać!)
4. niezalogowany                     → przepuść
5. brak/nieparsowalny claim salonId  → przepuść
6. pobierz status z bazy
7. Suspended                         → 402 Payment Required
8. przepuść
```

> ⚠️ **Kolejność warunków = kolejność wykonania.** Wyjątek na `/api/Subscription` **musi**
> stać przed sprawdzeniem statusu — inaczej zawieszony salon nie może zapłacić.

**Rejestracja w `Program.cs`** — po `UseAuthentication()`, przed `UseAuthorization()`:
```csharp
app.UseAuthentication();
app.UseMiddleware<SubscriptionMiddleware>();
app.UseAuthorization();
```

**Świadome ograniczenie:** blokowany jest tylko POST. Salon w `Suspended` może przez PUT
przesuwać istniejące wizyty. Uznane za dopuszczalne — zapas wizyt starcza na tydzień lub dwa.

## 6.5 `SubscriptionScheduler`

`BackgroundService`, interwał 24h, wzorzec jak `ReminderScheduler`:
`IServiceScopeFactory`, scope wewnątrz pętli, `try/catch`.

Nie potrzebuje `.IgnoreQueryFilters()`, bo `Salon` nie ma query filtra.

## 6.6 ⚠️ TYMCZASOWY ENDPOINT — DO USUNIĘCIA PRZED PRODUKCJĄ

```csharp
POST /api/Subscription/payment   [Authorize(Roles = "Owner")]
```

Właściciel może wywołać go sam i przedłużyć subskrypcję **za darmo**.
Docelowo: webhook Stripe z `[AllowAnonymous]` i weryfikacją podpisu.

---

# 7. NOWE ENDPOINTY „ALL-IN-ONE"

Trzy metody tworzące kilka encji w **jednej transakcji**, przez kolekcje nawigacyjne
(jeden `SaveChangesAsync`):

| Endpoint | Tworzy | DTO |
|---|---|---|
| `POST /api/Dog/with-owner` | `DogOwner` + `Dog` | `CreateDogWithOwnerDto` |
| `POST /api/ServiceBreed/with-service` | `Service` + `ServiceBreed` | `CreateServiceBreedWithServiceDto` |
| `POST /api/Visit/with-new-dog` | `DogOwner` + `Dog` + `Visit` | `CreateVisitWithNewDogDto` |

**Technika:**
```csharp
var owner = new DogOwner { /* ... */ };
var dog = new Dog { /* ... */ };
owner.Dogs.Add(dog);
var visit = new Visit { /* ... */, Dog = dog, DogOwner = owner };
ctx.DogOwners.Add(owner);
ctx.Visits.Add(visit);
await ctx.SaveChangesAsync(ct);   // JEDEN zapis = jedna transakcja
```

Przypisujesz **obiekty**, nie identyfikatory — EF Core ustawia klucze w odpowiedniej kolejności.

**W `CreateVisitWithNewDogAsync` dochodzi sprawdzenie blacklisty po numerze telefonu:**
```csharp
var blacklistedByPhone = await ctx.Blacklists
    .Where(b => b.SalonId == salonId)
    .AnyAsync(b => b.DogOwner.Phone == dto.Phone, ct);
```
Musi stać **przed** walidacją unikalności telefonu — inaczej salon dostanie mylący komunikat.

---

# 8. ZMIANY W ISTNIEJĄCYCH DTO

| DTO | Nowe pola |
|---|---|
| `AddVisitDto` | `int? DurationMinutes`, `int? AssistantGroomerId` |
| `VisitFilterDto` | `int? DogId` |
| `GetAllVisitsDto` | `GroomerId`, `EstimatedDuration`, `BreedName`, `AssistantGroomerFullName` |
| `GetVisitDetailsDto` | `AssistantGroomerFullName` |
| `GetDogOwnerDto` | `DogsCount` |
| `GetDogDetailsDto` | `DogOwnerId`, `DogOwnerPhone`; `AgeInMonths` zmienione na `int` |
| `GetGroomerDto` | `SettlementType`, `SettlementRate`, `HasAccount` |
| `EditGroomerDto` | `SettlementType`, `SettlementRate` |
| `GetWaitlistDto` | `DogOwnerPhone` |
| `GetSalonDto` / `UpdateSalonDto` | pola subskrypcji |
| `LoginResponseDto` | `RequiresPasswordChange` |

**`DurationMinutes` — ważna zmiana semantyki:**
```csharp
var duration = dto.DurationMinutes ?? serviceBreed.Duration;
```
Czas z cennika to teraz **podpowiedź**, nie sztywna reguła. Groomer sam określa, ile zajmie mu pies.

**Poprawki w `GetAllVisitsAsync`:**
- dodany brakujący filtr po `Status` (pole było w DTO, ale nieużywane)
- `DateFrom` zmienione z `>` na `>=` — przedział domknięty z obu stron

---

# 9. REFAKTOR ISP

Interfejsy serwisów podzielone na `Reader`/`Writer`:

```
IBreedReaderService
IGroomerReaderService / IGroomerWriterService
IDogReaderService / IDogWriterService
IDogOwnerReaderService / IDogOwnerWriterService
IServiceReaderService / IServiceWriterService
IServiceBreedReaderService / IServiceBreedWriterService
IVisitReaderService / IVisitWriterService
IEarningsReaderService
IGroomerScheduleReaderService / IGroomerScheduleWriterService
IGroomerTimeOffReaderService / IGroomerTimeOffWriterService
IWaitlistReaderService / IWaitlistWriterService
IAvailabilityReaderService
IBlacklistService + IBlacklistCheckService     ← jedyny podział wg ISP „z sensem"
ILoginService / IPasswordService / IRegistrationService / ITokenSessionService
```

**Rejestracja w `Program.cs`** — dwa wzorce, niespójne:
```csharp
// wzorzec A (większość) — DWIE OSOBNE instancje na żądanie
builder.Services.AddScoped<IGroomerReaderService, GroomerService>();
builder.Services.AddScoped<IGroomerWriterService, GroomerService>();

// wzorzec B (tylko Blacklist) — JEDNA instancja
builder.Services.AddScoped<BlacklistService>();
builder.Services.AddScoped<IBlacklistService>(sp => sp.GetRequiredService<BlacklistService>());
builder.Services.AddScoped<IBlacklistCheckService>(sp => sp.GetRequiredService<BlacklistService>());
```

> **Do ujednolicenia.** Przy serwisach bezstanowych różnica jest nieszkodliwa, ale gdyby
> któryś zaczął trzymać stan, dostaniesz dwie niezależne kopie.

> **Refleksja z refaktoru:** jedyny podział, który faktycznie realizuje ISP, to
> `IBlacklistCheckService` — bo `VisitService` używa jednej metody z sześciu.
> Pozostałe podziały to konwencja nazewnicza; kontrolery i tak wstrzykują oba interfejsy.

---

# 10. FRONTEND — NOWY PROJEKT

## 10.1 Stack

| Element | Wybór | Uzasadnienie |
|---|---|---|
| Framework | **Blazor Server** | wspólne DTO z backendem, token po stronie serwera, brak nauki nowego języka |
| UI | **MudBlazor 9.8** | MIT, ładniejszy niż Radzen |
| Kalendarz | **Radzen.Blazor 11.2** | MudBlazor nie ma schedulera; Radzen darmowy (MIT) |
| localStorage | **Blazored.LocalStorage 4.5.0** | ⚠️ trzeba wymusić wersję: `--version 4.5.0` |

> **FullCalendar i Bryntum odrzucone** — FullCalendar Premium (scheduler) to 480 USD,
> Bryntum wymaga licencji OEM dla SaaS.

## 10.2 Struktura

```
Grooming-Management-Frontend/
├── Components/
│   ├── AuthenticatedPage.cs          klasa bazowa stron chronionych
│   ├── App.razor                     style MudBlazor + Radzen
│   ├── Routes.razor                  ⚠️ TU są providery MudBlazora
│   ├── Layout/
│   │   ├── MainLayout.razor          MudLayout + AppBar + Drawer
│   │   ├── EmptyLayout.razor         dla logowania/rejestracji
│   │   └── NavMenu.razor             menu z ukrywaniem wg roli
│   ├── Pages/                        15 ekranów
│   ├── Dialogs/                      16 dialogów
│   └── Shared/
│       └── DurationPicker.razor      godziny + minuty → int
├── Services/
│   ├── TokenStore.cs                 tokeny, rola z JWT, ApplyTo
│   ├── ApiClient.cs                  Get/Post/Put/Delete + auto-refresh
│   └── EnumLabels.cs                 polskie nazwy enumów
└── appsettings.json                  ApiBaseUrl
```

## 10.3 Ekrany

| Trasa | Plik | Zawartość |
|---|---|---|
| `/` | `Home.razor` | pulpit: dzisiejsze wizyty, 4 kafelki (wizyt/przed nami/ukończone/przychód) |
| `/login` | `Login.razor` | logowanie, layout pusty |
| `/rejestracja` | `Register.razor` | rejestracja salonu + auto-login |
| `/kalendarz` | `Calendar.razor` | RadzenScheduler, widok dzienny i tygodniowy |
| `/wlasciciele` | `DogOwners.razor` | lista + dodawanie |
| `/wlasciciele/{id}` | `DogOwnerDetails.razor` | dane + psy + edycja |
| `/psy` | `Dogs.razor` | lista + dodawanie |
| `/psy/{id}` | `DogDetails.razor` | dane + historia wizyt + edycja |
| `/groomerzy` | `Groomers.razor` | lista, menu: karta/edycja/konto/aktywacja **[Owner]** |
| `/groomerzy/{id}` | `GroomerDetails.razor` | zakładki: grafik / blokady / rozliczenie **[Owner]** |
| `/cennik` | `PriceList.razor` | tabela z filtrami (rasa, usługa, status) **[Owner]** |
| `/raporty` | `Reports.razor` | rozliczenia + wykres dzienny **[Owner]** |
| `/czarna-lista` | `Blacklist.razor` | lista + dodawanie po kliencie lub psie |
| `/lista-oczekujacych` | `Waitlist.razor` | lista z priorytetem |
| `/ustawienia` | `SalonSettings.razor` | dane salonu **[Owner]** |

## 10.4 `TokenStore`

```csharp
public class TokenStore(ILocalStorageService localStorage)
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public string? Role { get; private set; }
    public event Action? OnChange;

    public bool IsLoggedIn => AccessToken != null;
    public bool IsOwner => Role == "Owner";

    public async Task SetTokensAsync(string accessToken, string refreshToken) { /* + ReadRoleFromToken() + NotifyChanged() */ }
    public async Task LoadFromStorageAsync() { /* + ReadRoleFromToken() + NotifyChanged() */ }
    public async Task ClearAsync() { /* + Role = null + NotifyChanged() */ }
    public void ApplyTo(HttpClient client) { /* nagłówek Bearer */ }

    private void ReadRoleFromToken() { /* dekoduje base64 payload JWT, szuka claimu kończącego się na "/role" */ }
}
```

> **`ReadRoleFromToken` MUSI być wołane** w `SetTokensAsync` i `LoadFromStorageAsync` —
> zapomnienie tego było realnym bugiem (rola zawsze `null`).

## 10.5 `ApiClient`

```csharp
public class ApiClient(IHttpClientFactory factory, TokenStore tokenStore)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }    // ⚠️ KONIECZNE
    };

    public async Task<T?> GetAsync<T>(string url);
    public async Task<HttpResponseMessage> PostAsync<T>(string url, T body);
    public async Task<HttpResponseMessage> PutAsync<T>(string url, T body);
    public async Task<HttpResponseMessage> DeleteAsync(string url);
    public static async Task<string> ReadErrorAsync(HttpResponseMessage response);  // parsuje ProblemDetails
}
```

**Auto-refresh:** przy 401 woła `POST /api/Auth/RefreshToken?refreshToken=...`, zapisuje
**obie** nowe wartości (backend rotuje refresh tokeny!), ponawia pierwotne żądanie.

## 10.6 `AuthenticatedPage`

```csharp
public abstract class AuthenticatedPage : ComponentBase
{
    [Inject] protected TokenStore TokenStore { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        if (!TokenStore.IsLoggedIn) await TokenStore.LoadFromStorageAsync();
        if (!TokenStore.IsLoggedIn) { Navigation.NavigateTo("/login"); return; }
        StateHasChanged();
    }
}
```

**Wzorzec każdej strony:**
```razor
@page "/adres"
@rendermode InteractiveServer
@inherits AuthenticatedPage
@inject ApiClient Api

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);      // ⚠️ NAJPIERW tokeny
        if (firstRender) { await LoadDataAsync(); StateHasChanged(); }
    }
}
```

---

# 11. PUŁAPKI FRONTENDU — LISTA WYUCZONA NA BŁĘDACH

## 11.1 🔴 HTTPS obowiązkowo

Backend ma `UseHttpsRedirection()`. Przy żądaniu na HTTP `HttpClient` dostaje przekierowanie
i **gubi nagłówek `Authorization`** — objawia się jako 401 mimo poprawnego tokenu.

**Rozwiązanie:** `ApiBaseUrl = "https://localhost:7250/"` i uruchamianie backendu
z profilem **https**, nie http.

## 11.2 🔴 `IHttpClientFactory` vs `Scoped TokenStore`

`DelegatingHandler` zarejestrowany przez `AddHttpMessageHandler` trafia do puli i dostaje
**inną instancję** `TokenStore` niż ekran. Widzi pusty token → 401.

**Rozwiązanie:** zamiast handlera — `TokenStore.ApplyTo(client)` wywoływane jawnie
w `ApiClient` przed każdym żądaniem.

## 11.3 🔴 Layout nie może być interaktywny

```
InvalidOperationException: Cannot pass the parameter 'Body' to component 'MainLayout'
with rendermode 'InteractiveServerRenderMode'
```

`MainLayout` dostaje `@Body` (`RenderFragment`), którego nie da się serializować.

**Konsekwencje:**
- `MainLayout` **bez** `@rendermode`
- Providery MudBlazora (`MudThemeProvider`, `MudDialogProvider` itd.) przeniesione
  do **`Routes.razor`**, który ma `@rendermode InteractiveServer`
- Wczytywanie tokenów przeniesione do `AuthenticatedPage`, nie do layoutu

## 11.4 🟡 Kolizje nazw MudBlazor ↔ Radzen

Obie biblioteki mają typy: `Variant`, `DialogOptions`, `DialogParameters`, `ChartSeries`,
`IDialogService`.

**Rozwiązanie:**
- W `_Imports.razor` **tylko** `@using Radzen.Blazor` — **NIE** `@using Radzen`
- W `Calendar.razor` (który potrzebuje `@using Radzen`) kwalifikować: `MudBlazor.Variant`,
  `MudBlazor.DialogOptions`
- `@inject MudBlazor.IDialogService DialogService` tam, gdzie oba są widoczne

## 11.5 🟡 `ShowMessageBox` nie do rozwiązania

Metoda rozszerzająca MudBlazora nie rozwiązywała się przy obecności Radzena.
**Rozwiązanie:** własny `MessageDialog.razor` otwierany przez `ShowAsync`.

## 11.6 🟡 `decimal` w query stringu

```csharp
// ❌ przy kulturze pl-PL wyśle "200,50" → backend odczyta 20050
$"api/Visit/{id}/final-price?finalPrice={finalPrice}"

// ✅
var priceParam = finalPrice.ToString(CultureInfo.InvariantCulture);
$"api/Visit/{id}/final-price?finalPrice={priceParam}"
```

To był realny bug: cena 200 zapisywała się jako 20000.

## 11.7 🟡 Radzen ignoruje kulturę

Kultura `pl-PL` ustawiona w `Program.cs` **nie wpływa** na format godzin w schedulerze.

```razor
<RadzenDayView TimeFormat="HH:mm" />
<RadzenWeekView HeaderFormat="ddd dd.MM" TimeFormat="HH:mm" />
```

## 11.8 🟡 `Template` + `ChildContent`

Gdy `RadzenScheduler` ma własny `<Template>`, widoki muszą trafić do jawnego `<ChildContent>`:

```razor
<RadzenScheduler ...>
    <Template Context="appointment"> ... </Template>
    <ChildContent>
        <RadzenDayView ... />
        <RadzenWeekView ... />
    </ChildContent>
</RadzenScheduler>
```

## 11.9 🟡 `JsonStringEnumConverter` po obu stronach

Backend serializuje enumy jako **nazwy** (`"Active"`). Frontend bez tego samego konwertera
oczekuje liczb i wywala się na `The JSON value could not be converted to ...Enum`.

## 11.10 🟡 `localStorage` tylko w `OnAfterRenderAsync`

Podczas pierwszego renderowania po stronie serwera przeglądarka nie jest podłączona —
JS interop rzuci wyjątkiem.

## 11.11 🟡 Kolekcje w DTO wymagają `= new()`

`AvailableSlots` bez inicjalizacji dawało `NullReferenceException` — i to **tylko wtedy,
gdy logika coś znalazła**, bo przy pustym wyniku `Add` się nie wykonywało.

---

# 12. PORTAL KLIENTA — ODŁOŻONY

**Decyzja:** świadomie odłożony po analizie konkurencji.

**Co zostało w kodzie:**
- `DogOwner.UserId` + migracja — zostaje, nic nie kosztuje
- `RoleEnum.Client` — zostaje na końcu enuma
- `BookVisitByClientAsync` — **zakomentowane** w `VisitService` z notatką:
  ```
  // Zaczątek portalu klienta — nieaktywne.
  // Wymaga rejestracji kont klienckich (RegisterClientAccountAsync), której nie ma.
  // Logika napisana, nieprzetestowana. Sierpień 2026.
  ```
- Endpoint `POST /api/Visit/book` — usunięty/zakomentowany

**Wnioski z tamtej pracy, które zostały:**
- Auto-przypisanie groomera to **odpowiedzialność frontendu** — backend zawsze dostaje
  konkretne `GroomerId` i tylko weryfikuje
- Wzorzec bezpieczeństwa: cudzy zasób → **404, nie 403** (żeby nie potwierdzać istnienia)

---

# 13. KONTEKST BIZNESOWY (ZAKTUALIZOWANY)

## 13.1 Konkurencja — zbadana

| Produkt | Cena | Uwagi |
|---|---|---|
| **GroomBook** | od 19 zł (bez SMS), 65 zł z SMS | aplikacja mobilna, zdjęcia przed/po, hotel, program lojalnościowy |
| **Groominarz** | od 59 zł | prosty widok mobilny (pasek dni + lista) |
| **GroomerSystem** | 89 zł | jednoosobowy |

> **Otwarte pytanie:** czym się odróżnić? Bez odpowiedzi wejście door-to-door do salonu
> płacącego 19 zł jest trudne.

## 13.2 Pomysł na przewagę — integracja ze sklepem

Zakładka „uzupełnij zapasy": groomer wybiera produkty, których używa, klika zamów,
dane adresowe idą z `Salon`.

**Model przychodu:** prowizja od zamówień + **lepsza cena dla klientów** (ważniejsze).

**Co sprawdzić przed budowaniem** — mail do groomershop.pl został przygotowany:
1. Czy jest API do składania zamówień albo feed produktów?
2. Czy jest program partnerski?
3. Czy przy wolumenie możliwa lepsza cena niż standardowe 30% dla groomerów?

Bez API możliwe tylko przekierowanie z koszykiem — nie „dwa kliknięcia".

## 13.3 Wnioski z konsultacji z groomerką (mama)

- ✅ Czas wizyty **w godzinach i minutach**, nie w samych minutach — zrobione (`DurationPicker`)
- ✅ Rasa widoczna przy wyborze psa — zrobione
- ❓ **Rasy czy rozmiary w cenniku** — do rozstrzygnięcia. 100+ ras to za dużo do wpisania,
  ale rozmiar nie oddaje różnicy między yorkiem a sznaucerem
- ❓ **Jak rozliczać pomoc** — u mamy 25% od psa, ale w innych salonach może być godzinowo

---

# 14. STAN PROJEKTU

## 14.1 ✅ Ukończone

- **Faza 1–3** — bez zmian od poprzedniej dokumentacji
- **Faza 4** — dostępność, kolizje, przetestowane
- **Faza 5** — monetyzacja: statusy, płatności, middleware, scheduler
- **Rozliczenia** — godzinowe/procentowe + pomoc
- **Frontend** — 15 ekranów, 16 dialogów, pełny CRUD

## 14.2 🔴 Blokuje sprzedaż

**Płatności**
- Konto Stripe + `Stripe.net`
- Webhook z weryfikacją podpisu
- **Usunąć tymczasowy `POST /api/Subscription/payment`**
- Ekran subskrypcji dla właściciela
- SMS przy wejściu w karencję

**SMS**
- Prawdziwy provider zamiast `MockSmsService`

**Frontend**
- Wymuszenie zmiany hasła (backend gotowy, brak ekranu) ← **w trakcie**

**Wdrożenie**
- Hosting, PostgreSQL, sekrety, HTTPS, backup, domena

**Formalne**
- Regulamin, polityka prywatności, umowa powierzenia (RODO), działalność, faktury

## 14.3 🟡 Przed pierwszym klientem

- [ ] Walidacja kolizji w `EditVisitAsync`
- [ ] Walidacja formatowa: email, telefon, puste stringi
- [ ] Polskie komunikaty błędów z backendu
- [ ] Strefy czasowe — `AvailabilityService` liczy okno od `UtcNow`, a grafiki to czas lokalny
- [ ] `ILogger` zamiast `Console.WriteLine`
- [ ] Ujednolicić rejestracje DI
- [ ] `DbSeeder` — hasło `TEMP_NIE_ZAHASHOWANE` uniemożliwia logowanie
- [ ] `DateTime.Now` → `UtcNow` w `WaitlistService`
- [ ] Literówki: `doeasnt exists`, `remainder`, `HasherSH256`
- [ ] `XAxisLabels` na `MudChart` — atrybut nieznany w tej wersji
- [ ] Ostrzeżenia `CS0108` — strony wstrzykują `NavigationManager` mimo dziedziczenia
- [ ] Widok mobilny
- [ ] Testy Fazy 2 i 3 end-to-end

## 14.4 🟢 Może poczekać

- Raporty: popularne usługi, rasy, obciążenie groomerów
- Zdjęcia przed/po (ma GroomBook)
- Portal klienta
- Integracja ze sklepem
- Sprzątanie wygasłych `RefreshToken`
- **Testy automatyczne** — nadal największy dług
- `AttemptCount` w `Notification` — logika ponawiania

---

# 15. PROCEDURA URUCHOMIENIA

```bash
# backend — PROFIL HTTPS, nie http!
cd Grooming-Management-Backend
dotnet ef database update
dotnet run --launch-profile https        # https://localhost:7250

# frontend
cd Grooming-Management-Frontend
dotnet run                                # https://localhost:7124
```

**Na nowym komputerze dodatkowo:**
```bash
dotnet dev-certs https --trust
```
Plus odtworzenie `appsettings.Development.json` backendu (jest w `.gitignore`):
`ConnectionStrings:DefaultConnection` i `JwtSettings:SecretKey` (min. 32 znaki).

**`appsettings.json` frontendu:**
```json
{ "ApiBaseUrl": "https://localhost:7250/" }
```

---

# 16. ZASADY WSPÓŁPRACY (bez zmian)

- Konrad pisze kod sam; asystent prowadzi przez opis, pytania i recenzję
- Gotowy kod tylko gdy Konrad utknie albo w nowej technologii (Blazor)
- **Wszystko po polsku** — rozmowy, etykiety UI, komunikaty do klienta
- Nazwy klas, metod i komunikaty wyjątków po angielsku
- Commit po każdym skończonym kroku
- Testowanie przez Swagger UI (Rider), błędy wychodzą przy realnym użyciu, nie przy czytaniu
