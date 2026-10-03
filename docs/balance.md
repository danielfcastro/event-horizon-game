# A-007 — docs/balance.md

- Owner: balance
- Depends on: A-006 docs/design.md (done), with rules inherited from A-004 docs/spec.md and A-005 docs/architecture.md
- Consumed by: A-008 docs/content.md, A-009 docs/levels.md
- Status: draft

This document defines progression and difficulty for Event Horizon: mass growth,
event horizon growth, pull strength, upgrade values (permanent meta-progression and
temporary run upgrades), level target masses and timers, difficulty targets, the
in-game economy, and stability/failure tuning. It answers the open questions of spec
section 17 owned by A-007 (stability budget, combo window, default primary goal,
permanent upgrade effects and costs) and the design §10 questions owned by A-007
(zoom curve constants, tension-escalation thresholds, combo window and timers).

It does not define the content catalog (A-008), level placement, unlock system, or
endless/daily structure (A-009), UI screens (A-010/A-011/A-012), or monetization
(owned by A-016 under PLAN 5.14). Where the brief is silent, the item is marked as a
**decision of A-007**. On disagreement PLAN.md wins.

Acceptance criteria coverage: **mass growth** (§2, §3), **upgrade values** (§10, §11),
**target masses** (§8). All are defined here with concrete numbers.

Hard rule (PLAN section 8, verbatim in the brief): this game is **not** a pure power
fantasy. More mass = stronger pull, larger collision risk, slower precise movement,
harder navigation, and stability that may decay. Every number below tunes that dial;
nothing below switches it off (spec 13).

---

## 1. Black hole stats and starting types

Stats are PLAN 5.4 verbatim: Mass, Event Horizon Radius, Pull Strength, Absorption
Radius, Speed, Stability, Efficiency, Combo. Every type shares the growth formulas
of §2; types differ only in starting constants and multipliers. Type *names and
lore* belong to A-008; the stat blocks here are A-007's numbers.

### 1.1 Shared growth constants (A-007 decision)

| Constant | Value | Notes |
| --- | --- | --- |
| `baseRadius` | 8.0 | event horizon radius floor, world units |
| `a` | 2.0 | `eventRadius = baseRadius + a * sqrt(mass)` |
| `epsilon` | 1.0 | attraction denominator floor; satisfies A-005 `epsilon >= 2^-32` |
| `absorbFactor` | 0.30 | `absorptionRadius = absorbFactor * eventRadius` (baseline type) |
| `massCeiling` | 16,777,216 (2^24) | hard cap on player mass (endless safety ceiling, §13) |
| `startMass` | 100 × T(n) | T(n) = level tier multiplier, §8 |

Event horizon radius at PLAN's reference masses (sanity table):

| mass | eventRadius = 8 + 2·sqrt(mass) |
| --- | --- |
| 100 | 28.0 |
| 1,000 | 71.3 |
| 10,000 | 208.0 |
| 50,000 | 455.3 |
| 250,000 | 1,008.0 |
| 1,000,000 | 2,008.0 |

### 1.2 Starting types (PLAN 5.5 qualitative → numbers, A-007 decision)

| Stat | Baseline (name owned by A-008) | Star Mass Black Hole | Quantum Black Hole |
| --- | --- | --- | --- |
| startMass | 100 × T(n) | 120 × T(n) | 90 × T(n) |
| pullScale | 2.0 | 1.8 | 2.5 |
| efficiency | 0.85 | 0.90 | 0.75 |
| stabilityMax | 100 | 110 | 70 |
| baseSpeed | 26 | 24 | 29 |
| absorbFactor | 0.30 | 0.34 | 0.26 |
| volatility | none | none | massGain rolls ×[0.85, 1.25] (seeded); stability drains ×1.5 |

Star is stable/reliable/slow-growth (beginner default); Quantum is volatile, high
pull, fast but fragile. Volatility is a seeded PRNG roll (A-005 determinism), never
unlucky spawns: spawns themselves are never unavoidable hazards (spec 7.4).

---

## 2. Growth model — shape fixed by PLAN 5.6/9.2, constants by A-007

The formulas below are PLAN 5.6 verbatim in shape; only the constants are mine.

```text
massGain        = bodyMass * efficiency_total
efficiency_total= clamp(efficiency_base + efficiency_mods, 0, 1.00) * comboMult
eventRadius     = baseRadius + a * sqrt(mass)
pullStrength    = pullScale * mass^0.75
absorptionRadius= absorbFactor * eventRadius
speed(mass)     = baseSpeed / (1 + mass / 100000)^0.25
```

Attraction keeps PLAN 5.6 exactly (`force = pullStrength * bodyMass /
distanceSquared`, `distanceSquared = direction.lengthSquared() + epsilon`,
`epsilon = 1.0`). Absorption fires when `distance(body, blackHole) <
absorptionRadius`.

`comboMult = 1 + 0.025 * min(streak, 20)` (§6). Efficiency is capped at 1.00:
absorbed mass is never fully free, and no upgrade removes the stability cost of
oversized absorptions (§7) — the tension rule survives every upgrade path.

Diminishing returns (PLAN 5.10) are structural, not optional: `sqrt(mass)` horizon,
`mass^0.75` pull, and the speed decay formula above mean each doubling of mass buys
disproportionately less control. The player trades control for size.

## 3. Mass growth pacing

Pacing targets (design §8, confirmed): micro loop 1–5 s per absorb decision, meso
loop 20–60 s per chain/region. A level should take roughly 25–40 absorbs of play
plus maneuvering, landing inside its timer (§8).

### 3.1 Body mass tiers (A-007 values; A-008 names the objects)

Body masses scale with the level tier multiplier `T(n) = target(n) / 1000` (§8):

| Tier | Base mass range | × T(n) | Role |
| --- | --- | --- | --- |
| Dust | 3–10 | fine filler | fast chain food |
| Pebble | 15–40 | staple | core micro loop |
| Chunk | 60–150 | strained | costs stability at low player mass |
| Anomaly | 300–800 | colossal | high reward, high drain, optional |

At level start (player mass 100·T) a Pebble is ~0.2–0.4× player mass (safe), a
Chunk is 0.6–1.5× (strained/oversized), an Anomaly is 3–8× (colossal). The player
must choose whether to pay the drain — growth is a decision, not a default.

### 3.2 Field density and respawn (A-007 decision)

- Live bodies on field: 110–140 (inside A-005 BodyPool cap 300 + 24 hazards).
- Each absorb respawns one body from the seeded PRNG at the field edge, tier
  sampled so the tier mix stays within ±10% of the level's recipe (A-009 sets
  recipes; the density budget above is mine).
- Micro loop check: one absorb ≈ 2–4 s of travel + attract; 25–40 absorbs ≈ 50–160
  s of core play, inside the meso loop and the §8 timers.

---

## 4. Event horizon growth, zoom curve, feedback bands

### 4.1 Zoom curve (answers design §10 question 5; keeps design §2 ring guarantee)

The camera shows a half-width that always contains the full ring with margin:

```text
W(mass) = 1.30 * eventRadius(mass) + 48        (visible half-width, world units)
zoom    = defaultHalfWidth / W(mass)           (monotonic zoom-out, design §2)
```

Ring guarantee holds by construction: ring radius = W/1.30 minus a widening
absolute margin (48 units), so the ring never touches the viewport edge. Punch-in
only on large absorbs (design §2); it never shrinks the visible half-width below
`1.15 * eventRadius`.

Sanity: at mass 100, W = 84.4; at 1,000, W = 140.7; at 10,000, W = 318.4; at
1,000,000, W = 2,658.4. Zoom is recomputed on absorb, not per frame (§13).

### 4.2 Tension-escalation bands (design §6.2 thresholds, A-007 decision)

Bands are relative to the level target so they read correctly at every tier:

| Band | Campaign condition | Endless absolute fallback |
| --- | --- | --- |
| low (thin calm ring) | mass < 0.30 × target(n) | mass < 3,000 |
| mid (ring brightens) | 0.30–0.75 × target(n) | 3,000–120,000 |
| high (ring pulses, field darkens) | > 0.75 × target(n) | > 120,000 |

Bands imply nothing about combo streak length (design §6.2 constraint). The high
band is where the level's tension peak lives: the largest absorbs happen while
speed is lowest and drains are heaviest (spec 8.2).

## 5. Pull strength and speed decay

`pullStrength = pullScale * mass^0.75` (PLAN shape). Reference values, baseline
type (pullScale 2.0):

| mass | pullStrength |
| --- | --- |
| 100 | 63 |
| 1,000 | 356 |
| 10,000 | 2,000 |
| 1,000,000 | 63,245,553 |

Force samples (PLAN 5.6 attraction, `epsilon = 1.0`): at level 1 a 40-mass pebble
touching the ring (d=28) drifts at ≈ 3.2 u/s; at half-ring (d=14) it is yanked at
≈ 12.9 u/s. At 10,000 mass on a tier-10 level (pebble 400, ring 208) the ring-edge
drift is ≈ 18.5 u/s against a player speed of 25.4 — the whole field is violent,
and navigation becomes route choice, not weaving.

More mass reaches farther (bigger ring, stronger pull at any fixed distance) and
drags more of the field — including hazards. That is the tension: a fat pull
radius is also a fat hazard-capture radius. Gravity Lens and the +15% pull run
upgrade enlarge that danger circle; nothing shrinks it for free.

Speed decay (PLAN 5.10 "speed decreases slightly as mass grows"):

```text
speed(mass) = baseSpeed / (1 + mass / 100000)^0.25
```

Baseline (baseSpeed 26): 26.0 at start; 21.9 at 100k; 19.0 at 250k; 14.3 at 1M;
7.2 at the 2^24 ceiling. Decay is computed as two nested integer square roots
(§13). Inertial Dampener (§10) offsets decay but never restores full early-game
maneuverability at high mass: max +18% speed cannot beat the ring growth and
drain costs that come with it.

---

## 6. Combo system (answers open questions 2 and 6)

- **Combo window: 4.0 s base** (A-007 decision — inside the 1–5 s micro loop:
  chaining means the next absorb lands within one micro loop).
- **Combo decays with mass** — the window shrinks, the bonus does not:

```text
comboWindow(mass) = max(1.0, 4.0 / (1 + mass / 200000)^0.5)   seconds
```

  4.0 s at start; 2.8 s at 200k; 1.6 s at 1M; floored at 1.0 s above ~6M mass
  (the floor keeps chaining possible at the ceiling — decisions, not punishment).
  Bigger
  holes chain harder — the same tension rule, expressed on the combo stat.
- Streak bonus: `comboMult = 1 + 0.025 * min(streak, 20)` → +2.5% mass per
  chained absorb, capped at +50% at streak 20. Cap keeps combo a modifier, not a
  stack that erases the timer.
- A missed window resets streak to 0 with no other penalty (failure creates
  decisions, not punishment — spec 7.4).
- Combo Anchor (§10) adds +0.5 s per rank to the window before the mass divisor,
  so it shifts the dial but never restores the base 4.0 s at high mass.

## 7. Stability and failure tuning (answers open question 1)

Stability is the decision resource (spec 7.4): every aggressive act spends it;
precise play conserves it. Failure fires the moment stability reaches 0 or the
timer expires (canonical set PLAN 5.11 / spec 7.1: time + stability + hazard
pressure). No single hit can kill a healthy run: the worst single drain is 26 of
a 100-point pool.

### 7.1 Budgets and drains (A-007 decision)

| Event | Drain |
| --- | --- |
| Stability pool | 100 (baseline), 110 (Star), 70 (Quantum) |
| Hazard contact, small tier | 8 |
| Hazard contact, medium tier | 14 |
| Hazard contact, large tier | 22 |
| Oversized absorb, body ≤ 0.5× player mass | 0 |
| Oversized absorb, 0.5×–1.0× | 6 |
| Oversized absorb, 1.0×–2.0× | 14 |
| Oversized absorb, > 2.0× | 26 |
| Quantum type multiplier on all drains | ×1.5 |
| Passive regen | +2.0/s after 6 s with no drain event, capped at pool max |

Hazard damage is assigned by size tier; **which hazards exist and their names are
A-008's**, mapping onto these tiers. Hazards drain stability only; mass loss from
hazards exists only as an A-009 challenge modifier (spec 7.3 "and/or mass"
resolved: stability). Oversized-absorption tiers use the §3.1 body tiers, so at
level start a Chunk is strained and an Anomaly is colossal by construction.

### 7.2 Headroom targets (difficulty dial, not luck)

A clean run never spends stability. A tier-1 level with its hazard budget (§9)
offers ~7 medium-equivalent hits of headroom on a 100-point pool; the high band of a level is
where drains concentrate (larger ring captures more hazards). Retry is immediate
and free — no lives, no currency gate (spec 7.4).

---

## 8. Level target masses and timers (answers open question 3)

**Decision: yes — "reach target mass before the timer expires" is the default
primary goal** for campaign levels (resolves spec 6.1 / spec 17 question 10).
Secondary goals follow spec 6.2 (one max, extra reward only, A-009 assigns them).

### 8.1 Target mass curve — piecewise log-linear through PLAN 5.10 anchors

Anchors (PLAN fixed): L1 1,000 · L5 10,000 · L10 50,000 · L20 250,000 · L40 1,000,000.
Between anchors, `target(n)` interpolates geometrically, rounded to 2–3
significant figures. Tier multiplier `T(n) = target(n) / 1000` drives §3.1 body
masses and §1.1 startMass.

| Levels | Target masses (worlds per A-009; shown here in level order) |
| --- | --- |
| 1–5 | 1,000 · 1,800 · 3,200 · 5,600 · 10,000 |
| 6–10 | 13,800 · 19,000 · 26,300 · 36,200 · 50,000 |
| 11–15 | 58,700 · 69,000 · 81,000 · 95,200 · 111,800 |
| 16–20 | 131,300 · 154,200 · 181,200 · 212,800 · 250,000 |
| 21–25 | 268,000 · 287,000 · 308,000 · 330,000 · 354,000 |
| 26–30 | 379,000 · 406,000 · 435,000 · 466,000 · 500,000 |
| 31–35 | 536,000 · 574,000 · 615,000 · 659,000 · 706,000 |
| 36–40 | 757,000 · 811,000 · 869,000 · 931,000 · 1,000,000 |

A-009 may re-order or re-round for placement; the anchors are fixed by PLAN and
must survive.

### 8.2 Timers (A-007 decision)

```text
timer(n) = 150 + 5 * n   seconds
```

L1 155 s · L5 175 s · L10 200 s · L20 250 s · L40 350 s. Competent play reaches
target in ~60–75% of the timer (40% headroom for a new player at L1, tapering to
~25% by L40 as hazard pressure grows — §9). Expert line: finish with ≥ 35% timer
remaining (3-star, §12). Because body masses scale with T(n), absorb counts stay
~25–40 per level (§3.2), so timers hold across the campaign.

### 8.3 Difficulty targets (per-level expectations, A-007 decision)

| Level band | Expected player | Hazard pressure | Stability headroom |
| --- | --- | --- | --- |
| 1–8 | learning; one decision at a time | low | ~10 small / 7 medium hits |
| 9–16 | combo + route choice | medium | ~5 medium hits |
| 17–24 | efficiency vs pull build choice | high | ~4 medium hits |
| 25–32 | oversized-absorption management | high | ~3 medium hits |
| 33–40 | full tension: big ring, fast field, thin pool | severe | ~2 medium + regen |

A-009 applies the campaign curve and unlock system on top of these bands.

---

## 9. Hazard budgets per world (difficulty targets A-009 applies)

Live hazard count per level, by world (world names from spec 11.2; hazard
*definitions* are A-008's). Inside the A-005 pool cap of 24 hazards:

| World | Levels | Live hazards | Note |
| --- | --- | --- | --- |
| Dust Belt | 1–8 | 6 | teaching; mostly small tier |
| Asteroid Field | 9–16 | 10 | medium tier introduced |
| Ship Graveyard | 17–24 | 14 | large tier introduced |
| Planet Ring | 25–32 | 18 | dense mixed tiers |
| Quantum Nebula | 33–40 | 22 | quantum anomalies counted separately (§3.2 recipe) |

No hazard spawn may be unavoidable (spec 7.4): every spawn point keeps a
clearance of `1.6 × absorptionRadius` from any player-occupied lane at spawn time;
the seeded PRNG re-rolls placement until valid (deterministic).

## 10. Permanent meta-progression (answers open question 4)

Seven upgrades, PLAN 5.9 names; **item definitions are A-008's, effects and costs
here are A-007's.** Three ranks each; ranks are bought with Stellar Cores (§12)
through play only — no purchase required (PLAN 5.14). Every effect shifts the
"grow fast vs stay precise" dial; none removes collision risk or oversized-drain
cost (spec 13).

| Upgrade | Rank effects (cumulative) | Cost per rank | Tension note |
| --- | --- | --- | --- |
| Gravity Lens | +8% pull strength | 40 / 90 / 160 | bigger pull circle also drags hazards in |
| Accretion Core | +4% efficiency (cap 1.00) | 40 / 90 / 160 | waste reduction ≠ drain reduction |
| Horizon Stabilizer | +10 stability pool | 50 / 110 / 190 | pool only; drains unchanged |
| Inertial Dampener | +6% movement speed | 40 / 90 / 160 | cannot out-buy ring growth |
| Combo Anchor | +0.5 s combo window | 35 / 80 / 140 | applied before mass shrinkage |
| Star Core | unlock Star type; R2 +0.02 efficiency; R3 +5 stability | 120 / 240 | safe lane |
| Quantum Core | unlock Quantum type; R2 +5% pull; R3 +1 anomaly spawn/level | 120 / 240 | risk lane |

Total investment: 2,195 cores. Quantum Core R3 raises anomaly spawn count
(§3.1 anomaly tier: 300–800 × T) — more colossal absorbs means more 26-drain
decisions: the risk lane pays more *and* hurts more.

Horizon Stabilizer touches stability only through pool size and regen headroom;
damage tables (§7.1) are never reduced by any permanent upgrade — this is how the
tension rule survives meta-progression.

---

## 11. Temporary run upgrades (values; PLAN 5.9 reference set kept)

Offered mid-run (when and how they are offered is A-009's; active upgrades are
HUD-visible per spec 11.1). Values are canonical A-007 numbers:

| Upgrade | Value | Tension note |
| --- | --- | --- |
| Pull surge | +15% pull strength | enlarges the hazard-capture circle too |
| Wide maw | +10% absorption radius | bodies absorb nearer the core sweep |
| Thruster calibration | +10% movement speed | does not shrink the ring |
| Filter matrix | +10% mass efficiency | capped at 1.00 total (§2) |
| Anomaly beacon | +1 quantum anomaly spawn | +1 colossal decision available |

Choices must be meaningful, not strictly additive (spec 11.1): pull-heavy builds
(2× pull surge + wide maw) farm the field violently; efficiency-heavy builds
(2× filter matrix) grow slower but keep the pool. Stacking cap: any single stat
may be taken at most 3× per run.

## 12. Economy — Stellar Cores (A-007 owns in-game economy; A-016 owns monetization)

Currency: **Stellar Cores**, earned by play only. No second currency affects
balance; IAP is cosmetic and ad-removal touches nothing here (PLAN 5.14, no
pay-to-win; every permanent upgrade reachable through play).

| Source | Value |
| --- | --- |
| Level clear | 25 + 5·n cores (L1 30 → L40 225) |
| First clear | ×2 |
| Secondary goal met (spec 6.2) | +40% of the level's clear value |
| 3-star (finish with ≥35% timer left) | +50% |
| Daily/weekly challenge | 2× a normal clear of the equivalent tier (structure A-009's) |

Supply check: campaign first-clears alone yield Σ(25+5n)×2 = 10,200 cores over
40 levels — 4.6× the 2,195-core full upgrade investment, so a focused player
completes meta-progression by ~level 15–20 without ever touching side content.
Replay farming is not required and is deliberately weak (repeat clears earn base
value only). Retry costs nothing (spec 7.4).

---

## 13. Fixed-point and determinism notes (A-005 constraint honored)

Every constant above is representable in 32.32 fixed-point:

- Masses are integers: player mass ≤ 2^24 (16,777,216); body mass ≤ 800,000 < 2^20.
- `a = 2.0`, `baseRadius = 8.0`, `absorbFactor`, pullScale ∈ {1.8, 2.0, 2.5}: exact
  or repeating-binary fractions handled as 32.32 values; `epsilon = 1.0 ≥ 2^-32`.
- `pullStrength` max = 2.5 × (2^24)^0.75 = 655,360 — comfortably inside int range.
- `mass^0.75 = sqrt(mass * sqrt(mass))` via two integer square roots; speed decay
  `(1 + m/S)^0.25` via two square roots and one fixed-point division.
- `pullStrength`, `eventRadius`, `absorptionRadius`, and `speed` are recomputed on
  absorb (and on rank change), never per frame; attraction runs the PLAN 5.6 form
  with division-first ordering (`pullStrength / distanceSquared` then × bodyMass)
  to keep intermediates in range — arithmetic owned by A-005's sim core.
- All rolls (Quantum volatility, respawn tier sampling, spawn validity re-roll)
  consume the seeded run PRNG only; leaderboard replays stay reproducible from
  (seed, inputDigest).

## 14. Open questions — answered here / left downstream

Answered by this artifact (spec 17, owner A-007):

1. Stability budget: pool 100/110/70 by type; hazard drains 8/14/22 by size tier;
   oversized-absorb drains 0/6/14/26 by mass ratio; regen +2/s after 6 s clean.
2. Combo window 4.0 s base, shrinking with mass (`max(1.0, 4.0/(1+m/200000)^0.5)`);
   streak bonus +2.5%/chain capped at +50% at streak 20.
3. Default primary goal: target mass + timer, yes; `timer(n) = 150 + 5n` seconds.
4. Permanent upgrade effects and costs: §10 table; Horizon Stabilizer touches
   stability via pool size only, never via drain reduction — tension preserved.

From design §10: zoom curve `W = 1.30·eventRadius + 48` with ring guarantee;
feedback band thresholds 0.30/0.75 of target (endless: 3,000 / 120,000); combo
window and timers as above.

Left open for downstream artifacts:

- **A-008**: which hazards map to the small/medium/large drain tiers; quantum
  particle and phase-shift mechanics and their drain interactions; anomaly object
  definitions behind the anomaly tier masses (§3.1).
- **A-009**: which levels use secondary goals and their modifiers (including any
  "mass drops below threshold" challenge levels); world/level placement and
  unlock system; endless, daily, weekly structure on top of §8/§9 curves.
- **A-013**: sim-based verification that timers, headroom, and absorb counts land
  (regression targets: 25–40 absorbs/level, ≤26 worst single drain).

Status: draft — acceptance criteria (mass growth, upgrade values, target masses)
all defined above.
