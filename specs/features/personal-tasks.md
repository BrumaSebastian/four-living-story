# Feature: Personal Tasks

## Goal
One-off tasks the user defines for themselves, completed once and done.
The user defines the task and its difficulty — the system calculates the reward.

---

## What the User Sets
| Field | Required | Notes |
|---|---|---|
| Title | Yes | e.g. "Read 50 pages of a book" |
| Description | No | Optional detail |
| Difficulty | Yes | Easy / Medium / Hard / Extreme |
| Target Quantity | No | Numeric goal, e.g. `50` |
| Target Unit | No | Label, e.g. `pages`, `km`, `minutes` |
| Due Date | No | Optional deadline |

The user **cannot** set XP, gold, or item drop chance.

---

## Completion Flow
1. User taps "Complete" on a personal task
2. If the task has a `targetQuantity`, a prompt asks: *"How many did you do?"*
3. User submits (or skips for binary completion)
4. API calculates rewards (see rewards.md) and returns reward summary
5. Task is marked complete and archived
6. UI shows reward animation

---

## Behavior
- Completing a task archives it (visible in a collapsed "Completed" section)
- Deleted tasks are permanently removed
- Overdue tasks (past due date, not yet complete) are highlighted

---

## UI
- List: incomplete tasks first (sorted by due date), then a collapsed "Completed" section
- Each task shows: title, difficulty badge, target (if set), optional due date with overdue highlight
- "Add task" button opens a form: title, description, difficulty picker, optional target + unit, optional due date
- Complete button triggers reward animation

## Status: Backlog
