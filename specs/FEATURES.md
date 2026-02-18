# Features

Tracks all planned features, their status, and links to individual specs.

## Status Legend
- `[ ]` — Backlog
- `[~]` — In Progress
- `[x]` — Done

---

## Core Domain

| # | Feature | Status | Spec |
|---|---|---|---|
| 1 | Character creation (name, class, race selection) | `[ ]` | `features/character.md` |
| 2 | Character sheet (stats, level, XP bar, gold) | `[ ]` | `features/character.md` |
| 3 | Leveling system (XP thresholds, level-up event) | `[ ]` | `features/leveling.md` |
| 4 | Item catalog (seeded items per rarity/type) | `[ ]` | `features/items.md` |
| 5 | Inventory + equipment slots | `[ ]` | `features/items.md` |
| 6 | Reward system (XP, gold, item drops) | `[ ]` | `features/rewards.md` |

## Productivity Modules

| # | Feature | Status | Spec |
|---|---|---|---|
| 7 | Daily tasks — list, create, delete | `[ ]` | `features/daily-tasks.md` |
| 8 | Daily tasks — complete + reward | `[ ]` | `features/daily-tasks.md` |
| 9 | Daily tasks — midnight reset | `[ ]` | `features/daily-tasks.md` |
| 10 | Personal tasks — list, create, delete | `[ ]` | `features/personal-tasks.md` |
| 11 | Personal tasks — complete + reward | `[ ]` | `features/personal-tasks.md` |
| 12 | Expense tracker — list, create, delete | `[ ]` | `features/expenses.md` |
| 13 | Expense tracker — mark complete + reward | `[ ]` | `features/expenses.md` |

## UI / UX

| # | Feature | Status | Spec |
|---|---|---|---|
| 14 | 4Story-themed layout (dark, medieval, fantasy) | `[ ]` | `features/ui-theme.md` |
| 15 | Character panel (visual equipment slots) | `[ ]` | `features/ui-theme.md` |
| 16 | Reward animation (XP gain, level-up popup) | `[ ]` | `features/ui-theme.md` |
| 17 | Item tooltip on hover | `[ ]` | `features/ui-theme.md` |
| 18 | Item rarity color coding | `[ ]` | `features/ui-theme.md` |

## Enhancement System

| # | Feature | Status | Spec |
|---|---|---|---|
| 19 | Enhancement attempt (formula consumption, success/fail/destroy) | `[ ]` | `features/items.md` |
| 20 | Scroll of Reflection (safe downgrade) | `[ ]` | `features/items.md` |
| 21 | Protection Scroll (prevent destruction on fail) | `[ ]` | `features/items.md` |
| 22 | Enhancement history log | `[ ]` | `features/items.md` |

## Infrastructure

| # | Feature | Status | Spec |
|---|---|---|---|
| 23 | PostgreSQL + EF Core setup with per-module schemas | `[ ]` | `MODULES.md` |
| 24 | Switch Web project to Blazor WASM | `[ ]` | — |
| 25 | Logto OIDC integration (Web login + API JWT validation) | `[ ]` | — |
| 26 | Seed item catalog | `[ ]` | `features/items.md` |
| 27 | In-process event bus (IEventBus + IEventHandler<T>) | `[ ]` | `MODULES.md` |
| 28 | SSE endpoint + NotificationHub | `[ ]` | `MODULES.md` |
| 29 | Scheduler module + DailyResetJob | `[ ]` | `MODULES.md` |
