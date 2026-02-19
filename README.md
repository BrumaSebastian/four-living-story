# FourLivingStory

A gamified personal productivity tracker styled after the [4Story](https://4story.gameforge.com) MMORPG.

Complete real-life tasks and habits → earn XP, gold, and item drops → level up your character → unlock better gear.

---

## Stack

| Concern | Technology |
|---|---|
| Runtime | .NET 10 (Aspire) |
| Frontend | Blazor WebAssembly (hosted) |
| Backend | Minimal API — modular monolith |
| Database | PostgreSQL + EF Core 10 |
| Auth | Logto (OIDC / JWT) |
| Event bus | Wolverine (PostgreSQL outbox) |
| Observability | OpenTelemetry (traces, metrics, logs) |
| Real-time | Server-Sent Events (SSE) |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (Aspire spins up PostgreSQL + pgAdmin containers)
- A [Logto](https://logto.io) instance with a configured application (for auth — optional during early development)

---

## Running the app

```bash
# From the repo root
dotnet run --project src/FourLivingStory/FourLivingStory.AppHost
```

Aspire will start all services and open the Aspire dashboard. The app is accessible at:

- **Web (Blazor WASM):** https://localhost:7030
- **API:** https://localhost:7509
- **pgAdmin:** http://localhost:5050
- **Aspire dashboard:** http://localhost:15888

---

## Running tests

```bash
dotnet test src/FourLivingStory/FourLivingStory.Tests
```

Tests use Aspire's integration testing host — the full application stack is started for each test run.

---

## Building

```bash
dotnet build src/FourLivingStory/FourLivingStory.slnx
```

---

## Solution structure

```
four-living-story/
├── specs/                              # Project specs, architecture decisions, DB schema
└── src/FourLivingStory/
    ├── FourLivingStory.slnx
    ├── FourLivingStory.AppHost/        # Aspire orchestration (PostgreSQL, pgAdmin, all services)
    ├── FourLivingStory.Domain/         # Pure domain event records — no framework dependencies
    ├── FourLivingStory.Application/    # App services, ports, module discovery
    ├── FourLivingStory.Infrastructure/ # EF Core, Wolverine event bus, PostgreSQL outbox
    ├── FourLivingStory.ApiService/     # HTTP composition root — Minimal API endpoints
    ├── FourLivingStory.Web/            # Thin Blazor WASM host (serves the client app)
    ├── FourLivingStory.Web.Client/     # Blazor WASM app (all pages and UI live here)
    ├── FourLivingStory.ServiceDefaults/# Shared OpenTelemetry, health checks, resilience
    └── FourLivingStory.Tests/          # Aspire integration tests
```

### Layer dependencies

```
Domain  ←  Application  ←  Infrastructure  ←  ApiService
                ↑                                   ↑
           (module ports)                   (HTTP composition)
```

- **Domain** — pure C# records, no NuGet dependencies
- **Application** — domain services, `IEventBus` port, `IServiceModule`/`IEndpointModule` interfaces, module discovery via reflection
- **Infrastructure** — `AppDbContext`, `WolverineEventBus` (PostgreSQL outbox), `InfrastructureServiceModule`
- **ApiService** — `Program.cs` wires everything with `AddModules()`/`MapModules()`; endpoint classes in `Endpoints/{Module}/`

---

## Modules

| Module | Responsibility |
|---|---|
| **Identity** | Resolves the current Logto user (`sub` claim) into `ICurrentUser` |
| **Character** | Character entity, leveling, XP accumulation, stats |
| **Inventory** | Item catalog, inventory slots, equipment, enhancement system |
| **Tasks** | Daily tasks, personal tasks, daily reset |
| **Expenses** | Expense entries — completing an entry awards rewards |
| **Rewards** | Stateless XP / gold / item drop calculator (difficulty tiers) |
| **Notifications** | Server-Sent Events — pushes reward and level-up events to the browser |
| **Scheduler** | `DailyResetJob` background service — fires `DailyResetTriggeredEvent` at midnight UTC |

---

## Event bus

Cross-module communication uses an in-process event bus backed by [Wolverine](https://wolverine.fm).

**Publishing** (in a handler or endpoint):
```csharp
// IEventBus is injected — it wraps Wolverine's IMessageBus
await eventBus.PublishAsync(new RewardGrantedEvent(...));
```

**Handling** — no interface required, Wolverine discovers handlers by convention:
```csharp
public sealed class RewardGrantedHandler
{
    public async Task HandleAsync(RewardGrantedEvent evt, AppDbContext db)
    {
        // Dependencies injected as method parameters
    }
}
```

With `UseEntityFrameworkCoreTransactions()` configured, published messages are written to the PostgreSQL outbox atomically with `SaveChangesAsync()` — no phantom events if a transaction rolls back.

---

## EF Core migrations

`AppDbContext` lives in the Infrastructure project. Run migrations from the repo root:

```bash
dotnet ef migrations add <Name> \
  --project src/FourLivingStory/FourLivingStory.Infrastructure \
  --startup-project src/FourLivingStory/FourLivingStory.ApiService
```

---

## Configuration

| Key | Where | Description |
|---|---|---|
| `Logto:Authority` | `appsettings.json` | Logto OIDC authority URL |
| `Logto:Audience` | `appsettings.json` | Logto API resource identifier |
| `Cors:AllowedOrigins` | `appsettings.Development.json` | Allowed CORS origins (Web host URLs) |
| `ConnectionStrings:fourlivingstory` | Injected by Aspire | PostgreSQL connection string |
