# Vehicle Import System

A web-oriented information system for calculating and analyzing customs payments when importing vehicles into Ukraine. The application estimates import duty, excise tax, VAT, and pension fund contributions, compares the result with the average market price from Auto.ria, and stores calculation history per anonymous device without user registration.

## Tech Stack

### Backend (.NET 8)

| Technology | Version | Project / Usage |
|------------|---------|-----------------|
| C# / .NET | 8.0 | All backend projects (`net8.0`) |
| ASP.NET Core Minimal API | 8.0 | `VehicleImportSystem.API` |
| Entity Framework Core | 8.0.22 | `Application`, `Infrastructure`, `API` (Design) |
| EF Core SQL Server Provider | 8.0.22 | `Infrastructure` |
| FluentValidation | 12.1.1 | `Application` |
| FluentValidation.AspNetCore | 11.3.1 | `API` |
| Microsoft.Extensions.Http.Polly | 8.0.22 (API) / 8.0.1 (Infrastructure) | HTTP resilience |
| Polly.Extensions.Http | 3.0.0 | Retry, circuit breaker, timeout |
| Swashbuckle.AspNetCore (Swagger) | 6.6.2 | API documentation |
| Microsoft.AspNetCore.OpenApi | 8.0.19 | OpenAPI support |

### Frontend (Angular)

| Technology | Version | Usage |
|------------|---------|--------|
| Angular | ^20.2.0 | SPA (`VehicleImportSystem.Web`) |
| Angular CLI / Build | ^20.2.1 | Tooling |
| TypeScript | ~5.9.2 | Language |
| Angular Material & CDK | ^20.2.14 | UI components |
| RxJS | ~7.8.0 | Async streams |
| Chart.js | ^4.5.1 | Tax breakdown charts |
| ng2-charts | ^8.0.0 | Angular Chart.js wrapper |
| SCSS | — | Component and global styles |
| uuid | ^13.0.0 | Device ID generation (via `crypto.randomUUID`) |

### Database & External APIs

- **Microsoft SQL Server** — primary data store (`AppDbContext`)
- **NBU (National Bank of Ukraine)** — official EUR/USD exchange rates (`NbuCurrencyService`)
- **Auto.ria Developers API** — vehicle models and average market prices (`AutoRiaMarketPriceService`)

## Architecture

The solution follows **Clean Architecture** with four layers under `src/`:

```
VehicleImportSystem/
├── VehicleImportSystem.Domain/          # Entities, enums, settings (no dependencies)
├── VehicleImportSystem.Application/   # Interfaces, DTOs, validators, mappings
├── VehicleImportSystem.Infrastructure/ # EF Core, external services, Polly, DI registration
└── VehicleImportSystem.API/             # HTTP endpoints, middleware, composition root
```

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Core business concepts: `CarBrand`, `CarModel`, `CurrencyRate`, `CustomsCalculation`, `FuelType`, `CustomsSettings`. No infrastructure references. |
| **Application** | Contracts (`ICustomsCalculatorService`, `ICurrencyService`, `IMarketPriceService`, `IHistoryService`, `IAppDbContext`), DTOs, FluentValidation rules, mapping extensions. |
| **Infrastructure** | `AppDbContext`, EF configurations, migrations, `CustomsCalculatorService`, NBU/Auto.ria HTTP clients, `PollyPolicies`, `DependencyInjection.AddInfrastructure()`. |
| **API** | Minimal API endpoint groups (`CalculatorEndpoints`, `HistoryEndpoints`, etc.), `GlobalExceptionHandlerMiddleware`, CORS, Swagger, `Program.cs` startup. |

Dependency rule: **API → Application + Infrastructure → Domain**. Application defines abstractions; Infrastructure implements them and is wired in `Program.cs` via `AddInfrastructure()`.

## Features

- **Customs calculation** — Computes import duty (10% for non-electric), excise (by fuel type, engine volume, and vehicle age), VAT (20% on price + duty + excise), and pension fund fee (tiered by UAH value). Electric vehicles are exempt from duty and pension fund; excise uses battery capacity (kWh).
- **Configurable tax rates** — Legislative thresholds and coefficients loaded from `CustomsSettings` in `appsettings.json` via the Options pattern.
- **Auto.ria integration** — Fetches average market price (`InterQuartileMean`), brand models, with in-memory caching and 429 rate-limit handling.
- **NBU currency rates** — EUR/USD rates with three-tier cache: `IMemoryCache` → SQL `CurrencyRates` → live NBU API, with fallback to the last stored rate.
- **Currency warmup** — `CurrencyRateWarmupService` preloads rates on startup and every 24 hours.
- **Calculation history** — Persists results in `CustomsCalculation`, scoped by anonymous `X-Device-Id` header (no login).
- **Brand catalog** — Seeded from `brands.json` (Auto.ria IDs); models loaded on demand from API and optionally persisted.
- **Resilience** — Polly retry (exponential backoff), circuit breaker, and timeout on `NbuApi` and `AutoRiaApi` HTTP clients.
- **API documentation** — Swagger UI in Development (`/swagger`).
- **Frontend** — Calculator form with autocomplete, market price fetch, pie chart of tax breakdown; history table with delete/clear actions; light/dark theme.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (LTS recommended, for Angular CLI)
- **SQL Server** (LocalDB, Express, or full instance)
- **Auto.ria API key** — register at [developers.ria.com](https://developers.ria.com/)

### 1. Clone and open the solution

```bash
git clone <repository-url>
cd VehicleImportSystem
```

Open `VehicleImportSystem.sln` in Visual Studio / Rider, or work from the command line.

### 2. Configure the backend

Edit `src/VehicleImportSystem.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=VehicleImportDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Set your Auto.ria API key in `src/VehicleImportSystem.API/appsettings.json` (or User Secrets / environment variables):

```json
"AutoRia": {
  "ApiKey": "YOUR_AUTORIA_API_KEY"
}
```

> **Note:** On startup, `DbInitializer.InitializeAsync()` applies EF Core migrations automatically and seeds car brands from `brands.json`. Manual migration is optional.

### 3. Run the API

```bash
cd src/VehicleImportSystem.API
dotnet restore
dotnet run
```

Default URLs (from `launchSettings.json`):

- HTTPS: `https://localhost:7291`
- HTTP: `http://localhost:5197`
- Swagger: `https://localhost:7291/swagger` (Development only)

### 4. Run the Angular frontend

In a second terminal:

```bash
cd src/VehicleImportSystem.Web
npm install
npm start
```

This runs `ng serve` at `http://localhost:4200`. The API base URL is configured in `src/environments/environment.ts`:

```typescript
apiUrl: 'https://localhost:7291/api'
```

Ensure CORS in the API allows `http://localhost:4200` (configured in `ServiceCollectionExtensions.AddCorsForAngularApp()`).

### 5. (Optional) Manual database migrations

If you prefer to apply migrations before running the API:

```bash
cd src/VehicleImportSystem.API
dotnet ef database update --project ../VehicleImportSystem.Infrastructure
```

Existing migrations:

- `20251216182523_InitialCreate`
- `20260103154023_RefactoringMappings`
- `20260117134832_RenamedTable`

### Project structure

```
VehicleImportSystem/
├── VehicleImportSystem.sln
├── README.md
├── Thesis_Materials.md
└── src/
    ├── VehicleImportSystem.Domain/
    ├── VehicleImportSystem.Application/
    ├── VehicleImportSystem.Infrastructure/
    ├── VehicleImportSystem.API/
    └── VehicleImportSystem.Web/
```

## API overview

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/calculator/calculate` | Calculate customs (requires `X-Device-Id`) |
| GET | `/api/history/` | User calculation history |
| DELETE | `/api/history/{id}` | Delete one record |
| DELETE | `/api/history/clear` | Clear user history |
| GET | `/api/brands` | List car brands |
| GET | `/api/market/brands/{id}/models` | Models for a brand |
| GET | `/api/market/average-price` | Average market price from Auto.ria |
| GET | `/api/currency/rates` | Current EUR/USD rates |

## License

Academic / diploma project — adjust licensing as required by your institution.
