# Feature: Daily Tasks

## Goal
Recurring tasks that reset each day. Completing them rewards the character.
The user defines the habit and its difficulty — the system calculates the reward.

---

## What the User Sets
| Field | Required | Notes |
|---|---|---|
| Title | Yes | e.g. "Do 25 pushups" |
| Description | No | Optional detail |
| Difficulty | Yes | Easy / Medium / Hard / Extreme |
| Target Quantity | No | Numeric goal, e.g. `25` |
| Target Unit | No | Label for the quantity, e.g. `pushups`, `minutes`, `pages` |

The user **cannot** set XP, gold, or item drop chance — these are derived from difficulty by the system.

---

## Completion Flow
1. User taps "Complete" on a daily task
2. If the task has a `targetQuantity`, a prompt asks: *"How many did you do?"*
3. User submits (or skips the prompt for binary completion)
4. API calculates rewards (see rewards.md) and returns reward summary
5. UI shows reward animation

---

## Reset Behavior
- At midnight UTC, all daily task completions for the previous day are considered expired
- The task itself persists; only the completion record is day-scoped
- A task completed at 23:59 is done for that calendar day; the user can complete it again after midnight

---

## Behavior
- Tasks can only be completed once per calendar day (UTC)
- Soft-delete (IsActive = false) to remove a task without losing history
- Sort order is user-adjustable (drag to reorder — future enhancement)

---

## UI
- List of daily tasks, each showing: title, difficulty badge, target (if set), completion checkbox
- Completed tasks are struck-through / greyed for the rest of the day
- "Add task" button opens a form: title, description, difficulty picker, optional target quantity + unit
- Reward animation triggered on completion

## Status: Backlog
