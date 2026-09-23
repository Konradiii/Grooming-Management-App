# Groomsli

Wielodostępowy SaaS dla polskich salonów groomerskich. Działa na produkcji (Azure),
a co najmniej jeden prawdziwy salon pracuje na nim codziennie — dane produkcyjne są prawdziwe.

## Stack

- .NET 10. Solution: `Grooming-Management-App.sln`
- Backend: `Grooming-Management-Backend/` — ASP.NET Core Web API, RootNamespace `Grooming_Management_App`
- Frontend: `Grooming-Management-Frontend/` — Blazor Server, RootNamespace `Grooming_Management_Frontend`
- Frontend ma referencję do projektu backendu: DTO i enumy są wspólne. Zmiana DTO w backendzie od razu wpływa na front.
- EF Core + Npgsql (PostgreSQL), MudBlazor 9, Radzen.Blazor (tylko scheduler), Stripe.net, SMSAPI
- Ostrzeżenie `CS0436` o konflikcie typu `Program` jest nieszkodliwe

## Jak pracujemy

- Rozmawiaj ze mną po polsku.
- Etykiety UI i komunikaty dla użytkownika: po polsku. Nazwy klas, metod, kody błędów, commity: po angielsku.
- Commity w trybie rozkazującym, krótkie (`Add visit reschedule tracking`), z roota repozytorium.
- Przy większym zadaniu najpierw zaproponuj plan i poczekaj na akceptację.
- Małe, skupione zmiany. Czytam każdy diff — po zmianie napisz krótko, co zmieniłeś i gdzie.
- `main` = to, co stoi na produkcji. Pracujemy na gałęziach `feature/...`.
- Przed ogłoszeniem, że skończone: `dotnet build` w katalogu solution musi przejść bez błędów.
- Po zmianie we froncie podaj ścieżkę testową: adres strony, co kliknąć, czego się spodziewać, i jakie przypadki brzegowe sprawdzić.
- Pracujemy wyłącznie na lokalnej bazie (`Host=localhost`). Jeśli connection string wskazuje gdziekolwiek indziej, przerwij i powiedz mi.
- Uruchamianie aplikacji zostaw mnie — ja odpalam backend i `dotnet watch` na froncie.

## ZAKAZY — bez wyjątków

- NIE uruchamiaj `dotnet ef database update` bez mojej wyraźnej zgody.
- NIGDY nie łącz się z produkcyjną bazą (`grooming-db.postgres.database.azure.com`) — żadnych migracji, zapytań ani `--connection` na produkcję.
- NIE edytuj `appsettings.Development.json`, zmiennych środowiskowych ani sekretów. Nie wypisuj ich zawartości.
- NIE publikuj niczego na Azure i nie rób `git push` bez pytania.
- NIE usuwaj ani nie edytuj istniejących migracji.
- NIE zmieniaj numeracji istniejących enumów. Nowe wartości dopisuj wyłącznie NA KOŃCU (w bazie to kolumny `int`).
- NIE commituj plików `*.sql` z danymi (zrzuty bazy).

## Backend — konwencje

**Serwisy**
- Folder na serwis: `Services/<Nazwa>Serv/` z `<Nazwa>Service.cs` i interfejsami obok.
- Interfejsy dzielone na `I<Nazwa>ReaderService` / `I<Nazwa>WriterService`.
- Rejestracja w `Program.cs`: `builder.Services.AddScopedWithInterfaces<XService>(typeof(IXReaderService), typeof(IXWriterService));`
- Klasa MUSI implementować każdy wymieniony interfejs — inaczej `InvalidCastException` przy tworzeniu kontrolera i pada cały kontroler.
- Metody asynchroniczne z sufiksem `Async` i `CancellationToken ct`. Sufiks tylko dla metod faktycznie async.

**Wielodostęp (najważniejsze)**
- Globalne filtry zapytań po `SalonId` z claima JWT (`CurrentUserService`).
- `salonId` ZAWSZE z claims, nigdy z DTO ani z adresu.
- Endpointy anonimowe (login, refresh, webhook Stripe): `.IgnoreQueryFilters()` przed `.Include()` encji z filtrem — inaczej `NullReferenceException`.
- Cudzy zasób przy odczycie → 404, nie 403 (nie potwierdzamy istnienia). Brak uprawnienia do utworzenia → 403.

**Błędy**
- Rzucaj `NotFoundException` / `ConflictException` / `ForbiddenException` / `UnauthorizedException` z kodem z `Exceptions/ErrorCodes.cs`.
- Nowy kod błędu = nowa stała w `ErrorCodes.cs` + polskie tłumaczenie w `Grooming-Management-Frontend/Services/ErrorMessages.cs`, w tym samym commicie.
- Kod musi pasować do WARUNKU, nie tylko do typu wyjątku (duplikat telefonu → `PhoneTaken`, nie `BreedNotFound`).

**Walidacja i dane**
- Pomocniki w `Exceptions/Validate.cs` (`NotEmpty`, `Email`, `PolishPhone`, `PolishPostalCode`).
- `.Trim()` raz i ta sama wartość do sprawdzenia duplikatu i do zapisu.
- E-mail normalizowany `Trim().ToLowerInvariant()` przy rejestracji, logowaniu i sprawdzaniu duplikatów.
- Dodajesz pole do DTO → sprawdź, czy projekcja je mapuje. Brak mapowania daje wartość domyślną i przy zapisie potrafi skasować kolumnę. Ten błąd powtórzył się trzy razy.
- `HasDefaultValue` w konfiguracji nie ustawia wartości dla nowych obiektów w kodzie — ustawiaj jawnie przy tworzeniu. Po zmianie domyślnej wygeneruj migrację.
- Kolekcje w DTO zawsze inicjalizowane `= new()`.
- Enumy serializowane jako nazwy (`JsonStringEnumConverter`) po obu stronach.

**Czas**
- Baza i backend trzymają wyłącznie UTC (`timestamp with time zone`).
- Konwersja na czas polski tylko we froncie (`Services/Dates.cs`: `ToUtc`, `ToLocal`, `Format`). Wyjątek: treść SMS-a.
- Daty z query stringu w kontrolerach: `.AsUtc()` (`Extensions/DateTimeExtensions.cs`).
- `GroomerSchedule` i `GroomerTimeOff` to czas lokalny (`DateOnly` + `TimeOnly`). Przed porównaniem z `Visit.Date` konwertuj przez `TimeZoneInfo` `Europe/Warsaw` — Npgsql odrzuca porównanie `timestamptz` z `timestamp`.
- Nigdy `ToLocalTime()` — serwer na Azure działa w UTC.
- `DateTime.UtcNow`, nie `DateTime.Now`.

**Wizyty**
- Wizyta ma dokładnie jedno z: `ServiceBreedId` (pozycja cennika) albo `ServiceId` (usługa + cena ręcznie).
- Cena, czas trwania i rozliczenie groomera są SNAPSHOTEM z chwili utworzenia — nie aktualizuj ich ze źródła.
- Nakładanie wizyt to ostrzeżenie (`IgnoreOverlap`), blokady czasu (`GroomerTimeOff`) są bezwzględne.
- W metodach edycji: najpierw walidacje, potem mutacje. Porównanie starej wartości z nową przed przypisaniem.

**Kontrolery**
- Zwracaj jawnie `Ok(result)` — goły `return result` przy `null` daje 204 i front wywala się na parsowaniu.

**Logowanie i usługi w tle**
- `ILogger<T>` ze strukturalnymi placeholderami, bez `$`: `logger.LogError(ex, "Failed for visit {VisitId}", id);` — wyjątek jako pierwszy parametr.
- `BackgroundService`: `catch (OperationCanceledException)` PRZED `catch (Exception)`, `Task.Delay` poza głównym `try`.

**Stripe**
- Webhooki idempotentne — unikalny indeks na `Payment.ProviderId`, `ConflictException` łapany i zwracane 200.
- Ignoruj faktury zerowe (trial) i faktury bez subskrypcji (doładowania SMS).
- Tryby test i live mają osobne `price_`. Lokalnie tylko klucze testowe.

**Konfiguracja**
- Każdy nowy klucz konfiguracji musi mieć odpowiednik w zmiennych Azure — `GetValue<int>` przy braku klucza zwraca 0 bez błędu. Przy nowym kluczu dodaj walidację przy starcie i powiedz mi, że trzeba go dodać w Azure.

## Frontend — konwencje

- Strona: `@rendermode InteractiveServer` + `@inherits AuthenticatedPage` (albo `OwnerOnlyPage` dla stron właściciela).
- W `OnAfterRenderAsync` najpierw `await base.OnAfterRenderAsync(firstRender);`, potem dane przy `firstRender`.
- `localStorage` wyłącznie w `OnAfterRenderAsync`.
- `MainLayout` NIE jest interaktywny. Providery MudBlazora są w `Routes.razor`. `AddMudServices` tylko raz, jeden `MudPopoverProvider`.
- W `_Imports.razor` tylko `@using Radzen.Blazor`, nigdy `@using Radzen`. Przy kolizji nazw kwalifikuj `MudBlazor.Variant`, `MudBlazor.DialogOptions` itd.
- Komunikacja z API przez `ApiClient` (`GetAsync`, `PostAsync`, `PutAsync`, `DeleteAsync`, `ReadErrorAsync`, `ReadErrorWithCodeAsync`). Rzuca `ApiException`, a 404 zwraca `default`.
- Logikę zależną od błędu opieraj na KODZIE (`ReadErrorWithCodeAsync`), nie na przetłumaczonym tekście.
- `decimal` w query stringu: `ToString(CultureInfo.InvariantCulture)`.
- Komunikaty dla użytkownika przez `ISnackbar`.
- Komponenty wspólne w `Components/Shared/` (`DurationPicker`, `AgePicker`, `BreedPicker`) — używaj zamiast duplikować logikę.
- Radzen ignoruje kulturę — formaty godzin ustawiaj jawnie (`TimeFormat="HH:mm"`).

## Uruchamianie lokalne

- Backend: z `Grooming-Management-Backend/`: `dotnet run --launch-profile https` → `https://localhost:7250`. Musi być HTTPS — przy HTTP przekierowanie gubi nagłówek `Authorization`.
- Frontend: z `Grooming-Management-Frontend/`: `dotnet run` → `https://localhost:7124`.
- Nowa migracja: `dotnet ef migrations add <NazwaPoAngielsku>` z katalogu backendu, przy ZATRZYMANYM backendzie. Pokaż mi wygenerowany plik, zanim cokolwiek dalej.
- Konta testowe (tylko lokalnie): `owner@test.com`, `anna@test.com`, hasło `Test1234!`.
- Testów automatycznych jeszcze nie ma. Pierwsze planowane: izolacja danych między salonami.

## Dokumentacja projektu

Szczegóły decyzji, pułapek i historii zmian są w `docs/`. Czytaj właściwą sekcję, zanim ruszysz Stripe, SMS, strefy czasowe, uwierzytelnianie albo subskrypcje:

- `docs/dokumentacja-techniczna-v4.md` — stan aktualny: plany, SMSAPI, Stripe live, domeny, lista powtarzających się błędów
- `docs/dokumentacja-techniczna-v3.md` — uprawnienia, strefy czasowe, kody błędów, PostgreSQL, Azure
- `docs/dokumentacja-techniczna-v2.md` — dostępność, monetyzacja, frontend Blazor, pułapki MudBlazor/Radzen

## Aktualny kierunek

Budujemy platformę dla klientów salonów: konta klientów w Groomsli, publiczna strona salonu, prośby o termin (klient prosi, groomerka zatwierdza — klient NIGDY nie tworzy wizyty sam), panel klienta, katalog salonów. Pierwszy etap to refaktor architektury pod dwa typy użytkowników: pracownik salonu (token z `salonId`) i klient (token bez `salonId`).
