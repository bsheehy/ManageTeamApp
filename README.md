# TeamManage

A .NET MAUI mobile app (Android / iOS) for managing sports teams, with an ASP.NET Core
Web API backend and SQL Server (EF Core) persistence.

## Solution structure

Clean-architecture layering, each concern in its own project:

| Project | Responsibility |
|---|---|
| `TeamManage.Domain` | Entities and enums. No dependencies. |
| `TeamManage.Application` | DTOs, service interfaces/implementations, business rules. Depends on Domain only. |
| `TeamManage.Infrastructure` | EF Core `DbContext`, repositories, password hashing, JWT issuing. Depends on Application + Domain. |
| `TeamManage.Api` | ASP.NET Core Web API (controllers, auth, DI composition root for the backend). |
| `TeamManage.App` | .NET MAUI mobile client (XAML Views + MVVM ViewModels), calls the API over HTTP. |

## Functionality

- **Login / Register** with two roles: **Manager** and **Player**.
- **Manager**:
  - Create named panels/teams per sport (e.g. GAA 15, Soccer 11, Basketball 5), with a
    configurable squad size and formation (positions).
  - Add/remove players on a team's panel (creates a player account automatically if the
    email doesn't already exist, returning a one-time temporary password).
  - Schedule matches/fixtures for a team.
  - Allocate players to starting positions and substitutes for a match.
- **Player**:
  - View teams they belong to and upcoming match details.
  - Confirm or decline attendance for a match.
- **Offline support**: the mobile app keeps a short-lived JSON cache of teams/matches and
  automatically falls back to it (with an "Offline" banner) when there's no connectivity or
  the API is unreachable. Sessions use short-lived JWT access tokens plus a longer-lived
  refresh token so users aren't forced to log in again every time the access token expires.

## Prerequisites

- .NET SDK 10 with the `android`, `ios`, `maccatalyst` and `maui-windows` workloads
  (`dotnet workload install android ios maccatalyst maui-windows`).
- SQL Server (LocalDB, Express, or full) for the API's database.
- For Android emulator testing, the emulator's loopback alias `10.0.2.2` is already
  configured in [src/TeamManage.App/Configuration/ApiConfig.cs](src/TeamManage.App/Configuration/ApiConfig.cs).

## Database setup

The API uses SQL Server via EF Core. Pick whichever SQL Server option is easiest for you:

- **SQL Server Express / Developer / full edition** — the default connection string
  (`Server=.\SQLEXPRESS;Database=TeamManage;Trusted_Connection=True;TrustServerCertificate=True;`)
  targets a local `SQLEXPRESS` named instance using Windows integrated auth. No changes needed
  if you already have SQL Server Express installed with the default instance name.
- **LocalDB** (bundled with Visual Studio): change the connection string to
  `Server=(localdb)\\MSSQLLocalDB;Database=TeamManage;Trusted_Connection=True;TrustServerCertificate=True;`.
- **Docker**: run a disposable SQL Server container and use SQL auth, e.g.:
  ```powershell
  docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YourStrong!Passw0rd>" -p 1433:1433 --name teammanage-sql -d mcr.microsoft.com/mssql/server:2022-latest
  ```
  then use `Server=localhost,1433;Database=TeamManage;User Id=sa;Password=<YourStrong!Passw0rd>;TrustServerCertificate=True;`.

Update the connection string in
[src/TeamManage.Api/appsettings.json](src/TeamManage.Api/appsettings.json)
(`ConnectionStrings:DefaultConnection`) — or override it locally without editing the committed
file via user-secrets: `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your connection string>"`
(run from `src/TeamManage.Api`).

Once the connection string points at a reachable SQL Server instance, create/update the database
schema by applying the EF Core migrations (this creates the `TeamManage` database if it doesn't
exist yet, and all tables: Users, Teams, TeamPlayers, Matches, MatchSelections, RefreshTokens, etc.):

```powershell
cd src/TeamManage.Api
dotnet tool install --global dotnet-ef   # first time only, if you don't already have it
dotnet ef database update
```

To inspect the schema or reset it during development, you can drop and recreate the database:

```powershell
dotnet ef database drop --force   # deletes the database entirely
dotnet ef database update         # recreates it from migrations
```

There's no seed data — register your first Manager account via the app's Register screen (or
`POST /api/auth/register`) once the API is running, then use it to create teams and add players.

## Running the API

```powershell
cd src/TeamManage.Api
dotnet user-secrets set "Jwt:Secret" "<a long random base64 string>"   # first time only
dotnet run
```

The connection string is in [src/TeamManage.Api/appsettings.json](src/TeamManage.Api/appsettings.json)
(`ConnectionStrings:DefaultConnection`) — update it to point at your SQL Server instance.
The JWT signing secret is intentionally **not** stored in appsettings.json; it's kept in
user-secrets (local dev) or should come from a secure secret store (e.g. Azure Key Vault,
environment variables) in any real deployment.

## Running the MAUI app

Update [src/TeamManage.App/Configuration/ApiConfig.cs](src/TeamManage.App/Configuration/ApiConfig.cs)
with your API's actual address (especially for a physical device or production), then run/deploy
the `TeamManage.App` project to your target platform (Android emulator/device, iOS simulator/device,
or Windows) from Visual Studio or `dotnet build -t:Run -f net10.0-android` etc.

## Adding an EF Core migration

```powershell
dotnet ef migrations add <Name> --project src/TeamManage.Infrastructure --startup-project src/TeamManage.Api --output-dir Persistence/Migrations
```

## Authentication & session handling

- Passwords are hashed with ASP.NET Core Identity's `PasswordHasher<TUser>` (PBKDF2-HMAC-SHA256,
  100,000 iterations) — see
  [src/TeamManage.Infrastructure/Security/PasswordHasher.cs](src/TeamManage.Infrastructure/Security/PasswordHasher.cs).
  This reuses Microsoft's maintained, audited implementation without pulling in full
  ASP.NET Core Identity (no `UserManager`/user store/schema changes).
- Login/Register return a short-lived **JWT access token** plus a long-lived, opaque
  **refresh token** (default 30 days, see `Jwt:RefreshTokenExpiryDays` in
  [src/TeamManage.Api/appsettings.json](src/TeamManage.Api/appsettings.json)). Refresh tokens are
  stored server-side as a SHA-256 hash only (never in plaintext) and rotate on every use — see
  `POST /api/auth/refresh` in
  [src/TeamManage.Api/Controllers/AuthController.cs](src/TeamManage.Api/Controllers/AuthController.cs).
- The MAUI app persists both tokens in platform secure storage via
  [src/TeamManage.App/Services/SessionService.cs](src/TeamManage.App/Services/SessionService.cs), and
  [src/TeamManage.App/Services/TokenRefreshHandler.cs](src/TeamManage.App/Services/TokenRefreshHandler.cs)
  transparently refreshes the access token and retries the request on a 401, so mobile users stay
  signed in across short network interruptions without re-entering their password.
- Auth uses JWT bearer tokens; the signing secret must be supplied via configuration/secrets,
  never committed to source control.
- A workspace-local [NuGet.Config](NuGet.Config) restricts package restore to nuget.org only.
