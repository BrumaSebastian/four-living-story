# Modules

## Design Rules

1. **No cross-module service injection** — a module must not inject a service from another module.
2. **Events for cross-module communication** — modules publish events; other modules handle them.
3. **Each module owns its DB tables** — tables live in a dedicated PostgreSQL schema per module.
4. **Minimal API style** — endpoints are registered in each module via extension methods on `IEndpointRouteBuilder`.
5. **No MediatR, no repository pattern** — services use EF Core `AppDbContext` directly.
6. **A module = a folder** — `Modules/<Name>/` contains entities, services, endpoints, and event handlers.

---

## Module Map

```
Modules/
├── Identity/       — current user resolution from Logto JWT
├── Character/      — character sheet, leveling
├── Inventory/      — items, inventory, equipment, enhancement
├── Tasks/          — daily tasks, personal tasks
├── Expenses/       — expense tracking
├── Rewards/        — XP/gold/drop calculation (stateless)
├── Notifications/  — SSE push to browser
└── Scheduler/      — cron jobs
```

---

## Module: Identity

**Responsibility**: Resolve the Logto `sub` claim into an internal `CurrentUser` context available to all other modules via DI.

**DB schema**: none — no local user table. Identity is fully external (Logto).

**Endpoints**: none (auth is middleware, not an endpoint).

**Publishes**:
- `UserFirstLoginEvent` — fired once when a `sub` is seen for the first time (no character exists yet).

**Handles**: nothing.

**Key types**:
```
ICurrentUser            — interface: UserId (Logto sub), IsAuthenticated
CurrentUserMiddleware   — resolves ICurrentUser from JWT claims per request
```

---

## Module: Character

**Responsibility**: Character creation, the character sheet (stats, level, XP, gold), and leveling.

**DB schema**: `character`

**Tables owned**: `character.characters`

**Endpoints**:
- `GET /character` — get current character
- `POST /character` — create character (name, class, race)

**Publishes**:
- `CharacterLeveledUpEvent { CharacterId, OldLevel, NewLevel }`

**Handles**:
- `UserFirstLoginEvent` — no-op route; client is redirected to character creation UI
- `RewardGrantedEvent` — applies XP and gold to character, checks for level-up, publishes `CharacterLeveledUpEvent` if needed

**Notes**:
- Level-up formula: `XpRequired(level) = floor(500 × 1.2^(level-1))`
- Overflow XP carries over on level-up

---

## Module: Inventory

**Responsibility**: Master item catalog, inventory slots owned by the character, equipment slot management, and the enhancement system.

**DB schema**: `inventory`

**Tables owned**:
- `inventory.items` — seeded item catalog
- `inventory.inventory_slots` — item instances owned by character
- `inventory.character_equipment` — 10 equipment slots per character
- `inventory.item_enhancements` — enhancement attempt audit log

**Endpoints**:
- `GET /inventory` — list all owned items
- `POST /inventory/{slotId}/equip` — equip item
- `POST /inventory/{slotId}/unequip` — unequip item
- `POST /inventory/{slotId}/enhance` — attempt enhancement (consumes formula from inventory)
- `POST /inventory/{slotId}/downgrade` — use Scroll of Reflection

**Publishes**:
- `ItemEquippedEvent { CharacterId, SlotId, Slot }`
- `ItemEnhancedEvent { CharacterId, SlotId, LevelBefore, LevelAfter, Success, Destroyed, VisualEffectAssigned }`

**Handles**:
- `RewardGrantedEvent` — if `DroppedItemId` is set, adds the item to the character's inventory

---

## Module: Tasks

**Responsibility**: Daily habits and one-off personal tasks. Owns the daily midnight reset.

**DB schema**: `tasks`

**Tables owned**:
- `tasks.daily_tasks`
- `tasks.daily_task_completions`
- `tasks.personal_tasks`

**Endpoints**:
- `GET /daily-tasks`
- `POST /daily-tasks`
- `DELETE /daily-tasks/{id}`
- `POST /daily-tasks/{id}/complete`
- `GET /personal-tasks`
- `POST /personal-tasks`
- `DELETE /personal-tasks/{id}`
- `POST /personal-tasks/{id}/complete`

**Publishes**:
- `DailyTaskCompletedEvent { TaskId, CharacterId, Difficulty, TargetQuantity, ActualQuantity }`
- `PersonalTaskCompletedEvent { TaskId, CharacterId, Difficulty, TargetQuantity, ActualQuantity }`
- `DailyResetTriggeredEvent { Date }` — published by the Scheduler module, handled here

**Handles**:
- `DailyResetTriggeredEvent` — clears all completions for the previous day

**Notes**:
- Completion endpoints call `RewardCalculator` (from Rewards module — the only allowed cross-module call, as it is a **pure stateless service with no DB access**), get the result, publish the event, and return the reward in the HTTP response.

---

## Module: Expenses

**Responsibility**: Expense entry tracking.

**DB schema**: `expenses`

**Tables owned**: `expenses.expenses`

**Endpoints**:
- `GET /expenses`
- `POST /expenses`
- `DELETE /expenses/{id}`
- `POST /expenses/{id}/complete`

**Publishes**:
- `ExpenseCompletedEvent { ExpenseId, CharacterId }`

**Handles**: nothing.

**Notes**:
- Same reward pattern as Tasks: calls `RewardCalculator`, publishes event, returns result.
- Expenses always use Easy-tier rewards (25 XP, 5 gold) — no difficulty field.

---

## Module: Rewards

**Responsibility**: Pure reward calculation. No DB access. No endpoints. No state.

**DB schema**: none.

**Endpoints**: none.

**Publishes**:
- `RewardGrantedEvent { CharacterId, XpAwarded, BonusXp, GoldAwarded, DroppedItemId? }`

**Handles**:
- `DailyTaskCompletedEvent` → calculates reward → publishes `RewardGrantedEvent`
- `PersonalTaskCompletedEvent` → calculates reward → publishes `RewardGrantedEvent`
- `ExpenseCompletedEvent` → calculates fixed reward → publishes `RewardGrantedEvent`

**Key types**:
```
RewardCalculator   — stateless service; injectable by Tasks and Expenses modules
                     (exception to the no-cross-module rule: it has no DB access)

RewardResult       — { BaseXp, BonusXp, TotalXp, Gold, DroppedItemId? }
```

**Reward formulas**:
```
Base rewards by difficulty:
  Easy:    25 XP,  5 gold, 2% drop
  Medium:  60 XP, 12 gold, 5% drop
  Hard:   120 XP, 24 gold, 10% drop
  Extreme:250 XP, 50 gold, 20% drop

Overachievement bonus (when actualQuantity > targetQuantity):
  cappedRatio = min((actual - target) / target, 0.5)
  bonusXp     = floor(baseXp   × cappedRatio)
  bonusGold   = floor(baseGold × cappedRatio)

Item drop: roll random 0–1; if < dropChance, pick item weighted by rarity.
```

---

## Module: Notifications

**Responsibility**: Manage SSE connections and push real-time events to the browser client.

**DB schema**: none (stateless; no notification history for now).

**Endpoints**:
- `GET /notifications/stream` — SSE endpoint; client connects once and keeps the connection open

**Publishes**: nothing.

**Handles**:
- `RewardGrantedEvent` → push `reward-granted` SSE message
- `CharacterLeveledUpEvent` → push `level-up` SSE message
- `ItemEnhancedEvent` → push `item-enhanced` SSE message
- `ItemEquippedEvent` → push `item-equipped` SSE message

**Key types**:
```
NotificationHub   — holds active SSE response streams keyed by UserId
                    channels are CancellationToken-aware Channel<SseMessage>
SseMessage        — { Event: string, Data: string (JSON) }
```

**Notes**:
- Uses ASP.NET Core response streaming (`text/event-stream`)
- One connection per user (single-user app; simple map of `UserId → Channel`)
- If the client disconnects and reconnects, it gets a fresh channel (no replay)

---

## Module: Scheduler

**Responsibility**: Time-based background jobs using `IHostedService` + `PeriodicTimer`.

**DB schema**: none.

**Endpoints**: none.

**Publishes**:
- `DailyResetTriggeredEvent { Date }` — published at midnight UTC every day

**Handles**: nothing.

**Jobs**:
| Job | Schedule | Event Published |
|---|---|---|
| `DailyResetJob` | Every day at 00:00:00 UTC | `DailyResetTriggeredEvent` |

**Notes**:
- Uses `PeriodicTimer` calibrated to fire at the next midnight boundary.
- No Quartz.NET dependency — keeps infrastructure minimal.

---

## Event Bus

In-process, synchronous event bus. Events are dispatched in the same request thread.
No serialization. No broker. No outbox pattern (single-user, low risk of partial failure).

```
IEventBus
  Publish<TEvent>(TEvent @event)   — resolves all IEventHandler<TEvent> from DI and invokes them

IEventHandler<TEvent>
  Handle(TEvent @event)
```

All handlers are registered in DI as `IEventHandler<T>` during module registration.

---

## Event Catalog

| Event | Published By | Handled By |
|---|---|---|
| `UserFirstLoginEvent` | Identity | Character |
| `DailyTaskCompletedEvent` | Tasks | Rewards |
| `PersonalTaskCompletedEvent` | Tasks | Rewards |
| `ExpenseCompletedEvent` | Expenses | Rewards |
| `RewardGrantedEvent` | Rewards | Character, Inventory, Notifications |
| `CharacterLeveledUpEvent` | Character | Notifications |
| `ItemEquippedEvent` | Inventory | Notifications |
| `ItemEnhancedEvent` | Inventory | Notifications |
| `DailyResetTriggeredEvent` | Scheduler | Tasks |

---

## DB Schema Ownership

| PostgreSQL Schema | Owned By Module | Tables |
|---|---|---|
| `character` | Character | `characters` |
| `inventory` | Inventory | `items`, `inventory_slots`, `character_equipment`, `item_enhancements` |
| `tasks` | Tasks | `daily_tasks`, `daily_task_completions`, `personal_tasks` |
| `expenses` | Expenses | `expenses` |

Cross-schema foreign keys are **avoided**. Modules reference entities from other modules by ID only (e.g. `character_id` is just a `uuid` column — no FK constraint crossing schema boundaries). Referential integrity for cross-module IDs is enforced at the application level.
