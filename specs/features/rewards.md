# Feature: Reward System

## Goal
Every task completion awards XP, gold, and a chance at an item drop.
Rewards are **calculated by the system** — the user never sets them directly.
The system uses the task's difficulty and, where applicable, how much the user overachieved their target.

---

## What the User Sets
- **Difficulty**: Easy / Medium / Hard / Extreme
- **Target** *(optional)*: a numeric goal + unit (e.g. `25 pushups`, `30 minutes`, `5 km`)

The system derives all XP, gold, and drop chance from these two inputs.

---

## Base Rewards by Difficulty

| Difficulty | Base XP | Base Gold | Item Drop Chance |
|---|---|---|---|
| Easy | 25 | 5 | 2% |
| Medium | 60 | 12 | 5% |
| Hard | 120 | 24 | 10% |
| Extreme | 250 | 50 | 20% |

---

## Overachievement Bonus

Applies **only** when a task has a numeric target and the user logs an `actualQuantity` at completion.

```
overRatio  = (actualQuantity - targetQuantity) / targetQuantity
cappedRatio = min(overRatio, 0.5)           -- bonus capped at 50% of base
bonusXp    = floor(baseXp   * cappedRatio)
bonusGold  = floor(baseGold * cappedRatio)
```

**Example:** Daily task "25 pushups", difficulty Hard (120 XP base). User logs 40.

```
overRatio  = (40 - 25) / 25 = 0.60  → capped at 0.50
bonusXp    = floor(120 * 0.50) = 60
totalXp    = 120 + 60 = 180 XP
```

If the user logs exactly the target (or less), no bonus is applied. Logging less than the target still counts as a completion but awards **only base rewards** with no penalty — the task is either done or not done.

---

## Reward Calculation Flow

1. User submits completion (with optional `actualQuantity`)
2. API looks up task difficulty → fetches base rewards
3. If task has a `targetQuantity` and `actualQuantity > targetQuantity` → compute bonus
4. Apply total XP + gold to character
5. Roll item drop (0–1 against `dropChance`)
   - If hit: pick a random item from catalog weighted by rarity (see weights below)
   - Add item to character inventory
6. Check if XP threshold crossed → level up if needed
7. Return full reward summary to client

---

## Item Drop Weights by Rarity

| Rarity | Weight |
|---|---|
| Common | 60 |
| Uncommon | 25 |
| Rare | 10 |
| Epic | 4 |
| Legendary | 1 |

---

## Reward Response Shape
```json
{
  "xpAwarded": 180,
  "bonusXp": 60,
  "goldAwarded": 18,
  "bonusGold": 6,
  "itemDropped": { "id": "...", "name": "...", "rarity": "Rare", ... } | null,
  "leveledUp": false,
  "newLevel": 3
}
```

---

## UI
- Toast popup: "+180 XP  (+60 overachievement bonus)"  "+18 Gold"
- Separate animated popup if item dropped: item card with rarity border glow
- Level-up banner overlay (dramatic, full-screen-ish) if leveled up

## Status: Backlog
