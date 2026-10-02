---
artifact: A-004
file: docs/spec.md
agent: planner
owner: planner
project: Event Horizon
platform: iOS, Android
depends_on:
  - A-001
status: doing
---

# Event Horizon — Game Specification

This is the high-level game specification for Event Horizon. It is the canonical
description of what the game is, how the player interacts with it, what the player
is trying to achieve, and how the player can lose. It does not define technical
implementation (that is A-005 `docs/architecture.md`), game feel and presentation
(that is A-006 `docs/design.md`), or numeric tuning (that is A-007 `docs/balance.md`).

Every rule in this document is derived from `PLAN.md`. If this document and
`PLAN.md` ever disagree, `PLAN.md` is the source of truth and must be updated first.

## 1. Identity

| Field | Value |
| --- | --- |
| Artifact ID | A-004 |
| File | `docs/spec.md` |
| Agent | planner |
| Depends on | A-001 `PLAN.md` (done) |
| Consumed by | A-005 `docs/architecture.md`, A-006 `docs/design.md` |
| Scope | Core loop, controls, objectives, failure conditions, player experience |

## 2. Concept

The player controls a small black hole moving through space. The black hole has a
gravitational field that attracts nearby bodies. Bodies that reach the black hole
are absorbed, increasing its mass, which in turn increases its event horizon and
its pull strength. Larger objects become absorbable only as the black hole grows.

Core fantasy, in one chain:

```text
small black hole
  -> attracts debris
  -> absorbs mass
  -> grows event horizon
  -> pulls larger objects
  -> upgrades gravity
  -> survives hazards
  -> reaches target mass
```

The game is not a power fantasy. Growth is a tradeoff, described in Section 8.

## 3. Core loop

### 3.1 The atomic loop

The core loop is the same at every scale: see something, attract it, absorb it,
grow, and face a harder version of the same decision.

```text
1. Observe    The player scans the field for bodies the current pull can move.
2. Position   The player drags the black hole to line up a target with its field.
3. Attract    The field applies force; the target drifts toward the black hole.
4. Absorb     When the target crosses the absorption radius, it is consumed.
5. Grow       Mass increases; event horizon and pull strength increase.
6. Reassess   Objects that were previously unmovable or dangerous are now
              reachable or avoidable. The loop repeats at a larger scale.
```

### 3.2 Loop timing

| Layer | Duration | What the player is doing |
| --- | --- | --- |
| Micro loop | 1-5 seconds | Attract and absorb one cluster |
| Meso loop | 20-60 seconds | Chain absorptions, build combo, clear a region |
| Macro loop | 2-4 minutes | One level: reach target mass before the limit |
| Meta loop | one session | Complete levels, spend permanent upgrades, unlock worlds |

### 3.3 Level loop

```text
Level start
  -> brief field overview (camera shows the whole play area once)
  -> play loop (Section 3.1, repeated)
  -> win: mass >= target mass before time limit
  -> lose: a failure condition in Section 9 is met
  -> result screen (mass, combo, stability, stars)
  -> reward / upgrade choice
  -> retry or next level
```

### 3.4 Session loop

```text
Campaign map
  -> select level
  -> level loop
  -> level result
  -> meta upgrade choice (permanent)
  -> unlock next level or world
  -> optional Endless / Daily / Weekly challenge
```

## 4. Controls

### 4.1 Default control

Default mobile control is **direct drag**:

- The player drags the black hole across the screen.
- The camera follows the black hole.
- There is no fire button, no aim button, and no separate absorb button.
  Absorption is purely positional: the player absorbs by moving.

### 4.2 Alternative controls

| Control | Description | Target audience |
| --- | --- | --- |
| Drag (default) | Drag the black hole directly with one finger | Everyone |
| Virtual joystick | Relative or fixed joystick moves the black hole | Players who dislike occlusion |
| Device tilt | Tilting the device steers the black hole | Accessibility / one-handed play |

Alternative controls are optional and selectable in settings. Detailed touch
handling, dead zones, and gestures belong to A-011 `docs/input.md`.

### 4.3 Control verbs

| Verb | Input | Effect |
| --- | --- | --- |
| Move | Drag / joystick / tilt | Translates the black hole in the play field |
| Wait | No input | Black hole holds position; field keeps pulling |
| Retreat | Drag away from a threat | Uses movement to manage hazard risk |
| Bait | Move near a target, then away | Uses the field to drag bodies into a safe line |

The spec deliberately has no "attract" button. The field is always on. Player
skill comes from positioning, timing, and route choice.

### 4.4 Camera contract

The camera follows the black hole and keeps the event horizon readable:

```text
camera follows black hole
zoom = f(event horizon radius, mass)
```

As the black hole grows, the camera zooms out smoothly, shows more field, keeps
the event horizon visible, and keeps the mass meter visible. Detailed camera
curves, framing, and screen shake belong to A-006 `docs/design.md`.

## 5. Player experience

The player should feel:

- **Small and curious** at level start: "what can my field pull?"
- **In control** while absorbing: every drag changes what gets pulled in.
- **Increasingly large** as the field visibly expands and reclaims the screen.
- **Under pressure** as mass grows: bigger means slower, riskier, less precise.
- **Rewarded** for chaining absorptions and for choosing routes, not for mashing.

The player must always have a readable decision:

```text
Grow fast and risk instability?
Stay smaller and move precisely?
Upgrade for efficiency or pull?
Absorb large objects or avoid hazards?
```

## 6. Black hole stats

| Stat | Meaning | Role in the spec |
| --- | --- | --- |
| Mass | Main progression stat | Win condition and growth driver |
| Event Horizon Radius | Visual gravitational boundary | Defines what the player can see as "mine" |
| Pull Strength | Gravitational attraction | Determines which bodies can be moved |
| Absorption Radius | Distance required to absorb | Defines the moment of consumption |
| Speed | Player-controlled movement speed | Decreases as mass grows (tension) |
| Stability | Prevents runaway growth or overload | Failure condition driver |
| Efficiency | How much absorbed mass becomes useful mass | Rewards clean play |
| Combo | Streak bonus for chaining absorptions | Skill expression |

Stat relationships are defined in Section 8 and tuned in A-007 `docs/balance.md`.

## 7. Starting black hole types

Two starting types express the two poles of the tension in Section 8.

### 7.1 Star Mass Black Hole

Theme: stable, reliable, slow-growth.

- Stable mass growth.
- Reliable pull.
- Better absorption efficiency.
- Lower volatility.
- Good for beginners.

```text
Mass:       stable
Pull:       medium
Efficiency: high
Stability:  high
Speed:      medium
```

### 7.2 Quantum Black Hole

Theme: unstable, high-power, high-risk.

- Stronger pull.
- Faster growth.
- More volatile.
- Can emit quantum particles.
- Can phase-shift absorption modes.
- Can chain-react with anomalies.
- Higher risk of instability.

```text
Mass:       volatile
Pull:       high
Efficiency: medium
Stability:  low
Speed:      high
```

### 7.3 Spec rule for types

Neither type may be strictly better. Star Mass must win on precision and
long-session reliability; Quantum must win on burst growth and anomaly scoring.
The exact values are set in A-007 `docs/balance.md`.

## 8. Growth and tension

### 8.1 Growth model (spec level)

Arcade physics, not real astrophysics:

```text
massGain      = bodyMass * efficiency
eventRadius   = baseRadius + a * sqrt(mass)
pullStrength  = pullScale * mass^0.75
```

Attraction:

```text
direction       = blackHole.pos - body.pos
distanceSquared = direction.lengthSquared() + epsilon
force           = pullStrength * bodyMass / distanceSquared
body.velocity  += direction.normalized() * force * deltaTime
```

Absorption:

```text
if distance(body, blackHole) < absorptionRadius:
    absorb(body)
```

The spec fixes the **shape** of these relationships (sublinear horizon,
sublinear pull, mass-proportional gain). The constants `a`, `pullScale`,
`epsilon`, base radii, and all thresholds belong to A-007 `docs/balance.md`.
The simulation structure belongs to A-005 `docs/architecture.md`.

### 8.2 Tension rule

More mass must create tension, not free power:

```text
more mass = stronger pull
more mass = larger collision risk
more mass = slower precise movement
more mass = harder navigation
more mass = stability may decay
```

Consequence for design: the player's biggest growth moments are also the player's
most dangerous moments. The spec forbids any mechanic that removes this tradeoff
outright.

### 8.3 Progression curve

Diminishing returns:

```text
mass gain is direct
event horizon grows with sqrt(mass)
pull grows with mass^0.75
speed decreases slightly as mass grows
```

Example target masses (reference points, not final tuning):

```text
Level 1:   1,000
Level 5:  10,000
Level 10: 50,000
Level 20: 250,000
Level 40: 1,000,000
```

## 9. Objectives and failure conditions

### 9.1 Primary objective

Reach the level's target mass before the time limit expires.

### 9.2 Level goal types

```text
Reach target mass.
Survive time.
Maintain combo.
Avoid hazards.
Absorb anomalies.
Use efficiency challenge.
```

A level may combine one primary goal with one secondary goal. Secondary goals
grant extra reward (stars, currency, upgrade tokens) and never replace the
primary goal.

### 9.3 Failure conditions

Possible:

- Time limit expires before target mass is reached.
- Stability reaches zero.
- Too many hazard collisions.
- Mass drops below threshold.

Recommended and canonical set:

```text
Time limit + stability + hazard pressure
```

Mapping between the three:

| Condition | How it fails | Player counterplay |
| --- | --- | --- |
| Time limit | Target mass not reached in the level timer | Choose higher-value targets, upgrade pull |
| Stability | Reaches zero | Hazard hits and oversized absorptions drain stability; Horizon Stabilizer and Inertial Dampener restore it |
| Hazard pressure | Hazard collisions drain stability and/or mass | Route planning, retreat, smaller profile |

Mass dropping below a threshold is an optional variant used only in specific
challenge levels, not in the default failure set.

### 9.4 Failure presentation

Failure must be readable in one glance:

- The failing condition is named on the result screen (time, stability, or mass).
- The result screen shows the gap to the goal (mass short, time left, stability).
- Retry is immediate and never costs currency.

### 9.5 Anti-frustration rules

- No lives system. Retry is free.
- No paywall, ad, or IAP may gate a retry or a level.
- No failure may be caused by an unavoidable hazard spawn.
- Monetization must not block core game completion.

## 10. Object categories (overview)

The spec names categories only. The full object list, values, and behaviors are
defined in A-008 `docs/content.md`.

| Category | Examples | Spec rule |
| --- | --- | --- |
| Small | Dust, pebbles, ice fragments, small debris | Absorbable at level start |
| Medium | Rocks, metal shards, ship fragments, asteroids | Require mid-level growth |
| Large | Asteroid chunks, small planets, star fragments, quantum anomalies | Require late-level growth; high value |
| Hazards | Mines, gravity anchors, magnetic fields, collapsing debris, massive objects | Punish careless growth; some are tools |

Hazards must be readable before they are dangerous. A hazard must never absorb
the player without a visible warning state.

## 11. Upgrade systems

Two systems, both required:

- **Permanent meta-progression** — persistent power across levels.
- **Temporary run upgrades** — choices made inside a level.

Permanent upgrades (named set):

```text
Gravity Lens
Accretion Core
Horizon Stabilizer
Inertial Dampener
Combo Anchor
Quantum Core
Star Core
```

Temporary run upgrades (reference values, tuned in A-007):

```text
+15% pull strength
+10% absorption radius
+10% movement speed
+10% mass efficiency
+1 quantum anomaly spawn
```

Spec rules for upgrades:

- Upgrade choices must be meaningful, not strictly additive. A player must be
  able to build pull-heavy or efficiency-heavy.
- Permanent upgrades must be reachable through play; no upgrade may require
  purchase.
- Temporary upgrades must be visible on the HUD while active.

## 12. Modes

| Mode | Purpose | Spec rule |
| --- | --- | --- |
| Campaign | Primary progression, worlds 1-5 | Must be completable without any monetization |
| Endless | One long run, escalating pressure | Must not stall; must end on a failure condition |
| Daily challenge | Shared daily goal | Same seed for all players |
| Weekly challenge | Longer shared goal | Same seed for all players |
| Leaderboards | Compare score in shared modes | Score must be reproducible from seed |

Campaign worlds (names only; content in A-009 `docs/levels.md`):

```text
World 1: Dust Belt        teaches movement, attraction, absorption
World 2: Asteroid Field   teaches larger objects and clustering
World 3: Ship Graveyard   teaches high-value targets and path planning
World 4: Planet Ring      teaches large mass and stability management
World 5: Quantum Nebula   teaches quantum mode, phase shifts, chain reactions
```

## 13. Player experience by level

| Stage | Player knowledge | New verb introduced |
| --- | --- | --- |
| World 1 | Move, attract, absorb | Bait |
| World 2 | Cluster and sequence | Retreat under pressure |
| World 3 | Value selection | Route planning around hazards |
| World 4 | Stability management | Grow-or-stay-small decision |
| World 5 | Phase and chain mechanics | Risk/reward on quantum mode |

The tutorial must teach by play, not by text walls. First level must be
completable in under 60 seconds with the default control only.

## 14. Glossary

| Term | Definition |
| --- | --- |
| Field | The region in which the black hole applies attraction |
| Event horizon | Visual boundary of the field; grows with mass |
| Absorption radius | Distance at which a body is consumed |
| Absorb | Remove a body and add its mass, scaled by efficiency |
| Body | Any object in the field that can be attracted or absorbed |
| Hazard | A body that harms the player or blocks absorption |
| Stability | Resource that fails the run when it reaches zero |
| Combo | Streak of absorptions that multiplies reward |
| Run | One attempt at one level |
| Meta | Permanent state that persists across runs |

## 15. Out of scope for this artifact

Deliberately not defined here, to avoid duplicate work:

| Topic | Owner artifact |
| --- | --- |
| Modules, simulation loop, data structures, platform choice | A-005 architecture.md |
| Visual language, particles, sound, camera curves, feedback | A-006 design.md |
| Numeric tuning, economy, upgrade values, difficulty targets | A-007 balance.md |
| Full object list, hazards, modifiers, rewards | A-008 content.md |
| World and level layout, campaign structure | A-009 levels.md |
| HUD, menus, settings, safe areas | A-010 ui.md |
| Touch handling, dead zones, gestures | A-011 input.md |
| Reduced motion, contrast, colorblind palette | A-012 accessibility.md |
| Testing strategy | A-013 test-plan.md |

## 16. Acceptance criteria

- [x] Core loop defined (Section 3).
- [x] Controls defined, with default drag and alternatives (Section 4).
- [x] Goals and objectives defined (Section 9.1, 9.2).
- [x] Failure conditions defined (Section 9.3, canonical set chosen).
- [x] Player experience defined (Section 5, Section 13).
- [x] No design decision conflicts with `PLAN.md`.

## 17. Open questions for downstream artifacts

1. A-005: how is the field simulated (per-body force vs. field texture), and what
   is the fixed timestep used for absorption checks?
2. A-005: what is the maximum simultaneous body count, and how does the spec's
   "readable field" survive it?
3. A-006: how is the event horizon drawn so that "mine" vs "not yet mine" is
   readable at a glance?
4. A-006: how is stability loss communicated without relying on color alone?
5. A-007: what is the exact stability budget per level, and how much does each
   hazard and each oversized absorption cost?
6. A-007: what is the combo window length, and does combo decay with mass?
7. A-008: which hazards are tools rather than pure punishment (gravity anchors,
   magnetic fields)?
8. A-011: does tilt control need a calibration step for accessibility?
