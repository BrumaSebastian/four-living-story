# Architecture

## Overview
.NET Aspire application (net10.0) with a Blazor WebAssembly frontend, a Minimal API backend, PostgreSQL for persistence, Redis for output caching, and Logto for authentication.

## Projects
| Project | Role |
|---|---|
| `FourLivingStory.AppHost` | Aspire orchestration — wires up all services |
| `FourLivingStory.ApiService` | Minimal API — business logic and data access |
| `FourLivingStory.Web` | Blazor WebAssembly — UI, calls ApiService via service discovery |
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

## ApiService Internal Structure

The ApiService is a **modular monolith**: a single deployable unit organized into self-contained modules.

```
FourLivingStory.ApiService/
├── Modules/
│   ├── Identity/          — Logto JWT resolution, current-user context
│   ├── Character/         — Character entity, leveling, stats
│   ├── Inventory/         — Items catalog, inventory slots, equipment, enhancement
│   ├── Tasks/             — Daily tasks, personal tasks, daily reset
│   ├── Expenses/          — Expense entries
│   ├── Rewards/           — XP/gold/drop calculation (stateless, no DB)
│   ├── Notifications/     — Server-Sent Events (SSE) push
│   └── Scheduler/         — Cron jobs (daily reset, etc.)
├── Infrastructure/
│   ├── EventBus/          — In-process synchronous event bus
│   ├── Database/          — AppDbContext, migrations
│   └── Auth/              — JWT validation middleware
└── Program.cs             — Composes all modules
```

See `specs/MODULES.md` for the full module breakdown, event catalog, and DB schema ownership.

## Architectural Decisions

- **Blazor WASM over Blazor Server**: UI runs entirely in the browser for richer client-side interactivity (animations, character sheet, tooltips).
- **PostgreSQL over SQLite**: Better suited for production use; integrates natively with Aspire hosting. Supports full-text search and JSON columns for item stat blobs.
- **Logto for auth**: OIDC-compliant, self-hostable auth provider. Avoids building auth from scratch while keeping control of user data.
- **No on-premise user table**: User identity comes entirely from Logto's `sub` claim. The local DB stores only a `UserId` (Logto sub) on the Character.
- **EF Core**: Standard ORM for .NET; integrates cleanly with Aspire and Npgsql.
- **Modular monolith**: Single deployable unit. Modules are isolated — no direct service-to-service calls across module boundaries. Cross-module communication uses an in-process event bus.
- **No MediatR, no repository pattern**: Minimal API endpoints call module services directly. EF Core DbContext is used directly inside services.
- **In-process event bus**: Synchronous, in-memory. No broker needed for a single-user app. Can be swapped for a message broker later if needed.
- **SSE for real-time**: Server-Sent Events pushed from the Notifications module on reward events, level-ups, and item drops.
- **Cron jobs via IHostedService**: Scheduled work (daily task reset) runs inside the same process using .NET's PeriodicTimer.
