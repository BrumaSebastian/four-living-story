# Feature: Items, Inventory & Enhancement

## Goal
A rich item system with a master catalog of equipment and consumables. Characters collect items into their inventory, equip them, and can enhance them using consumable formulas — with visual transformation and random magical effects at high enhancement levels.

---

## Item Categories

### Equipment
Items that occupy an equipment slot on the character. Class and level restricted.

| Type | Slot |
|---|---|
| Headwear | Headwear |
| Chest | Chest |
| Cape | Cape |
| Pants | Pants |
| Boots | Boots |
| Sword | MainHand |
| TwoHandedSword | MainHand |
| Axe | MainHand |
| Bow | MainHand |
| Crossbow | MainHand |
| Dagger | MainHand |
| Staff | MainHand |
| Rod | MainHand |
| Wand | MainHand |
| Cane | MainHand |
| Gantha | MainHand |
| Shield | OffHand |
| Ring | Ring1 / Ring2 |
| Necklace | Necklace |

### Consumables
Items used from the inventory for specific actions.

| Type | Use |
|---|---|
| `EnhancementFormula` | Used to attempt item enhancement (Apprentice / Assistant / Master) |
| `ScrollOfReflection` | Downgrades an item by one level (avoids high-risk upgrade attempts) |
| `ProtectionScroll` | Protects item from destruction on a failed enhancement attempt |
| `Potion` | Consumable (future use — e.g. XP boost) |

---

## Rarity Tiers

| Rarity | Color | Display |
|---|---|---|
| Common | `#9d9d9d` gray | No glow |
| Uncommon | `#1eff00` green | Subtle glow |
| Rare | `#0070dd` blue | Medium glow |
| Epic | `#a335ee` purple | Strong glow |
| Legendary | `#ff8000` orange | Animated glow |

---

## Enhancement System

Based on 4Story's refinement mechanic.

### Enhancement Levels Overview

| Range | State | Visual | Effect |
|---|---|---|---|
| +0 to +15 | Normal | Default item appearance | Stat bonus only |
| +16 | Golden | Item name and border turn **gold** | Stat bonus only |
| +17 to +24 | Enchanted | Golden + **random magical effect** active | Stat bonus + magic effect |
| +25 to +28 | Transcendent | Golden + effect + **enhanced glow** | Stat bonus + magic effect |

> The effect at +17 stays fixed until the item drops below +17. If it rises back to +17, a new effect is rolled.

---

### Formula Types
| Formula | Levels Per Success | Risk |
|---|---|---|
| Apprentice's Formula | +1 | Lower risk at low levels |
| Assistant's Formula | +2 | Medium risk |
| Master's Formula | +3 | Higher risk |

---

### Failure Mechanics
- **Failed attempt**: Item is **destroyed** (removed from inventory) — no downgrade, it's gone
- **Scroll of Reflection**: Downgrades the item by 1 level safely (no destruction risk, consumes the scroll)
- **Protection Scroll**: Consumed alongside the formula; if attempt fails, item is NOT destroyed (no level gain, but item survives)

---

### Enhancement Success Rates (approximate)
| Level | Approx. Success Rate |
|---|---|
| +0 → +1 | 100% |
| +1 → +2 | 95% |
| +2 → +3 | 90% |
| +3 → +4 | 80% |
| +4 → +5 | 70% |
| +5 → +6 | 60% |
| +6 → +7 | 50% |
| +7 → +8 | 40% |
| +8 → +9 | 30% |
| +9 → +10 | 20% |
| +10 → +15 | ≤15%, decreasing |
| +15 → +16 | 10% |
| +16 → +17 | 8% |
| +17 and above | ≤6%, decreasing |

---

### Stat Increase from Enhancement
Each +1 level adds a **flat amount** to every stat the item already has.
Exact flat increment per level TBD in implementation (e.g. +2 per stat per level).
Enhancement does **not** add new stats — it only grows the ones already on the item.

---

### Golden State (+16)
**Visual only — no additional stat effect.**

- Item name rendered in gold color
- Item border / tooltip frame turns gold
- Enhancement stat increases continue to apply normally

---

### Magic Effect System (+17 to +28)
**Visual only — purely cosmetic. No mechanical bonus is granted.**

#### Trigger
When an item's enhancement level crosses **from +16 to +17**, the system assigns a random visual effect name and stores it on the inventory item instance. This is cosmetic flavour — a name and particle/glow style shown in the tooltip and on the character sheet.

#### Effect Persistence
- The assigned visual effect remains **fixed** as long as the item stays at +17 or above
- Enhancing further (+18, +19 … +28) does **not** re-roll the effect
- If the item is downgraded below +17 (via Scroll of Reflection), the visual effect is **cleared**
- If the item is enhanced back to +17, a **new** visual effect is rolled

#### Visual Effect Names (cosmetic pool)
Examples of effect names that are randomly assigned — displayed in the tooltip for flavour:

`Blazing`, `Frozen`, `Stormbound`, `Void-touched`, `Soulforged`, `Cursed`,
`Radiant`, `Ancient`, `Shadowmarked`, `Bloodbound`, `Celestial`, `Plagued`,
`Ironwilled`, `Frostbitten`, `Emberstruck`, `Runic`, `Wraithbound`, `Sanctified`

#### Effect Display
- Effect name shown in the item tooltip in a distinct cyan color (e.g. `#00e5ff`) as pure flavour text
- Item name gains a shimmer/glow animation while the effect is active
- Character sheet slot visually distinguishes enchanted (+17+) items from golden (+16) items

---

## Inventory Behavior
- Equipment items: quantity = 1, not stackable
- Consumables: stackable (quantity > 1 per inventory slot)
- Inventory displayed as a grid
- Equipping: click item → "Equip" → swaps into matching slot (displaced item returns to inventory)
- Unequip: click equipped slot → "Unequip" → item returns to inventory

---

## Item Catalog Seeding
The item catalog (`items` table) is seeded via EF Core data seeder on first migration. Players cannot create items — they only obtain them through:
- Task completion drops (random roll)
- Future: in-game shop (TBD)

## Status: Backlog
