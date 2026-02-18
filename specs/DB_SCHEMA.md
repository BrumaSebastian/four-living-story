# Database Schema

## Overview
- **Database**: PostgreSQL
- **ORM / access**: EF Core 10 with Npgsql provider
- **Migrations**: EF Core migrations, applied on AppHost startup
- **Conventions**: snake_case column names (Npgsql convention), UUIDs as primary keys, `created_at` / `updated_at` on all tables

---

## Domain Model Overview

```
User (Logto) ──1:1──► Character ──1:1──► CharacterEquipment
                           │
                           ├──1:N──► InventorySlot ──N:1──► Item
                           ├──1:N──► DailyTask
                           ├──1:N──► DailyTaskCompletion
                           ├──1:N──► PersonalTask
                           └──1:N──► Expense
```

---

## Tables

### characters
One character per Logto user.

| Column | Type | Nullable | Description |
|---|---|---|---|
| id | uuid | NO | Primary key |
| user_id | text | NO | Logto `sub` claim — unique per user |
| name | text | NO | Character name |
| class | text | NO | `Warrior`, `Archer`, `Magician`, `Priest`, `Evocator` |
| race | text | NO | `Human`, `Feline`, `Fairy` |
| level | int | NO | Current level (starts at 1) |
| current_xp | int | NO | XP accumulated toward next level |
| gold | int | NO | Current gold |
| created_at | timestamptz | NO | |
| updated_at | timestamptz | NO | |

**Indexes**
- `idx_characters_user_id` UNIQUE on `user_id`

---

### items
The master item catalog — all possible items in the game (seeded, not user-created).

| Column | Type | Nullable | Description |
|---|---|---|---|
| id | uuid | NO | Primary key |
| name | text | NO | Item name |
| category | text | NO | `Equipment` or `Consumable` |
| type | text | NO | See Item Types below |
| rarity | text | NO | `Common`, `Uncommon`, `Rare`, `Epic`, `Legendary` |
| required_level | int | NO | Minimum character level to equip |
| allowed_classes | text[] | YES | Null = all classes; otherwise `{Warrior, Archer, ...}` |
| base_stats | jsonb | YES | e.g. `{"Strength": 10, "Agility": 5}` |
| description | text | YES | Flavor/lore text |
| image_path | text | YES | Path to item icon asset |
| created_at | timestamptz | NO | |

**Item Types**
- Equipment slots: `Headwear`, `Chest`, `Cape`, `Pants`, `Boots`
- Weapon slots: `Sword`, `TwoHandedSword`, `Axe`, `Bow`, `Crossbow`, `Dagger`, `Staff`, `Rod`, `Wand`, `Cane`, `Gantha`
- Accessory slots: `Ring`, `Necklace`
- Shield slot: `Shield`
- Consumables: `EnhancementFormula`, `ScrollOfReflection`, `ProtectionScroll`, `Potion`, `Other`

**Weapon Restrictions by Class**
| Class | Allowed Weapon Types |
|---|---|
| Warrior | Sword, TwoHandedSword, Axe, Crossbow, Bow |
| Archer | Bow, Crossbow, Dagger, Sword, TwoHandedSword |
| Magician | Staff, Rod, Cane, Gantha |
| Priest | Staff, Rod, Gantha |
| Evocator | Wand, Cane, Gantha |

---

### character_equipment
The currently equipped items for a character. One row per equipment slot.

| Column | Type | Nullable | Description |
|---|---|---|---|
| id | uuid | NO | Primary key |
| character_id | uuid | NO | FK → characters.id |
| slot | text | NO | Equipment slot identifier (see Slots below) |
| inventory_slot_id | uuid | YES | FK → inventory_slots.id — the equipped item instance |
| created_at | timestamptz | NO | |
| updated_at | timestamptz | NO | |

**Equipment Slots**
| Slot | Notes |
|---|---|
| `Headwear` | |
| `Chest` | |
| `Cape` | |
| `Pants` | |
| `Boots` | |
| `MainHand` | Weapon (class-restricted) |
| `OffHand` | Shield — only Warrior and Priest can equip shields |
| `Ring1` | |
| `Ring2` | |
| `Necklace` | |

**Indexes**
- `idx_character_equipment_character_slot` UNIQUE on `(character_id, slot)`

**Relations**
- `character_equipment.character_id` → `characters.id`
- `character_equipment.inventory_slot_id` → `inventory_slots.id`

---

### inventory_slots
Each row is one item instance owned by the character. Supports stacking for consumables.

| Column | Type | Nullable | Description |
|---|---|---|---|
| id | uuid | NO | Primary key |
| character_id | uuid | NO | FK → characters.id |
| item_id | uuid | NO | FK → items.id |
| enhancement_level | int | NO | +0 to +28 (equipment only; 0 for consumables) |
| visual_effect | text | YES | Cosmetic effect name assigned at +17 (e.g. `Blazing`). NULL below +17. No mechanical effect. |
| quantity | int | NO | Stack size (1 for equipment, N for consumables) |
| acquired_at | timestamptz | NO | When the item was obtained |

**Enhancement state rules (derived from `enhancement_level`, not stored):**
- +0 to +15: Normal
- +16: Golden (visual only)
- +17 to +28: Enchanted — `visual_effect` is populated; cleared if item drops below +17

**Relations**
- `inventory_slots.character_id` → `characters.id`
- `inventory_slots.item_id` → `items.id`

---

### item_enhancements
Audit log of every enhancement attempt on an inventory item.

| Column | Type | Nullable | Description |
|---|---|---|---|
| id | uuid | NO | Primary key |
| character_id | uuid | NO | FK → characters.id |
| inventory_slot_id | uuid | YES | NULL if item was destroyed |
| item_id | uuid | NO | The item that was enhanced (preserved even if destroyed) |
| formula_used | text | NO | `Apprentice`, `Assistant`, `Master` |
| level_before | int | NO | Enhancement level before attempt |
| level_after | int | YES | NULL if item was destroyed |
| success | bool | NO | Whether the attempt succeeded |
| item_destroyed | bool | NO | Whether the item broke on failure |
| visual_effect_assigned | text | YES | If this attempt crossed +16→+17: the cosmetic effect name that was rolled. NULL otherwise. |
| visual_effect_cleared | bool | NO | True if this attempt (via downgrade) caused the item to drop below +17, clearing its effect. |
| attempted_at | timestamptz | NO | |

**Relations**
- `item_enhancements.character_id` → `characters.id`
- `item_enhancements.inventory_slot_id` → `inventory_slots.id` (nullable — NULL if destroyed)

---

### daily_tasks
Recurring tasks that reset each day.
XP, gold, and drop chance are **not stored** — they are calculated at completion time from `difficulty` (see rewards.md).

| Column | Type | Nullable | Description |
|---|---|---|---|
| id | uuid | NO | Primary key |
| character_id | uuid | NO | FK → characters.id |
| title | text | NO | |
| description | text | YES | |
| difficulty | text | NO | `Easy`, `Medium`, `Hard`, `Extreme` — drives reward calculation |
| target_quantity | int | YES | Optional numeric goal, e.g. `25` |
| target_unit | text | YES | Label for target, e.g. `pushups`, `minutes`, `km` |
| sort_order | int | NO | Display ordering |
| is_active | bool | NO | Soft-delete flag |
| created_at | timestamptz | NO | |

**Relations**
- `daily_tasks.character_id` → `characters.id`

---

### daily_task_completions
Tracks which daily tasks were completed on which calendar day.

| Column | Type | Nullable | Description |
|---|---|---|---|
| id | uuid | NO | Primary key |
| daily_task_id | uuid | NO | FK → daily_tasks.id |
| character_id | uuid | NO | FK → characters.id |
| actual_quantity | int | YES | What the user actually did (e.g. 40 pushups); NULL for binary tasks |
| xp_awarded | int | NO | Recorded at time of completion (base + any overachievement bonus) |
| gold_awarded | int | NO | Recorded at time of completion |
| completed_on | date | NO | Calendar date (UTC) — used for daily reset check |
| completed_at | timestamptz | NO | Exact timestamp |

**Indexes**
- `idx_dtc_task_date` UNIQUE on `(daily_task_id, completed_on)`

**Relations**
- `daily_task_completions.daily_task_id` → `daily_tasks.id`
- `daily_task_completions.character_id` → `characters.id`

---

### personal_tasks
One-off tasks.
XP, gold, and drop chance are **not stored** — they are calculated at completion time from `difficulty`.

| Column | Type | Nullable | Description |
|---|---|---|---|
| id | uuid | NO | Primary key |
| character_id | uuid | NO | FK → characters.id |
| title | text | NO | |
| description | text | YES | |
| difficulty | text | NO | `Easy`, `Medium`, `Hard`, `Extreme` |
| target_quantity | int | YES | Optional numeric goal |
| target_unit | text | YES | Label for target, e.g. `pages`, `km` |
| actual_quantity | int | YES | Logged at completion; NULL for binary tasks |
| xp_awarded | int | YES | Recorded at time of completion; NULL until complete |
| gold_awarded | int | YES | Recorded at time of completion; NULL until complete |
| is_completed | bool | NO | |
| completed_at | timestamptz | YES | |
| due_date | date | YES | |
| created_at | timestamptz | NO | |

**Relations**
- `personal_tasks.character_id` → `characters.id`

---

### expenses
Expense entries to track and mark handled.
Rewards are fixed by the system: logging and completing any expense awards Easy-tier rewards (25 XP, 5 gold).
No difficulty field — all expense tracking is treated as an Easy-equivalent effort.

| Column | Type | Nullable | Description |
|---|---|---|---|
| id | uuid | NO | Primary key |
| character_id | uuid | NO | FK → characters.id |
| title | text | NO | |
| amount | numeric(12,2) | NO | |
| currency | text | NO | ISO 4217 code, e.g. `USD` |
| category | text | YES | `Food`, `Transport`, `Bills`, `Health`, `Entertainment`, `Shopping`, `Other` |
| notes | text | YES | |
| xp_awarded | int | YES | Recorded at time of completion; NULL until complete |
| gold_awarded | int | YES | Recorded at time of completion; NULL until complete |
| is_completed | bool | NO | |
| completed_at | timestamptz | YES | |
| due_date | date | YES | |
| created_at | timestamptz | NO | |

**Relations**
- `expenses.character_id` → `characters.id`
