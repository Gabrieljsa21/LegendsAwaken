# Vision

## Game Concept

LA (Legends Awaken) is a Discord bot RPG inspired by *Pick Me Up Infinite Gacha*. The player is a **Master** who summons heroes via gacha, sends parties to climb an infinite Tower, and manages a **City** that grows as the hero collection evolves.

The core loop is: **summon → allocate → progress**

```
[SUMMON]   → Gacha generates new heroes
    ↓
[ALLOCATE] → Heroes are assigned to the Tower or a City building
    ↓
[PRODUCE]  → City generates resources/XP passively while the player is offline
    ↓
[COMBAT]   → Party climbs Tower floors, earns rewards
    ↓
[IMPROVE]  → City resources fund building upgrades and equipment
    ↓
 (back to start)
```

## Design Pillars

### 1. Meaningful Collection
Every hero has a role. 1★ heroes are not trash — they are the economy of the early game. 5★ heroes are aspirational. Ascension means a player-forged hero can rival a gacha legendary.

### 2. Idle Without Being Brainless
City production is idle; Tower is active; Missions are semi-idle. Players choose their time investment. No system demands constant presence. No system rewards AFK infinitely.

### 3. Strategic Depth Without Complexity Walls
- Party composition matters (race bonuses, archetype bonuses)
- Hero allocation creates real trade-offs (city OR tower, not both)
- Confidence/Humor systems create emergent city management

### 4. Social Without Forcing It
- Market: voluntary player-to-player trade
- Mercenaries: borrow a hero you don't have
- Training: hire a coach for your hero's off-hours
- All optional. No mechanic requires other players to function.

### 5. Fail Interesting
- Mission failed → narrative side event generated
- Hero captured → unlocks a rescue instance in the Tower
- Tower wipe → indirect hint about cause of failure on next attempt
- Crafting critical fail → unusual "broken" item with odd property (future)

## Player Fantasy

> "I built this city with my own hands. I know every hero by name — the ones I summoned, the ones I raised from nothing. When my party clears a floor that stopped me for days, it means something because I chose who was there."

The player is not a commander issuing orders to NPCs. They are a Master with a roster of characters who have personalities, relationships, and histories. Progress feels earned.

## Scope by Phase

| Phase | Focus |
|---|---|
| Fase 1 | Design complete; scope closed |
| Fase 2 | City prototype; validate the management loop |
| Fase 3A | Vertical slice; validate the full core loop |
| Fase Q | Close technical debt before expanding |
| UX-0 | Establish hybrid interaction pattern |
| Fase 3B | Economic depth; complementary loops |
| Fase 3C | AI and automation; city policy |
| Fase 3.5 | Infra and observability before beta |
| Beta | Real players; real data; balance with data not feeling |
| v1.0 | Stable, documented, deployed |

## Post-Launch Vision (No Date)

High-risk, high-impact expansions after v1.0 is proven stable:
- NPC Invasions — city faces periodic attacks from NPC factions
- Betrayal System — heroes with Confidence ≤ 0 become internal hostile agents
- Expeditions — player invades the world; retaliation generates invasion on city
- Arena Tournaments — periodic PvP events with Gold betting
- Unique Heroes / Permadeath — design decisions that radically change scope

These are design ideas. None are in scope until v1.0 data validates the base game.
