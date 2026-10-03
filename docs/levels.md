# A-009 — docs/levels.md

| Field | Value |
| --- | --- |
| Artifact ID | A-009 |
| File | docs/levels.md |
| Owner agent | level-designer |
| Depends on | A-008 docs/content.md (done); honors A-007 docs/balance.md (done) |
| Consumed by | A-015 store listing, A-020 ship, code work |
| Branch | feature/level-designer-levels |
| Commit prefix | `docs(levels):` |

**Scope.** Level design only: the 40-level campaign across the five PLAN 5.12
worlds, per-level objectives and field recipes, hazard and modifier placement,
tutorial order, unlock rules, endless and daily/weekly structure, and the
difficulty-curve reading. A-007 numbers (targets, timers, hazard budgets,
bands, economy, 3-star line) are honored as stated and never renumbered. A-008
objects, hazards, modifiers, and rewards are referenced by name only and never
redefined or renumbered. Out of scope: HUD/screens/input/accessibility (A-010–
A-012), tests (A-013), store and monetization placement (A-015/A-017/A-020).

**Acceptance criteria.** "Defines worlds, objectives, difficulty curve" — covered
by §2 (curve reading and unlocks), §3–§7 (five worlds, 40 levels, per-level
objectives), and §9 (recipes that realize the curve).

## 1. How to read this document

Every level row states the PLAN section 8 tension dial explicitly. The dial has
five levers, and each level names which ones are on:

```text
pull      stronger attraction with mass
collision larger collision risk with mass
speed     slower precise movement with mass
nav       harder navigation with mass
stab      stability may decay with mass
```

A level with no hazard pressure, no oversized-absorb decision, and no timer
tension would be a rule violation; none of the 40 levels is written that way.

Conventions used throughout:

- **Target mass** is A-007's 40-level table; PLAN anchors (L1 1,000 · L5 10,000
  · L10 50,000 · L20 250,000 · L40 1,000,000) are exact, intermediates are
  A-007's log-linear curve re-rounded to three significant figures, which A-007
  permitted. Where this table and A-007's §8.1 table disagree (19 intermediates,
  all within 0.22%), **A-007's table is canonical for code**; this document's
  table is for human reading. **Timer** is A-007's `150 + 5n` s and the 3-star
  line (≥35% timer remaining) are unmodified. **Hazard counts** are live hazards at any instant,
  never above A-007's world budgets (6 / 10 / 14 / 18 / 22). **Secondary goals**
  are A-007's: at most one per level, extra reward only, never a gate.
  **Determinism** is seeded-PRNG only (A-005): recipes are seeds-and-rules, and
  no level hand-places a body.

## 2. Difficulty curve reading and unlock system

### 2.1 Curve reading across the five worlds

The curve is A-007's difficulty bands; the worlds are the lens on them, not a
second curve. Each world re-frames *which* lever of the dial the player is being
asked to reason about:

| Band (A-007) | Levels | World | Headroom | What the world teaches | Dominant dial levers |
| --- | --- | --- | --- | --- | --- |
| Learning | 1–8 | Dust Belt | ~7 medium hits | movement, attraction, absorption | `speed`, `pull` |
| Combo + route choice | 9–16 | Asteroid Field | ~5 | larger objects, clustering | `collision`, `nav` |
| Efficiency-vs-pull choice | 17–24 | Ship Graveyard | ~4 | high-value targets, path planning | `pull`, `nav`, `stab` |
| Oversized management | 25–32 | Planet Ring | ~3 | large mass, stability management | `stab`, `collision`, `nav` |
| Full tension | 33–40 | Quantum Nebula | ~2 + regen | quantum mode, phase shifts, chains | all five |

Reading the campaign as one line: worlds 1–2 are *permission* worlds (the
player learns that mass is a decision, not a reward); world 3 is the *pivot*
where efficiency and pull become genuinely opposed; worlds 4–5 are the *pressure*
worlds where every lever is live at once. Mass targets grow log-linearly
(A-007) while timers grow only linearly, so the required absorb-rate roughly
doubles each world — that is the curve's actual slope, and it is why hazard
budgets grow 6→10→14→18→22 rather than scaling with mass.

### 2.2 Unlock rules

- Levels unlock **linearly**: clearing level *n* unlocks level *n+1*. No
  purchase, no timer, no ad gate (PLAN 5.14: free-to-play, no pay-to-win).
- **World entry** unlocks on clearing the previous world's level 8. Worlds are
  not skippable in campaign mode; endless and daily are separate modes.
- **Star gates are cosmetic and reward-bearing only.** A world's *codex*
  entries, replay tokens, and the Supply economy (A-007: 10,200 Supply against
  a 2,195-core full upgrade investment) are the only star-gated content. A
  player who never earns a single 3-star can still reach L40 and clear it.
- **Soft gate:** the meta-upgrade Supply line is intentionally generous (A-007's
  surplus), so a stuck player can always grind a cleared level for cores rather
  than be blocked. Replaying a cleared level re-runs the same seed and awards
  cores at the first-clear rate halved — no infinite farming of ×2 first clears.
- **No level is behind a daily/weekly reward.** Daily and weekly rewards are
  cosmetic or core-bearing, never content gates (PLAN 5.14).

## 3. World 1 — Dust Belt (levels 1–8)

Teaching beats (PLAN 5.12): movement → attraction → absorption. Hazard budget
6, mines only (A-008 small drain tier). No modifiers on L1–L6; the world is a
permission world.

| L | Target mass | Timer | Primary goal | Secondary goal | Tension dial (levers on) |
| --- | --- | --- | --- | --- | --- |
| 1 | 1,000 | 155 s | Reach target mass | — | `speed` + timer: one Chunk sits on the starting drift line and must be skipped until mass ≈400; absorbing it early costs the 3-star window |
| 2 | 1,800 | 160 s | Reach target mass | — | `pull` introduced: a Dust cluster drifts out of reach, teaching that attraction radius is the movement tool |
| 3 | 3,200 | 165 s | Reach target mass | Maintain combo | `collision` first appears: 1 mine on the direct line; combo window (A-007, 4.0 s shrinking) is generous here |
| 4 | 5,600 | 170 s | Reach target mass | — | `nav`: two mines form a gate; the wide route is shorter in time, the narrow route is safer |
| 5 | 10,000 | 175 s | Reach target mass | Absorb anomalies | PLAN anchor. `collision` + `speed`: 2 mines, and the Anomaly-tier object is oversized at start mass — absorb-now or route-around is the first real decision |
| 6 | 13,800 | 180 s | Reach target mass | Efficiency | `pull` vs `nav`: dense Dust field tempts a straight line through a mine pair |
| 7 | 19,000 | 185 s | Reach target mass | Avoid hazards | `collision` + `stab` preview: 4 mines; first level where a full-mass route loses stability |
| 8 | 26,300 | 190 s | Reach target mass | Absorb anomalies | World gate. All five levers present at low intensity: 6 mines, one oversized Chunk, timer pressure for 3-star |

**Level-1 time budget (answers spec §17 q15).** 155 s total (A-007's formula).
Design budget: ~35 s tutorial prompt + movement, ~40 s to first absorption,
~25 s to reach 1,000, leaving ~55 s of slack — comfortably inside the 3-star
line (finish within 100.75 s). L1 is deliberately winnable in ~70 s so the
3-star line teaches "efficient route" without ever teaching "you failed".

**Tutorial teaching order.** Movement is taught by the level itself (L1 has no
hazard, so any failure is a movement failure). Attraction is taught by L2's
out-of-reach cluster. Absorption of oversized objects is taught by L3 (combo)
and paid off by L5 (Anomaly tier). No tutorial text gates any level; the beats
are diegetic.

## 4. World 2 — Asteroid Field (levels 9–16)

Teaches larger objects and clustering. Hazard budget 10: mines plus gravity
anchors (A-008 medium drain tier begins). Band 9–16 headroom ~5 medium hits, so
hazard counts ramp 4→10 and every level keeps a clean route inside the timer
(A-008's mine guarantee holds here too).

| L | Target mass | Timer | Primary goal | Secondary goal | Tension dial (levers on) |
| --- | --- | --- | --- | --- | --- |
| 9 | 36,200 | 195 s | Reach target mass | Avoid hazards | `collision` + `nav`: first asteroid cluster (A-008 clusters 3–7); the gap between rocks is narrower than the player's grown radius |
| 10 | 50,000 | 200 s | Reach target mass | Absorb anomalies | PLAN anchor. `collision` + `speed` + `stab`: 5 hazards, one anchor on the value route; the Anomaly is worth a detour or nothing |
| 11 | 58,700 | 205 s | Reach target mass | Maintain combo | `pull` + `speed`: Drift current pushes the cluster off-axis, so pull must be spent against the current |
| 12 | 69,000 | 210 s | Reach target mass | Efficiency | `nav` + `collision`: metal shards (A-008 Chunk) form a cheap-but-narrow lane beside a wide safe lane |
| 13 | 81,000 | 215 s | Reach target mass | — | Dense cluster: `collision` + `stab` at full world intensity; the dense field is the oversized-absorb decision |
| 14 | 95,200 | 220 s | Reach target mass | Absorb anomalies | `pull` vs `nav`: anchor pair forces a slingshot (A-008 anchor tool); skipping it is faster but leaves the Anomaly unreachable |
| 15 | 111,800 | 225 s | Reach target mass | Avoid hazards | `collision` + `speed` + `nav`: 9 hazards, metal shards read as hazard-adjacent value |
| 16 | 131,300 | 230 s | Reach target mass | Efficiency | World gate. Quiet field: `pull` is the only lever left live, so mass growth must be paid for by route choice instead |

**Curve note.** L16's Quiet field is the world's last teaching of "stay small
and move precisely"; it is also the first level where a player can fail
without ever touching a hazard, which is the point of the dial.

## 5. World 3 — Ship Graveyard (levels 17–24)

Teaches high-value targets and path planning. Hazard budget 14: mines, gravity
anchors, magnetic fields (A-008 medium drain tier at full). Band 17–24 headroom
~4. This is the pivot world: efficiency and pull are genuinely opposed, so
magnetic fields (A-008 conveyor tool) are placed to *tempt* the greedy line.

| L | Target mass | Timer | Primary goal | Secondary goal | Tension dial (levers on) |
| --- | --- | --- | --- | --- | --- |
| 17 | 154,200 | 235 s | Reach target mass | Absorb anomalies | `nav` + `pull`: Gravefall modifier; ship fragments are high-value but sit behind a mine corridor |
| 18 | 181,100 | 240 s | Reach target mass | Efficiency | `collision` + `speed`: fragments cluster so the greedy absorb order breaks the combo window |
| 19 | 212,700 | 245 s | Reach target mass | Maintain combo | Anchor lattice: `pull` + `nav` + `stab` — slingshots cost mass-free time but spend the pull budget |
| 20 | 250,000 | 250 s | Reach target mass | Absorb anomalies | PLAN anchor. `pull` vs `stab`: one magnetic field carries the whole Anomaly value; entering it grows collision risk |
| 21 | 268,000 | 255 s | Reach target mass | Avoid hazards | `collision` + `nav`: 12 hazards, mines guarantee a clean route but only at ~70% of the mass budget |
| 22 | 287,200 | 260 s | Reach target mass | Efficiency | Drift current: `speed` + `pull` against drift; the efficient line is the drifting line |
| 23 | 307,800 | 265 s | Reach target mass | — | `nav` + `stab` + `collision`: two anchors bracket the only Anomaly; both routes are viable |
| 24 | 329,900 | 270 s | Reach target mass | Efficiency | World gate. `pull` vs `nav` at world max: 14 hazards, and the full-mass route cannot fit the last corridor |

**Secondary-goal distribution (world).** Efficiency on 18, 22, 24 — the
pivot band is where efficiency goals concentrate, because that is the decision
the band is about. Absorb-anomalies on 17, 20; avoid-hazards on 21; combo on 19.
L23 carries none, which is legal: A-007 allows zero or one secondary goal.

## 6. World 4 — Planet Ring (levels 25–32)

Teaches large mass and stability management. Hazard budget 18: gravity anchors,
magnetic fields, collapsing debris, massive objects (A-008 large drain tier).
Band 25–32 headroom ~3, so hazard counts ramp 10→18 and stability is the
expected cause of failure, not the timer.

| L | Target mass | Timer | Primary goal | Secondary goal | Tension dial (levers on) |
| --- | --- | --- | --- | --- | --- |
| 25 | 353,600 | 275 s | Reach target mass | Absorb anomalies | `stab` + `collision`: first planet fragments; the Star fragment (A-008, ≤2 live) is oversized and stability-negative to take early |
| 26 | 378,900 | 280 s | Reach target mass | Efficiency | Collapsing cascade: `nav` + `stab` — the timing puzzle (A-008 collapsing-debris tool) is the only cheap route |
| 27 | 406,100 | 285 s | Reach target mass | Avoid hazards | `collision` + `nav`: 13 hazards, massive objects force shadow threading (A-008 tax) |
| 28 | 435,300 | 290 s | Reach target mass | — | `stab` + `speed`: gravity-anchor ring; the ring is passable only at reduced mass |
| 29 | 466,500 | 295 s | Reach target mass | Maintain combo | `pull` + `stab`: Monolith alley; pull must be spent inside a corridor that punishes the grown radius |
| 30 | 500,000 | 300 s | Reach target mass | Efficiency | `collision` + `nav` + `stab`: 16 hazards, the half-way mass point of the campaign |
| 31 | 535,900 | 305 s | Reach target mass (mass-below-threshold **active**) | Absorb anomalies | `stab` + `speed`: Frost sheets; dropping mass to dodge costs the hold-threshold, so the dodge is the decision |
| 32 | 574,300 | 310 s | Reach target mass | Efficiency | World gate. Dense cluster at world max: all five levers, and the last corridor fits only a deliberately small build |

**Mass-below-threshold (answers spec §17 q14).** **Yes — on exactly two
campaign levels, L31 and L37** (see §8 for the full policy). PLAN 5.11 lists it
as an alternative failure condition; A-007 recommended time + stability +
hazard pressure as the default, and that default stays the default for the
other 38 levels. The variant is used only where a *stability* mechanic already
makes dropping mass a temptation, so it never adds pressure the player cannot
see.

## 7. World 5 — Quantum Nebula (levels 33–40)

Teaches quantum mode, phase shifts, chain reactions (A-008 particles,
Solid/Sift/Phase phases, chain reactions). Hazard budget 22: all A-008 hazard
families plus unstable cores. Band 33–40 headroom ~2 plus regen, so every level
names its regen affordance.

| L | Target mass | Timer | Primary goal | Secondary goal | Tension dial (levers on) |
| --- | --- | --- | --- | --- | --- |
| 33 | 615,500 | 315 s | Reach target mass | Absorb anomalies | Anomaly bloom: `pull` + `collision` + `stab`; Sift phase is the cheap answer, Solid phase the greedy one |
| 34 | 659,700 | 320 s | Reach target mass | Efficiency | `nav` + `collision`: phase particles gate a Phase window; missing it costs the 3-star, not the clear |
| 35 | 707,100 | 325 s | Reach target mass | Maintain combo | Drift current: `speed` + `pull`; chain reactions are the combo answer, and they cost stability |
| 36 | 757,800 | 330 s | Reach target mass | Avoid hazards | `collision` + `nav` + `stab`: 17 hazards, unstable cores read as high-value bait |
| 37 | 812,200 | 335 s | Reach target mass (mass-below-threshold **active**) | Absorb anomalies | Volatile seed: `stab` + `speed` + `pull`; the seed makes the safe route the low-mass route. Quantum run — A-008's Quantum-only restriction on Volatile seed is satisfied |
| 38 | 870,500 | 340 s | Reach target mass | Efficiency | `pull` vs `nav` at 20 hazards; Phase is the only route that keeps the timer line |
| 39 | 933,000 | 345 s | Reach target mass | — | `collision` + `speed` + `stab`: regen affordance is the one Sift window A-008's rules place mid-level |
| 40 | 1,000,000 | 350 s | Reach target mass | Absorb anomalies | PLAN anchor. Anomaly bloom + Volatile seed (the only stacked campaign pair): all five levers at max, and the 2^24 ceiling is reachable-but-punishing here. Quantum run — A-008's Quantum-only restriction on Volatile seed is satisfied |

**Anomaly-start rule.** A-008's "quantum anomalies never inside the first 20 s
of field" holds on **all 40 levels, with no per-level override** — including
Anomaly bloom on L33 and L40. Bloom adds anomalies after the window, never
inside it. This keeps one rule for code to enforce instead of a per-level
exception table, and it preserves the teaching beat that anomalies are always
an optional decision.

## 8. Failure conditions and the mass-threshold policy

Default set (A-007's recommendation, unchanged): **time limit + stability +
hazard pressure** on 38 of 40 levels. Star rating and 3-star lines are A-007's
and are not restated here.

**Mass-below-threshold: active on L31 and L37 only.**

- **Threshold definition (level-design parameter, not an A-007 number):** the
  level records the player's mass at the field's midpoint checkpoint; the
  threshold is 55% of that recorded mass, checked continuously. Dropping below
  it fails the run with a distinct message ("you lost too much mass").
- **Why only these two.** Both are the levels where A-008's tools make a
  mass-dropping dodge the *tempting* line (Frost sheets on L31, Volatile seed
  on L37). The variant converts a hidden temptation into a visible one; it is
  not extra pressure.
- **Never stacked with the timer as the only other condition.** Both levels
  keep the full default set, so a player who plays the safe route cannot fail
  by the threshold.
- **Not used in endless, daily, or weekly.** Endless has no checkpoint
  structure to compare against, and dailies must stay legible from a single
  seed. If A-013 or the shipping build wants a threshold in dailies, it is an
  explicit opt-in, not a default.
- **Tutorial levels never use it.** Levels 1–8 are the permission band; adding
  a failure the player has not been taught to expect there is a rule violation.

**Regen affordance (band 33–40, A-007's "~2 + regen").** Every level 33–40
places exactly one A-008 Sift window or stability-positive Star fragment
(A-008, ≤2 live) at roughly 60% of the timer, so the ~2-hit headroom is
recoverable once per run. This is a placement rule, not a new mechanic.

## 9. Per-level field recipes

Recipes are rules the seeded PRNG resolves, never coordinates. All mixes sit
inside A-007's ±10% live-mix band, and live bodies stay inside A-007's 110–140
budget with the BodyPool cap (300 + 24 hazards) from A-005.

### 9.1 Tier mix per world (A-008 tiers: Dust/Ice fragment, Pebble, Chunk, Anomaly)

| World | Dust+Pebble | Chunk | Anomaly | Drift across the world's 8 levels |
| --- | --- | --- | --- | --- |
| Dust Belt | 62% → 48% | 30% → 42% | 8% → 10% | Chunk share rises each level; Dust never below 40% |
| Asteroid Field | 44% → 34% | 46% → 54% | 10% → 12% | clusters bias to 5–7 (A-008 3–7) from L12 on |
| Ship Graveyard | 30% → 24% | 56% → 62% | 14% → 14% | Ship fragment (Chunk) share is the world's signature |
| Planet Ring | 22% → 18% | 58% → 62% | 20% → 20% | Anomaly-tier share is capped by the ≤2 live Star fragments |
| Quantum Nebula | 18% → 14% | 52% → 56% | 30% → 30% | Quantum anomaly (Anomaly tier) carries the remainder |

### 9.2 Anomaly-tier live counts, per level

```text
W1  L1–L8   : 1 1 1 2 2 2 2 3
W2  L9–L16  : 2 3 3 3 4 4 4 5
W3  L17–L24 : 4 4 5 5 5 6 6 6
W4  L25–L32 : 6 6 7 7 7 8 8 8
W5  L33–L40 : 9 10 11 12 12 13 14 15
```

Star fragments stay ≤2 live in every level (A-008). Quantum anomalies appear
only in W5 and only after the first 20 s of field (A-008, no override).

### 9.3 Hazard composition per level (live counts, within world budget)

```text
W1  mines 0 0 1 2 2 3 4 6
W2  mines 3 3 3 3 3 3 3 3   anchors 1 2 3 4 4 5 6 7
W3  mines 4 4 4 4 4 4 4 4   anchors 3 3 4 4 4 4 5 5   fields 1 2 2 3 4 4 4 5
W4  mines 3 3 3 3 3 3 3 3   anchors 4 4 4 4 4 4 4 4   fields 2 3 3 3 3 3 4 4
    collapsing 0 2 2 2 3 3 3 3   massive 1 0 1 2 2 3 3 4
W5  mines 4 4 4 4 4 4 4 4   anchors 4 4 4 4 4 4 4 4   fields 3 3 3 3 3 3 3 3
    collapsing 1 2 2 2 2 3 3 3   massive 0 1 3 4 5 6 7 8
```

Placement rules per family are A-008's tools (anchors = slingshot, fields =
conveyor, collapsing = timing puzzle with tier-down map, massive = shadow
threading tax, mines = guaranteed clean route inside the timer); I only state
how many of each family a level runs.

### 9.4 Absorb-count target

A-007's 25–40 absorbs per level, ramped linearly by level index: L1 25, L10 30,
L20 33, L30 36, L40 40. Cluster sizes (A-008, 3–7) are chosen so the count is
reachable without a single-body-per-absorb degenerate run.

### 9.5 Mid-run upgrade offers (answers A-008 §10 q5)

A-007 owns the values; placement is mine:

- **Run-upgrade offers are mass-gated, not timer-gated.** An offer fires the
  moment the player crosses **0.30× and 0.75× of the level's target mass** —
  exactly A-007's two feedback bands, so the offer and the feedback are the same
  event. Levels 33–40 add a third offer at **0.55×**, because that band is the
  one with ~2-hit headroom and needs a mid-level decision.
- **Three choices per offer**, drawn from A-007's run-upgrade pool by the seeded
  PRNG, always including at least one efficiency option and at least one pull
  option — the PLAN 8 "upgrade for efficiency or pull?" decision must be
  presentable at every offer.
- **Meta-progression offers appear only at world gates** (L8, L16, L24, L32) on
  the level-complete screen. No meta offer mid-level, so a run never becomes a
  shop.
- **No offer is ever paid for with currency.** Offers are chosen, not bought
  (PLAN 5.14, no pay-to-win).

## 10. Modifier placement and stacking policy

All ten modifiers are A-008's (Dense cluster, Drift current, Anomaly bloom,
Quiet field, Anchor lattice, Gravefall, Frost sheets, Collapsing cascade,
Monolith alley, Volatile seed) and change field composition only.

### 10.1 Campaign schedule

| Level | Modifier | Why here |
| --- | --- | --- |
| 7 | Drift current | first motion modifier, still inside the learning band |
| 8 | Dense cluster | world gate; density is the oversized-absorb decision |
| 11 | Drift current | pairs with the combo goal |
| 13 | Dense cluster | world's peak density |
| 16 | Quiet field | the "stay small and move precisely" test |
| 17 | Gravefall | world signature |
| 19 | Anchor lattice | slingshot budget becomes the decision |
| 22 | Drift current | efficiency line only exists under drift |
| 26 | Collapsing cascade | timing puzzle is the only cheap route |
| 29 | Monolith alley | corridor punishes the grown radius |
| 31 | Frost sheets | sets up the mass-threshold decision |
| 32 | Dense cluster | world gate at max density |
| 33 | Anomaly bloom | quantum world opens on value density |
| 35 | Drift current | chain reactions are the combo answer under drift |
| 37 | Volatile seed | sets up the mass-threshold decision |
| 40 | Anomaly bloom + Volatile seed | the only stacked pair in campaign |

Every other campaign level runs no modifier, which is deliberate: a modifier
every level would hide the base dial behind it.

### 10.2 Stacking policy

- **Campaign:** one modifier per level, except L40 (two).
- **Endless:** up to 2 stacked by depth tier 1, up to 3 by depth tier 3 (see §11).
- **Daily:** exactly 1, derived from the day seed. **Weekly:** exactly 2.
- **A-008's rule is confirmed, not overridden:** Volatile seed never coexists
  with Quiet field, at the level-policy level as well as inside A-008's own
  rule.
- **Two exclusivity pairs I add at this level** (they are placement policy, not
  content changes): Quiet field vs Dense cluster (opposite field density), and
  Collapsing cascade vs Monolith alley (both reshape the same corridor).
- **Family rule:** never two modifiers from the same family. Families for
  scheduling only: density {Dense cluster, Quiet field}, shape {Anchor lattice,
  Gravefall, Collapsing cascade, Monolith alley}, motion {Drift current, Frost
  sheets}, value {Anomaly bloom, Volatile seed}. L40's bloom+seed pair is the
  single documented exception, where the value family is the point.
- **Modifier selection is seeded**, so a daily's modifiers are recoverable from
  the day seed alone.

## 11. Endless, daily, and weekly modes

### 11.1 Endless

One continuous field, no target mass, no campaign unlocks. Score is final mass.

- **Depth tiers are the campaign curve re-cut**, using A-007's absolute
  feedback bands as the tier boundaries:

| Tier | Mass range | Hazard budget | Tier clock |
| --- | --- | --- | --- |
| 1 | start → 3,000 | 6 | 190 s |
| 2 | 3,000 → 120,000 | 10 | 230 s |
| 3 | 120,000 → 600,000 | 14 | 270 s |
| 4 | 600,000 → 2,000,000 | 18 | 310 s |
| 5 | 2,000,000 → 2^24 | 22 | 350 s |

  3,000 and 120,000 are A-007's absolute feedback bands; 600,000 and 2,000,000
  are my tier boundaries. Hazard budgets and clocks reuse the five worlds'
  budgets and L8 timers, so endless is the campaign difficulty curve re-cut
  rather than a second curve.
- **Tier crossing = the clock resets** to the next tier's clock and the field
  regenerates under that tier's budget and tier mix. Failure is A-007's default
  set only (time, stability, hazard pressure); the mass-threshold variant is
  never active here (§8).
- **Upgrade offers** fire at A-007's 0.30× and 0.75× of the tier's upper mass
  bound, so the feedback bands and the offers remain the same event.
- **2^24 mass ceiling is the terminal condition.** Reaching 2^24 ends the run
  with a "horizon reached" result and the full score; it is a cap, not a
  overflow, and it is the same cap the campaign and dailies share.
- **Modifier stacking** follows §10.2: 2 by tier 2, 3 from tier 4.
- **Determinism:** one seed per run; the leaderboard entry is reproducible from
  (seed, inputDigest) per A-005.

### 11.2 Daily and weekly challenges

- **Seed derivation:** daily seed = the UTC calendar day (date → integer);
  weekly seed = the ISO week (ISO year + ISO week number). One seed per
  calendar day, one per ISO week (A-005). UTC is stated so the boundary is
  unambiguous across stores.
- **Field template:** the seed selects one of the 40 campaign levels as the
  template (tier mix, hazard budget, hazard composition, absorb target all
  inherited), plus §10.2's modifier count (1 daily, 2 weekly). The template is
  not revealed before the attempt; the level name is shown after.
- **Goals:** primary = reach the template's target mass (A-007's target and
  timer unchanged); secondary = the template's secondary goal, worth A-007's
  +40%. The mass-threshold variant is disabled even when the template is L31 or
  L37, so a daily never hides a second failure rule behind one seed.
- **Reproducibility:** the run is reproducible from (seed, inputDigest);
  replay tokens (A-008) are the challenge's share mechanic.
- **Rewards:** Stellar Cores at A-007's clear values plus a codex entry. No
  unlock, no currency purchase, no content gate (PLAN 5.14).
- **Leaderboards** are per-seed, not global, so a player is compared against the
  same field, not a different one.

## 12. Open questions left downstream

**For A-007 follow-up (balance may re-own these numbers):**

1. Endless tier clocks reuse the five worlds' L8 timers (190/230/270/310/350 s).
   If balance prefers a dedicated endless clock table, the tier structure here
   survives and only the clock column changes.
2. My tier boundaries 600,000 and 2,000,000 are unvalidated against A-007's
   curve; 3,000 and 120,000 are A-007's and must not move.
3. The 55% mass-threshold is my level-design parameter. If balance wants it as
   a tuned constant, it should move to balance.md and be referenced here.

**Regression targets for A-013 (targets, not tests):**

4. The ±10% live-mix band (§9.1) must hold at every one of the 40 levels under
   the seeded recipes, with live bodies inside A-007's 110–140.
5. Live hazard counts must never exceed 6/10/14/18/22 in any level, at any
   instant, including under the modifiers named on that level.
6. A clean route inside the timer must exist on all 40 levels (A-008's mine
   guarantee), and the 3-star line (≥35% timer left) must be achievable on all
   40.
7. L31 and L37 must be clearable without ever touching the mass-threshold.
8. Anomaly bloom must not place an anomaly inside the first 20 s of field on
   L33 or L40.
9. Endless tier crossings must not carry a hazard budget above the tier's own
   ceiling, and the 2^24 cap must terminate rather than overflow.

**For the shipping artifacts (A-015, A-020):**

10. The store listing must state that daily/weekly boundaries are UTC (§11.2);
    I chose UTC, the disclosure is theirs.
11. Whether the store screenshot set uses the world gates (L8/L16/L24/L32/L40)
    is A-015's; I only guarantee those five levels exist and are the gates.
12. The UI must show, at minimum: current mass, target mass, timer remaining,
    stability, active modifier name, and (on L31/L37) the threshold line. Where
    and how is A-010–A-012's.
13. Whether replay tokens (A-008) are consumed per daily attempt or per run is
    unresolved here; it is a content/economy question, not a level question.
