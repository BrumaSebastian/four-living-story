# Progress

## Status: Planning

## Completed
- [x] .NET Aspire project scaffold (AppHost, ApiService, Web, ServiceDefaults, Tests)
- [x] Redis output caching configured on Web
- [x] OpenTelemetry and health checks via ServiceDefaults
- [x] `specs/` folder and project documentation initialized
- [x] Domain defined (see PROJECT.md)
- [x] Architecture decided (Blazor WASM, PostgreSQL + EF Core, Logto auth)
- [x] DB schema designed — characters, items, inventory, equipment, enhancement, tasks, expenses (see DB_SCHEMA.md)
- [x] API contracts designed (see API.CONTRACTS.md)
- [x] Feature list created (see FEATURES.md)
- [x] Character classes/races/weapon restrictions documented (see features/character.md)
- [x] Item enhancement system documented (see features/items.md)

## In Progress
- [ ] Infrastructure setup

## Backlog
See FEATURES.md for full feature tracking.

### Recommended Implementation Order
1. Switch Web to Blazor WASM
2. Add PostgreSQL + EF Core to ApiService (Npgsql, migrations)
3. Integrate Logto into Web (OIDC login) and ApiService (JWT validation)
4. Seed item catalog
5. Character creation + character sheet UI
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
