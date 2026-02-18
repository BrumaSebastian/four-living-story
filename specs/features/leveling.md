# Feature: Leveling System

## Goal
Characters gain XP from completing tasks and level up. No level cap — progression is unlimited.
Each level costs exactly **20% more XP** than the previous one, giving a smooth and motivating curve.

---

## XP Formula

```
XpRequired(level) = floor(500 × 1.2^(level - 1))
```

Level 1 costs 500 XP. Every subsequent level costs 20% more than the one before it.

---

## Reference Table

Assuming a consistent user earns roughly **~400 XP/day**
(e.g. 3 Hard daily tasks + a few Medium personal tasks + expenses).

| Level | XP to Next Level | Cumulative XP | Est. Time at 400/day |
|---|---|---|---|
| 1 | 500 | 500 | 1 day |
| 2 | 600 | 1,100 | 3 days |
| 3 | 720 | 1,820 | 5 days |
| 4 | 864 | 2,684 | 1 week |
| 5 | 1,036 | 3,720 | 9 days |
| 6 | 1,244 | 4,964 | 12 days |
| 7 | 1,493 | 6,457 | 16 days |
| 8 | 1,791 | 8,248 | 3 weeks |
| 9 | 2,149 | 10,397 | 26 days |
| 10 | 2,579 | 12,976 | ~1 month |
| 12 | 3,715 | 19,786 | 7 weeks |
| 15 | 6,419 | 36,012 | 3 months |
| 20 | 15,974 | 77,363 | ~6.5 months |
| 30 | 98,900 | ~495,000 | ~3.5 years |

---

## Design Intent

- **Each level costs 20% more than the previous** — consistent, predictable, never feels like a wall.
- **Levels 1–10**: Reachable in about a month of daily use — fast enough to stay engaged.
- **Levels 10–20**: Several months — sustained effort required but progress is always visible.
- **Level 20+**: Long-term goals. High levels are a real achievement, not just time served.
- **No cap** — the formula scales indefinitely. Level 93 (4Story's original cap) is an endgame milestone, not a ceiling.

---

## Level-Up Behavior

- When accumulated XP meets or exceeds the threshold, level increments and overflow XP carries over
- A level-up event is returned in the reward response alongside the normal reward
- UI shows a level-up popup/animation

## Status: Backlog
