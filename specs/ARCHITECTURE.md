# Architecture

## Overview
.NET Aspire application (net10.0) with a Blazor WebAssembly frontend, a Minimal API backend, PostgreSQL for persistence, Redis for output caching, and Logto for authentication.

## Projects
| Project | Role |
|---|---|
| `FourLivingStory.AppHost` | Aspire orchestration — wires up all services |
| `FourLivingStory.Domain` | Pure domain event records — no framework dependencies |
| `FourLivingStory.Application` | App services, ports (`IEventBus`), module discovery, domain services |
| `FourLivingStory.Infrastructure` | EF Core (`AppDbContext`), Wolverine event bus, PostgreSQL outbox |
| `FourLivingStory.ApiService` | HTTP composition root — Minimal API endpoints, `Program.cs` |
| `FourLivingStory.Web` | Thin Blazor WASM host — serves client app, exposes `/_config` |
| `FourLivingStory.Web.Client` | Blazor WASM app — all pages and UI components |
| `FourLivingStory.ServiceDefaults` | Shared defaults — OpenTelemetry, health checks, resilience |
| `FourLivingStory.Tests` | Integration tests — spins up full AppHost via Aspire testing |

## Infrastructure
| Concern | Technology |
|---|---|
| Database | PostgreSQL (via Aspire.Hosting.PostgreSQL) |
| ORM | EF Core 10 with Npgsql provider |
| Migrations | EF Core migrations, applied on startup |
| Cache | Redis (output caching on Web) |
| Auth | Logto (OIDC/OAuth2) |
| Observability | OpenTelemetry (traces, metrics, logs) |
| Health checks | `/health` on ApiService and Web |

## Data Flow
```
User (Browser)
  → [Logto OIDC Login]
  → Blazor WASM (Web, runs in browser)
      → HttpClient + Bearer token (service discovery: https+http://apiservice)
          → Minimal API (ApiService)
              → EF Core → PostgreSQL
```

## Authentication Flow
- Blazor WASM handles the OIDC login redirect via Logto
- Logto issues a JWT (access token) on successful login
- Every API request carries the JWT as a Bearer token in the `Authorization` header
- ApiService validates the JWT against Logto's JWKS endpoint
- The `sub` claim (Logto user ID) is used to look up or create the user's Character

## Backend Internal Structure

The backend is a **modular monolith** split across four projects following a clean 3-layer architecture.

### Layer rules

| Layer | Project | Dependencies |
|---|---|---|
| Domain | `FourLivingStory.Domain` | none |
| Application | `FourLivingStory.Application` | Domain |
| Infrastructure | `FourLivingStory.Infrastructure` | Application + Domain |
| HTTP entry point | `FourLivingStory.ApiService` | Application + Infrastructure + ServiceDefaults |

### Application layer — module layout

```
FourLivingStory.Application/
├── IServiceModule.cs          — Implemented by each module to register services
├── IEndpointModule.cs         — Implemented by endpoint classes to map routes
├── ModuleExtensions.cs        — AddModules() / MapModules() via reflection
├── Infrastructure/EventBus/   — IEventBus, IEventHandler<T> ports
└── Modules/
    ├── Identity/              — ICurrentUser, CurrentUser, IdentityServiceModule
    ├── Character/             — CharacterServiceModule (stub)
    ├── Inventory/             — InventoryServiceModule (stub)
    ├── Tasks/                 — TasksServiceModule (stub)
    ├── Expenses/              — ExpensesServiceModule (stub)
    ├── Rewards/               — RewardCalculator, RewardsServiceModule
    ├── Notifications/         — NotificationHub (SSE), NotificationsServiceModule
    └── Scheduler/             — DailyResetJob, SchedulerServiceModule
```

### ApiService — endpoints only

```
FourLivingStory.ApiService/
├── Endpoints/
│   ├── Notifications/     — GET /notifications/stream (SSE)
│   ├── Character/         — (stub)
│   ├── Inventory/         — (stub)
│   ├── Tasks/             — (stub)
│   └── Expenses/          — (stub)
└── Program.cs             — AddModules() + MapModules() over all 4 assemblies
```

See `specs/MODULES.md` for the full module breakdown, event catalog, and DB schema ownership.

## Architectural Decisions

- **Blazor WASM over Blazor Server**: UI runs entirely in the browser for richer client-side interactivity (animations, character sheet, tooltips).
- **PostgreSQL over SQLite**: Better suited for production use; integrates natively with Aspire hosting. Supports full-text search and JSON columns for item stat blobs.
- **Logto for auth**: OIDC-compliant, self-hostable auth provider. Avoids building auth from scratch while keeping control of user data.
- **No on-premise user table**: User identity comes entirely from Logto's `sub` claim. The local DB stores only a `UserId` (Logto sub) on the Character.
- **EF Core**: Standard ORM for .NET; integrates cleanly with Aspire and Npgsql.
- **Modular monolith**: Single deployable unit. Modules are isolated — no direct service-to-service calls across module boundaries. Cross-module communication uses the event bus.
- **No MediatR, no repository pattern**: Minimal API endpoints call module services directly. EF Core DbContext is used directly inside services.
- **Wolverine event bus with PostgreSQL outbox**: Messages are written to the outbox atomically with `SaveChangesAsync()`. No phantom events on transaction rollback. Handler discovery is convention-based — any class with a `HandleAsync` method is automatically registered.
- **IEventBus abstraction**: Application layer exposes `IEventBus` (our port). Infrastructure implements it via `WolverineEventBus` wrapping `IMessageBus`. Application stays framework-agnostic.
- **Async event bus**: `IEventBus.PublishAsync` returns `ValueTask`; handlers can be fully async.
- **SSE for real-time**: Server-Sent Events pushed from the Notifications module on reward events, level-ups, and item drops.
- **Cron jobs via IHostedService**: Scheduled work (daily task reset) runs inside the same process. `TimeProvider` is injected for testability — no `DateTime.UtcNow` usage.
