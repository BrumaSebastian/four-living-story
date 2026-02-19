# Progress

## Status: Infrastructure complete — ready for feature implementation

## Completed
- [x] .NET Aspire project scaffold (AppHost, ApiService, Web, ServiceDefaults, Tests)
- [x] OpenTelemetry and health checks via ServiceDefaults
- [x] `specs/` folder and project documentation initialized
- [x] Domain defined (see PROJECT.md)
- [x] Architecture decided (Blazor WASM hosted, PostgreSQL + EF Core, Logto auth)
- [x] DB schema designed (see DB_SCHEMA.md)
- [x] API contracts designed (see API.CONTRACTS.md)
- [x] Feature list created (see FEATURES.md)
- [x] Character classes/races/weapon restrictions documented (see features/character.md)
- [x] Item enhancement system documented (see features/items.md)
- [x] Modular monolith architecture defined (see MODULES.md)
- [x] PostgreSQL + EF Core wired up (AppDbContext, Aspire.Npgsql.EntityFrameworkCore.PostgreSQL)
- [x] All 8 modules scaffolded: Identity, Character, Inventory, Tasks, Expenses, Rewards, Notifications, Scheduler
- [x] JWT Bearer auth configured in ApiService (Logto OIDC)
- [x] SSE notifications (NotificationHub singleton, GET /notifications/stream)
- [x] Daily reset cron job (DailyResetJob BackgroundService)
- [x] RewardCalculator (stateless singleton, difficulty tiers, overachievement bonus)
- [x] Modular monolith split into Domain / Application / Infrastructure / ApiService layers
- [x] Module discovery via reflection (AddModules / MapModules — no manual wiring in Program.cs)
- [x] Wolverine event bus with PostgreSQL outbox (replaces InMemoryEventBus)
- [x] IEventBus / IEventHandler ports in Application; WolverineEventBus adapter in Infrastructure
- [x] Async event bus (IEventBus.PublishAsync returns ValueTask)
- [x] TimeProvider injected into DailyResetJob (testable, no DateTime.UtcNow)
- [x] Blazor WASM hosted setup: Web.Client (WASM) + Web (thin host)
- [x] OIDC/PKCE auth configured in Web.Client (Logto)
- [x] Dynamic ApiService URL discovery via GET /_config

## In Progress
- [ ] Logto credentials (Authority + ClientId) — fill in appsettings once Logto app is created

## Backlog
See FEATURES.md for full feature tracking.

### Recommended Implementation Order
1. Configure Logto (create app, fill in Authority + ClientId in appsettings)
2. Switch EnsureCreatedAsync → MigrateAsync + add first EF Core migration
3. Seed item catalog
4. Character creation + character sheet UI
6. Leveling system
7. Inventory + equipment slots
8. Enhancement system
9. Daily tasks module
10. Personal tasks module
11. Expense tracker module
12. Reward system (XP, gold, item drops)
13. 4Story UI theme

## Decisions Made
- **Frontend**: Blazor WebAssembly (changed from initial Blazor Server scaffold)
- **Database**: PostgreSQL (changed from SQLite — production-grade, native Aspire support)
- **ORM**: EF Core 10 with Npgsql provider
- **Auth**: Logto (OIDC) — `sub` claim links Logto user to Character
- **No local user table**: Identity fully managed by Logto
- **Enhancement system**: 4Story-accurate — +0 to +28, item destruction on failure, protection scrolls
- **Classes**: Warrior, Archer, Magician, Priest, Evocator
- **Races**: Human, Feline, Fairy
- **Backend structure**: Modular monolith — 8 modules (Identity, Character, Inventory, Tasks, Expenses, Rewards, Notifications, Scheduler)
- **No MediatR, no repository pattern**: services use EF Core directly, endpoints use Minimal API
- **Event bus**: Wolverine with PostgreSQL outbox — messages committed atomically with DbContext; async (ValueTask); convention-based handler discovery
- **SSE**: Notifications module manages Server-Sent Events for real-time push
- **Cron jobs**: Scheduler module uses IHostedService + TimeProvider (no Quartz.NET)
- **DB schemas**: one PostgreSQL schema per module; no cross-schema FK constraints
