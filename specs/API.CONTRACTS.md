# API Contracts

Base URL (local): `https://localhost:<port>`
OpenAPI spec: `/openapi/v1.json`

---

## Conventions
- All endpoints return JSON
- Errors follow RFC 9457 Problem Details (`application/problem+json`)
- Route naming: `Get<Resource>`, `Create<Resource>`, `Complete<Resource>`, etc.
- All endpoints except `/health` require a valid Logto JWT Bearer token
- The authenticated user's `sub` claim identifies their character; no explicit user ID is passed by the client

---

## Endpoints

### Health
| Method | Path | Description |
|---|---|---|
| GET | `/health` | Health check |

---

### Character

#### GET /character
Returns the current character.

**Response 200**
```json
{
  "id": "uuid",
  "name": "string",
  "class": "Warrior | Archer | Magician | Priest | Evocator",
  "race": "Human | Feline | Fairy",
  "level": 1,
  "currentXp": 0,
  "xpToNextLevel": 100,
  "gold": 0
}
```

#### POST /character
Create the character (first-time setup).

**Request**
```json
{
  "name": "string",
  "class": "Warrior | Archer | Magician | Priest | Evocator",
  "race": "Human | Feline | Fairy"
}
```
**Response 201**
```json
{ "id": "uuid" }
```

---

### Inventory

#### GET /inventory
Returns all items the character owns.

**Response 200**
```json
[
  {
    "inventoryItemId": "uuid",
    "item": {
      "id": "uuid",
      "name": "string",
      "type": "Weapon | Helmet | Chest | Legs | Boots | Accessory",
      "rarity": "Common | Uncommon | Rare | Epic | Legendary",
      "requiredLevel": 1,
      "statBonus": { "Strength": 5 },
      "description": "string",
      "imagePath": "string"
    },
    "isEquipped": false,
    "acquiredAt": "datetime"
  }
]
```

#### POST /inventory/{inventoryItemId}/equip
Equip an item (unequips existing item in same slot).

**Response 200**
```json
{ "equippedSlot": "Weapon" }
```

#### POST /inventory/{inventoryItemId}/unequip
Unequip an item.

**Response 200** *(empty)*

---

### Daily Tasks

#### GET /daily-tasks
Returns all daily tasks with today's completion status.

**Response 200**
```json
[
  {
    "id": "uuid",
    "title": "string",
    "description": "string",
    "difficulty": "Easy | Medium | Hard | Extreme",
    "targetQuantity": 25,
    "targetUnit": "pushups",
    "baseXpReward": 120,
    "baseGoldReward": 24,
    "completedToday": false
  }
]
```
> `baseXpReward` and `baseGoldReward` are computed from difficulty and shown for display only — the client does not send them.

#### POST /daily-tasks
Create a new daily task.

**Request**
```json
{
  "title": "string",
  "description": "string",
  "difficulty": "Easy | Medium | Hard | Extreme",
  "targetQuantity": 25,
  "targetUnit": "pushups"
}
```
**Response 201**
```json
{ "id": "uuid" }
```

#### DELETE /daily-tasks/{id}
Soft-delete (sets IsActive = false).

**Response 204**

#### POST /daily-tasks/{id}/complete
Mark a daily task as complete for today. System calculates and awards XP, gold, and possible item drop.
If the task has a `targetQuantity`, `actualQuantity` may be provided to trigger the overachievement bonus.

**Request**
```json
{
  "actualQuantity": 40
}
```
> `actualQuantity` is optional. Omit for binary (done/not done) tasks.

**Response 200**
```json
{
  "baseXpAwarded": 120,
  "bonusXp": 60,
  "totalXpAwarded": 180,
  "goldAwarded": 36,
  "itemDropped": null,
  "leveledUp": false,
  "newLevel": 3
}
```

---

### Personal Tasks

#### GET /personal-tasks
Returns all personal tasks (incomplete first, then completed).

**Response 200**
```json
[
  {
    "id": "uuid",
    "title": "string",
    "description": "string",
    "difficulty": "Easy | Medium | Hard | Extreme",
    "targetQuantity": 50,
    "targetUnit": "pages",
    "baseXpReward": 60,
    "baseGoldReward": 12,
    "isCompleted": false,
    "dueDate": "2026-02-28",
    "completedAt": null,
    "createdAt": "datetime"
  }
]
```

#### POST /personal-tasks
Create a personal task.

**Request**
```json
{
  "title": "string",
  "description": "string",
  "difficulty": "Easy | Medium | Hard | Extreme",
  "targetQuantity": 50,
  "targetUnit": "pages",
  "dueDate": "2026-02-28"
}
```
**Response 201**
```json
{ "id": "uuid" }
```

#### DELETE /personal-tasks/{id}
Delete a personal task.

**Response 204**

#### POST /personal-tasks/{id}/complete
Complete a personal task. System calculates rewards from difficulty + optional overachievement.

**Request**
```json
{
  "actualQuantity": 60
}
```
> `actualQuantity` is optional.

**Response 200**
```json
{
  "baseXpAwarded": 60,
  "bonusXp": 12,
  "totalXpAwarded": 72,
  "goldAwarded": 14,
  "itemDropped": null,
  "leveledUp": false,
  "newLevel": 2
}
```

---

### Expenses

#### GET /expenses
Returns all expense entries.

**Response 200**
```json
[
  {
    "id": "uuid",
    "title": "string",
    "amount": 42.50,
    "category": "string",
    "notes": "string",
    "xpReward": 25,
    "goldReward": 5,
    "isCompleted": false,
    "dueDate": "2026-02-28",
    "completedAt": null,
    "createdAt": "datetime"
  }
]
```

#### POST /expenses
Log a new expense entry.

**Request**
```json
{
  "title": "string",
  "amount": 42.50,
  "category": "string",
  "notes": "string",
  "dueDate": "2026-02-28"
}
```
**Response 201**
```json
{ "id": "uuid" }
```

#### DELETE /expenses/{id}
Delete an expense entry.

**Response 204**

#### POST /expenses/{id}/complete
Mark expense entry as tracked/handled. Awards fixed Easy-tier rewards (25 XP, 5 gold) — no difficulty input required.

**Response 200**
```json
{
  "xpAwarded": 25,
  "goldAwarded": 5,
  "leveledUp": false,
  "newLevel": 1
}
```

---

### Inventory Enhancement

#### POST /inventory/{inventorySlotId}/enhance
Attempt to enhance an item. Consumes one `EnhancementFormula` from inventory (and a `ProtectionScroll` if `useProtection: true`).

**Request**
```json
{
  "formulaType": "Apprentice | Assistant | Master",
  "useProtection": false
}
```
**Response 200 (success)**
```json
{
  "success": true,
  "levelBefore": 4,
  "levelAfter": 5,
  "itemDestroyed": false
}
```
**Response 200 (failure with destruction)**
```json
{
  "success": false,
  "levelBefore": 7,
  "levelAfter": null,
  "itemDestroyed": true
}
```

#### POST /inventory/{inventorySlotId}/downgrade
Use a `ScrollOfReflection` from inventory to safely downgrade an item by 1 level.

**Response 200**
```json
{
  "levelBefore": 6,
  "levelAfter": 5
}
```
