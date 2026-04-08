# NoviCode

ASP.NET Core Web API project for managing wallets and currency conversion using ECB exchange rates.

---

## Solution structure

- **`NoviCode.Api/`**: Web API (controllers, exception handling, rate limiting, Quartz job scheduler)
- **`NoviCode.Application/`**: application services (wallet service, DTOs, interfaces, application exceptions)
- **`NoviCode.Domain/`**: domain entities and domain exceptions (e.g., `Wallet`)
- **`NoviCode.Infrastructure/`**: EF Core `DbContext`, repositories, SQL bulk merge for rates, Redis cache integration
- **`NoviCode.EcbGateway/`**: ECB XML gateway + parser
- **`Tests/`**: unit/integration tests 

---

## Tech stack

- **.NET 8**
- **ASP.NET Core** (controllers + exception handler + ProblemDetails)
- **Entity Framework Core** (SQL Server)
- **Quartz.NET** (scheduled background job)
- **StackExchange.Redis** (optional caching)

---

## Design decisions

- **Layered architecture (API / Application / Domain / Infrastructure)** → separation of concerns and testability.
- **ECB rates are EUR-based** → all currency conversions normalize through EUR to support arbitrary currency pairs.
- **Rates are stored per date** → enables historical storage and future extensibility for “as-of” queries.
- **Bulk upsert via SQL MERGE** → efficient insert/update of many rates.
- **Redis cache for latest rates** → reduces database load for read-heavy operations.
- **Strategy pattern for wallet operations** → clean separation of balance adjustment rules.


---

## Currency conversion

ECB provides exchange rates relative to EUR.

All conversions are performed in two steps:
1. Convert source currency → EUR
2. Convert EUR → target currency

---

## Prerequisites

- **.NET SDK 8.0**
- **SQL Server** (LocalDB or SQL Express or any reachable SQL Server instance)
- *(Optional but recommended)* **Redis** (local `localhost:6379`)

---

## Redis (optional)

By default the API is configured to use Redis at `localhost:6379`.

### Run Redis with Docker

If you have Docker Desktop installed, you can start a local Redis container with:

```bash
docker run -d --name novicode-redis -p 6379:6379 redis:7
```

To stop/remove it later:

```bash
docker stop novicode-redis
docker rm novicode-redis
```

If you prefer not to use Redis, the application will continue working using the database fallback paths (you may see warning logs when Redis is unavailable).

---

## Configuration

Main config is in `NoviCode.Api/appsettings.json`.

### Connection strings

- **`ConnectionStrings:ExchangeRateDb`**: SQL Server connection string
- **`ConnectionStrings:Redis`**: Redis connection string (optional; default is `localhost:6379` if missing)

### ECB Gateway options

- **`EcbGateway:DailyRatesUrl`**: URL for the ECB daily XML
- **`EcbGateway:TimeoutSeconds`**: HTTP timeout
- **`EcbGateway:RetryCount`**: retry attempts for transient failures

---

## Database & migrations

The database schema is managed by EF Core migrations in `NoviCode.Infrastructure/Migrations/`.

From the repo root:

```bash
dotnet ef database update --project NoviCode.Infrastructure --startup-project NoviCode.Api
```

Notes:
- You need the EF CLI available (`dotnet ef`). If it’s missing, install once:

```bash
dotnet tool install --global dotnet-ef
```
---

## Running the API

From the repo root:

```
dotnet run --project NoviCode.Api
```

### Swagger

Swagger is enabled in Development environment.

---

## Background job (Quartz): exchange rates sync

The API config registers a Quartz job:

- **Job**: `ExchangeRatesSyncJob`
- **Schedule**: every **1 minute**
- **Behavior**:
  - Fetches latest ECB rates via `NoviCode.EcbGateway`
  - Converts the response to DB rows
  - Merges rows into SQL Server (bulk MERGE)
  - Updates Redis snapshot cache (if available)

Redis:
- If Redis is down, the app will log warnings and continue using DB fallback paths.

---

## Rate limiting

The API uses IP-based sliding window rate limiting:

- **Create**: 10 requests / minute
- **Adjust**: 10 requests / minute
- **Read**: 60 requests / minute

If exceeded:
- Status code **429**
- Response includes Retry-After: 60

---

## Error handling

The API uses a global exception handler and returns ProblemDetails-style responses.
Common mappings:

- **400**: validation errors (including domain validation)
- **404**: wallet not found
- **409**: concurrency conflicts
- **502**: external service failures
- **500**: unexpected errors

---

## Tests

From the repo root:

`dotnet test NoviCode.sln`

Test projects:
- `Tests/NoviCode.EcbGateway.Tests`: XML parsing tests for ECB payloads
- `Tests/NoviCode.Infrastructure.Tests`: wallet service + exception mapping + caching fallback tests




