# Feature: Character

## Goal
Each Logto user has exactly one character. Character has a class, race, name, level, XP, gold, and a full set of equipment slots.

---

## Classes

| Class | Role | Armor Type | Weapon Types |
|---|---|---|---|
| Warrior | Melee tank / DPS | Chain mail, Plate | Sword, TwoHandedSword, Axe, Crossbow, Bow |
| Archer | Ranged physical DPS | Hard leather, Chain | Bow, Crossbow, Dagger, Sword, TwoHandedSword |
| Magician | Elemental magic DPS | Fabric, Light | Staff, Rod, Cane, Gantha |
| Priest | Healer / Support caster | Fabric, Light | Staff, Rod, Gantha |
| Evocator | Summoner / Support | Leather, Hard leather | Wand, Cane, Gantha |

---

## Races

| Race | Stat Affinity | Best For |
|---|---|---|
| Human | Balanced — high intelligence, strength, magic resistance | Versatile; good for Archer, Priest |
| Feline | High HP and physical damage; low magic capability | Warrior, Archer (melee) |
| Fairy | High intelligence and wisdom; excellent magic damage; low HP | Magician, Priest, Evocator |

---

## Equipment Slots

| Slot | Type | Notes |
|---|---|---|
| Headwear | Armor | |
| Chest | Armor | |
| Cape | Armor | |
| Pants | Armor | |
| Boots | Armor | |
| MainHand | Weapon | Class-restricted (see class table above) |
| OffHand | Shield | Only Warrior and Priest can equip a shield |
| Ring1 | Accessory | |
| Ring2 | Accessory | |
| Necklace | Accessory | |

---

## Character Progression
- Starts at Level 1, 0 XP, 0 Gold
- XP gained from completing tasks, expenses
- Leveling formula: `XpRequired(level) = level * 100`
- Gold used in future for in-game shop (TBD)

---

## UI
- Character creation screen shown on first login (name, class, race selection)
- Character panel (left sidebar): avatar silhouette with equipment slots laid out around the body, name, class badge, race badge, level, XP bar, gold counter
- Clicking a slot opens the inventory filtered to compatible items for that slot

## Status: Backlog
