# A-008 — docs/content.md

- Owner: level-designer
- Depends on: A-006 docs/design.md (done), A-007 docs/balance.md (done), with rules inherited from A-004 docs/spec.md and A-005 docs/architecture.md
- Consumed by: A-009 docs/levels.md, and the shipping artifacts
- Status: draft

This document is the content catalog for Event Horizon: absorbable objects,
hazards (with their tool-vs-punishment decisions), quantum mechanics (particles,
phase-shift absorption modes, chain reactions), anomaly objects, upgrade item
definitions, modifiers, rewards, and the identity/lore of the special black hole
types. It answers spec §17 question 12 (which hazards are tools) and 13 (exact
quantum mechanics), and the A-007 §14 items left open for A-008 (hazard→drain
tier mapping, quantum particle/phase-shift drain interaction, anomaly object
definitions).

It does not define level placement, world/level structure, the unlock system,
secondary goals per level, endless/daily structure (A-009); UI, input, or
accessibility screens (A-010/A-011/A-012); monetization placement (A-016); or
any stat table. Every number here is A-007's and is referenced, never renumbered.
On disagreement PLAN.md wins.

Acceptance criteria coverage: **objects** (§3), **hazards** (§4), **rewards**
(§8), **modifiers** (§7), plus upgrades (§6), quantum mechanics (§5), and black
hole types (§1).

## 1. Black hole types

A-007 set the stat blocks (§2); names, lore, and non-numeric behavioral traits
are ours. The three playable types:

### 1.1 Nomad Black Hole (baseline — name owned by A-008)

The default type; every save starts with it. Identity: a scavenger's
well, a plain accretion engine carried by Wayfarer crews across the Dust Belt.
Behavioral traits: honest growth, no volatility, no quantum abilities. It is the
reference point for the tension dial — every other type shifts the dial relative
to it. Stats: A-007 §2 baseline row (pullScale 2.0 / eff 0.85 / stab 100 /
speed 26 / absorbFactor 0.30, startMass 100 × T(n) — A-007 §1.2 baseline row).

### 1.2 Star Mass Black Hole — "The Wayfarer's Lantern"

Lore: salvaged from the beacons of the old transit lanes. A stabilized accretion
engine, generations older than any quantum manifold — slow, dependable, and
patient. The Dust Belt guilds call it "the lantern that came home."

Behavioral traits (non-numeric, honoring A-007's block: pullScale 1.8 / eff
0.90 / stab 110 / speed 24 / absorbFactor 0.34):

- Stable mass growth: every absorb contributes at the highest efficiency in the
  game, so mass gain is predictable — the beginner lane of the tension dial.
- Reliable pull: the weakest pull circle (1.8) means bodies must be walked to
  the horizon by movement, not vacuumed in; precision is rewarded.
- No volatility: massGain rolls are never modified; no quantum particles, no
  phase shift, no chain reactions. Its stability pool (110) is the largest, and
  ring-fracture (design §6.1) reads on it with the most headroom before failure.
- Visual identity within design §3.2: rounded, calm silhouette; its horizon ring
  carries a steady double-edge stroke so it stays the loudest element per the
  contrast hierarchy. No new visual channel; the ring-fracture anchor is
  untouched.

### 1.3 Quantum Black Hole — "The Unbound"

Lore: a containment failure frozen mid-collapse. Something opened a manifold in
the hole and could not close it; it leaks quantum particles and answers to them.
Salvage crews log it as "the hole that talks." High-power, high-risk, the
volatile end of the tension dial.

Behavioral traits (non-numeric; A-007's block: pullScale 2.5 / eff 0.75 / stab
70 / speed 29 / absorbFactor 0.26, massGain rolls ×[0.85,1.25] seeded, stability
drains ×1.5):

- Emits quantum particles (§5.1) and accepts them back to shift absorption
  phase (§5.2) and to deepen chain reactions (§5.3).
- Volatile growth: massGain rolls ×[0.85,1.25] from the seeded run PRNG only —
  replays stay reproducible from (seed, inputDigest).
- Chain-reacts with quantum anomalies (§5.3): absorbing an anomaly detonates a
  deterministic shockwave. This is the only type for which anomalies are a
  gamble rather than a plain colossal absorb.
- Every drain costs ×1.5 (A-007 §7.1): the smallest pool (70) with the heaviest
  costs. Ring-fracture reads earlier on it; content must not fight that cue.
- Visual identity: same rounded body language, but its horizon ring edge
  shimmers with a deterministic (seeded) flicker cadence — motion, not color —
  and its particle motes are small rounded dots, never spiky (they are not
  hazards).

## 2. How content serves the tension dial

PLAN section 8 is the rule every item here must keep alive: more mass = stronger
pull, larger collision risk, slower precise movement, harder navigation,
stability may decay. The dial is: **grow fast and risk instability vs stay
smaller and move precisely.** Content expresses it as follows, with all numbers
owned by A-007:

- Body tiers (§3.1 A-007) are the growth ladder: Dust → Pebble → Chunk → Anomaly.
  Each step up trades a bigger, more strained absorb (oversized-absorb drains
  0/6/14/26, ×1.5 on Quantum) for faster growth.
- Hazards (§4) are the tax on a big ring: a larger horizon captures more hazards,
  so growing fast walks you into drains; staying small keeps routes clean but
  spends timer pressure (timers 150+5n, A-007 §8).
- Anomalies are the colossal gamble: optional, high reward, heaviest drain.
- Upgrades (§6) split the same way: pull/efficiency lanes vs stability/speed
  lanes; A-007 capped efficiency at 1.00 and no upgrade reduces drain tables.
- Modifiers (§7) re-shape the field, never the stat tables.

No item here may switch the dial off. A hazard with no decision is a rule
violation; every hazard in §4 states the decision it forces.

## 3. Absorbable object catalog

All twelve PLAN 5.7 objects, mapped onto A-007's four body tiers (§3.1, masses
× T(n), T(n) = target(n)/1000). Sub-ranges below are content picks **inside**
A-007's tier bands; the bands themselves are A-007's and are not renumbered.
Value = mass × body efficiency at absorb (A-007 §2); combo multiplier applies
per A-007 §6. Silhouettes follow design §3.2: rounded/organic, brightness below
the ring and above the background, contrast hierarchy large > medium > small.

| Object | PLAN group | Tier (A-007) | Mass sub-range × T(n) | Silhouette (shape-first) | Spawn notes |
| --- | --- | --- | --- | --- | --- |
| Dust | Small | Dust 3–10 | 3–5 | soft speck cluster, dimmest | densest filler; chains for combo |
| Ice fragment | Small | Dust 3–10 | 4–10 | rounded-corner frost wedge (corners filleted so it never reads spiky) | drifts slower; clusters in sheets |
| Pebble | Small | Pebble 15–40 | 15–24 | rounded cobble, matte | the staple; core micro loop |
| Small debris | Small | Pebble 15–40 | 20–40 | rounded hull-offcut, grommet visible | scatter; near wrecks |
| Rock | Medium | Chunk 60–150 | 60–90 | weathered sphere, cratered (craters, not spikes) | common in Asteroid Field |
| Metal shard | Medium | Chunk 60–150 | 70–110 | tumbling plate, all edges filleted | spins slowly; Ship Graveyard |
| Ship fragment | Medium | Chunk 60–150 | 80–130 | recognizable rounded fuselage section | high value density; often mine-guarded (§4.1) |
| Asteroid | Medium | Chunk 60–150 | 90–150 | heavy rounded boulder with impact dimples | forms clusters (World 2 teaching) |
| Asteroid chunk | Large | Anomaly 300–800 | 300–500 | shattered half of an asteroid, concave side inward | colossal; optional |
| Small planet | Large | Anomaly 300–800 | 450–800 | ringed planet, ring detached and rounded | largest ordinary body; Planet Ring |
| Star fragment | Large | Anomaly 300–800 | 350–650 | glowing rounded splinter, brightest body but below the ring | rare; marks bonus routes |
| Quantum anomaly | Large | Anomaly 300–800 | 300–800 | rounded core with 3 orbiting motes (organic; motes echo the particle look of §5.1) | the only hazard-adjacent body that changes behavior by type (§5.3) |

Spawn rules (all draws from the seeded run PRNG only, per A-005 and A-007 §3.2):

- Initial seeding and respawn follow A-007 §3.2: each absorb respawns one body
  at the field edge; the tier is sampled so the live tier mix stays within ±10%
  of the level recipe (the recipe itself is A-009's).
- Within a tier, the object is sampled by weight: Dust Belt recipes weight Dust
  and Ice fragment; Asteroid Field weights Asteroid and Rock; Ship Graveyard
  weights Metal shard and Ship fragment; Planet Ring weights Small planet and
  Asteroid chunk; Quantum Nebula weights Star fragment and Quantum anomaly
  (A-007 §9 counts Quantum Nebula anomalies separately in the recipe).
- Clustering is a spawn behavior, not a stat: Asteroids, Ice fragments, and
  Small debris spawn in seeded 3–7 body clusters at one anchor point; other
  objects scatter. Cluster anchors are ≥ 1.2 × eventRadius apart (deterministic
  rejection sample, bounded at 8 tries, then place-scatter).
- Quantum anomalies never spawn inside the opening meso loop's first 20 s of
  field (they would bait a level-start gamble); placement count per level is
  A-009's, plus A-007's Anomaly beacon and Quantum Core R3 spawn bonuses.
- Star fragments spawn at most 2 live at a time (seeded cap check at respawn).

## 4. Hazard catalog

Five PLAN 5.7 hazards. All drain stability only, by A-007 §7.1 size tier (small
8 / medium 14 / large 22; ×1.5 on Quantum). All follow design §3.2: sharp/spiky
silhouette **plus** a marker glyph from the bracket/X family, never color-only;
off-screen indicators follow design §3.4 (max 8; chevron = pulled body, bracketed
X = hazard whose approach vector intersects the horizon). Hazard placement and
budgets (6/10/14/18/22 live) are A-009's within A-007 §9.

**Tier mapping (answers A-007 §14 open item 3):** Mines = small (8); Gravity
anchors and Magnetic fields = medium (14); Collapsing debris and Massive
objects = large (22).

### 4.1 Mines — small tier, drain 8

- Behavior: static or slow-drifting burr. Captured by the horizon like any body
  (A-007 §5: the pull circle drags hazards in too); contact with the ring drains
  8 and the mine is consumed (detonates against the ring, spiky burst + ring
  fracture flicker per design §6.1 feedback ladder).
- Silhouette/glyph: spiky icosahedral burr + single bracket pair `[ ]` at its
  bounds. Bracket persists at all zooms; at min LOD the bracket alone remains
  (design §3.2 LOD rule).
- Decision it forces (**tool-leaning punishment**): mines are seeded on the
  shortest routes to high-value bodies (ship fragments, asteroid clusters). The
  player chooses: pay 8 to take the cluster on the greedy route, or detour and
  spend timer pressure and possibly lose the combo window (4.0 s base, A-007
  §6). A mine can also be *used*: pulling a drifting mine across a hazard-dense
  lane forces the big-ring player to re-route — the mine is a movable toll gate.
  Never free damage: mines are always placed so a clean route exists within the
  timer (A-009 verifies; A-013 tests).

### 4.2 Gravity anchors — medium tier, drain 14

- Behavior: static mass point, immune to absorption and to pull. It carries a
  local pull well (radius 0.6 × eventRadius, content constant) that curves the
  trajectories of nearby bodies and of the player's own drift. Contact with the
  anchor body itself drains 14.
- Silhouette/glyph: heavy spiked ring with a dark core + `X` glyph centered.
- Decision it forces (**tool**): the anchor is a slingshot. Line a Chunk or
  Asteroid on the well's tangent and it is bent into your horizon from an angle
  you could not reach precisely — free setup, no drain. Or keep distance and
  pay nothing but lose the setup. The same well bends *your* path: cutting near
  an anchor saves travel but risks the 14. Grow-fast players find more anchors
  inside their fat capture circle; precise players use them as corners.

### 4.3 Magnetic fields — medium tier, drain 14

- Behavior: a rectangular current (seeded axis and length from {1, 1.5, 2} ×
  eventRadius) that translates every body inside it along the axis at a
  constant drift (A-007 §5 drift physics; no new velocity numbers). The emitter
  nodes sit at the field's short ends; only emitter contact drains 14. The
  current itself is safe to ride.
- Silhouette/glyph: field drawn as a hatched band with motion streaks (rhythm,
  not color); emitters are spiky nodes with double bracket `[[ ]]` glyphs.
- Decision it forces (**tool**): the current sorts the field. Park down-axis and
  the current ferries Pebbles/Chunks to you for free (a farm lane); or cross
  the band to reach the far side, where the nearest emitter may drain 14.
  Grow-fast players risk being slid *toward* hazards at the band's exit — the
  current is a conveyor that a big ring cannot switch off. The player's decision
  is where to stand relative to the axis, every meso beat.

### 4.4 Collapsing debris — large tier, drain 22

- Behavior: a hollow shell of 6–10 spiky shards orbiting a seeded anchor on a
  12 s cycle (content constant; telegraphed 2 s before collapse by design §6.1
  feedback ladder — spiky burst channel only). At collapse the shards fold
  inward: any body caught inside the shell is shattered one tier down (determin-
  istic; which body first is seeded order, integer tier-down map Anomaly→Chunk→
  Pebble→Dust→removed). The shell's core always holds one high-value body
  (Ship fragment, Asteroid, or Star fragment) exposed **only** during the 2 s
  open window before collapse.
- Silhouette/glyph: spiky shard ring + `X` glyph with shatter ticks; collapse
  cadence is legible by motion (shards visibly close) and rhythm.
- Decision it forces (**tool — a timing puzzle, not a wall**): harvest the core
  during the open window (clean, but the window sits mid-meso and costs
  positioning), or let it collapse and collect the tier-down fragments safely
  (less mass, no drain), or get caught inside with a big body and pay 22 plus
  lose that body tiered-down. Big-ring players cannot hover the shell "for
  free": the shell's radius is under their ring floor.

### 4.5 Massive objects — large tier, drain 22

- Behavior: colossal inert bodies (visual radius ≈ 1.4 × eventRadius at level
  start; never absorbable — mass > mass-ceiling fraction rule, A-007 §3.1
  colossal band). They block the horizon: contact drains 22 once per object per
  approach (a re-approach from a new bearing re-drains; sliding along the same
  contact does not). They also block pull trajectories: bodies behind a Massive
  object sit in a deterministic shadow where the pull is suppressed.
- Silhouette/glyph: irregular spiked monolith + corner-bracket glyphs `[ ]`
  scaled to its bounds (bracket family; largest marker on screen).
- Decision it forces (**tool — the threading tax**): the shadow zone is
  information — bodies parked in a shadow do not drift into your ring, so
  precise players use Massive objects as parking walls to stage a clean
  second sweep. But the gap between two Massive objects is where the value
  clusters sit, and threading it is where a grown ring pays 22. The larger the
  horizon, the narrower the thread reads — growth literally makes the same
  route harder (PLAN 8: larger collision risk, harder navigation).

## 5. Quantum mechanics (answers spec §17 Q13 and A-007 §14 item 4)

Quantum abilities belong to the Quantum Black Hole (§1.3) only. Everything here
is integer-friendly arithmetic on the 32.32 core, draws from the seeded run PRNG
only, and runs on the DT = 1/60 tick — replays stay reproducible from
(seed, inputDigest). No drain table below is restated or altered: A-007 §7.1
remains canonical, including the Quantum ×1.5 multiplier.

### 5.1 Quantum particles

- Emission: the Quantum hole emits **1 particle every 4th absorb** (integer
  absorb counter, no PRNG needed) and **2 particles per chain reaction** (§5.3).
  Live particle cap: **8**. Emission at cap is dropped (no queue).
- Particles are small rounded motes (design §3.2 food language — they are never
  spiky; they are not hazards and carry no glyph). They orbit the horizon at a
  seeded initial heading (one PRNG draw per emission), radius fixed at 0.6 ×
  eventRadius, and persist until consumed or 20 s old (integer age counter).
- Pool cost: particles occupy up to **8 BodyPool slots** flagged non-hazard,
  non-absorbable, non-colliding. Worst case stays 140 live bodies + 8 particles
  + 24 hazards = 172 ≤ 300 (A-005 cap). No per-frame allocation.
- Consumption (deterministic): a particle is consumed when an absorb or a phase
  shift occurs; nearest-to-core first, ties broken by spawn order (seeded).

### 5.2 Phase-shift absorption modes

The Quantum hole offers three absorption phases; switching is a player action,
not RNG, and costs **1 particle**. Phases change *what* the horizon accepts,
never the numbers:

- **Solid** (default): absorbs everything, drains as normal. The grow-fast lane.
- **Sift**: accepts Dust-tier and Pebble-tier bodies only. Chunk/Anomaly bodies
  are not captured — they are deflected to tangential drift at the ring (no
  drain, no absorb). Precision lane: never triggers an oversized-absorb drain,
  but growth per meso beat is the lowest and the field clutters around the ring.
- **Phase**: accepts Chunk-tier and Anomaly-tier bodies only; small bodies pass
  through the ring untouched (no capture, no drain). High-value lane: every
  absorb is strained-or-colossal by construction (A-007 §3.1), so the drain
  tables apply at their 14/26 (×1.5) rows — the riskiest lane, the fastest lane.

Interaction with A-007 tables: oversized-absorb tiers, hazard drains, regen
(+2/s after 6 s clean), and the Quantum ×1.5 all apply unchanged inside every
phase; Sift simply never presents an oversized body, and Phase never presents
a safe one. Combo counts only accepted absorbs (A-007 §6 window rules apply).
Phase state is HUD-visible (spec 11.1; layout is A-010's).

### 5.3 Chain reactions

- Trigger: the Quantum hole absorbs a **quantum anomaly** (anomaly tier, 300–800
  × T(n), §3). The absorb pays the normal oversized drain (>2.0× row, 26, ×1.5
  on Quantum — A-007's number, not a new one). Then a shockwave fires.
- Shockwave (deterministic BFS, no recursion, visited-set): starting from the
  anomaly's position, take all bodies within **1.5 × eventRadius**; each is
  auto-absorbed contributing **mass >> 1** (integer halving, fixed-point shift)
  at the hole's current efficiency; the wave then extends once more from each
  such body (depth cap 2 total; **1** if a quantum particle is consumed at the
  anomaly absorb, §5.1). Cell iteration order is the A-005 uniform-grid cell
  order — fully deterministic, no PRNG in the wave itself.
- Chain absorbs pay **no additional drain**: the entire burst's cost is the one
  anomaly drain already paid. That is the gamble's shape: one heavy, visible
  price, then free secondary mass — and the secondary mass is exactly what a
  grown ring would have collected anyway, so the wave rewards *setting it up*
  near a thinned field.
- Chain reactions emit 2 particles (§5.1) and count once toward the "absorb
  anomalies" goal family (goal assignment per level is A-009's).
- Non-Quantum holes absorb quantum anomalies as plain colossal bodies (drain
  per A-007 tables; no wave). The anomaly's orbiting motes are cosmetic.
- Pool cost: the BFS reuses the spatial hash and the visited-set scratch buffer
  (A-005); no new pools. Worst-case wave touches ≤ 140 live bodies in one tick
  batch; A-013 budgets this in the sim test.

## 6. Upgrade item definitions

Effects, costs, ranks, and totals are A-007's (§10, §12) — referenced here, not
restated as new numbers. A-008 owns what each item *is*: identity, flavor, and
the tension note it carries. Unlock mechanics and where offers appear are A-009's.

### 6.1 Permanent meta-progression (PLAN 5.9 set; A-007 §10 table)

| Item | Identity and flavor | Lane |
| --- | --- | --- |
| Gravity Lens | A transit-lane lens that focuses the pull circle. Bigger vacuum, but the hazard-capture circle grows with it (A-007 §10 note). | grow-fast |
| Accretion Core | A refinery lattice; waste reduction only — drains unchanged (cap 1.00, A-007). | efficiency, no drain relief |
| Horizon Stabilizer | A containment collar for the hole itself: more pool headroom, same price per mistake. | stay-precise |
| Inertial Dampener | Reaction vanes; speed that cannot out-buy ring growth (A-007 §10 note). | maneuvering |
| Combo Anchor | A metronome fused to the streak counter: widens the window before mass shrinks it (A-007 §6). | chaining |
| Star Core | Unlocks §1.2 "The Wayfarer's Lantern"; ranks lean efficiency/stability. | safe lane unlock |
| Quantum Core | Unlocks §1.3 "The Unbound"; R3 grants +1 anomaly spawn/level (A-007), feeding §5.3 setups. | risk lane unlock |

### 6.2 Temporary run upgrades (A-007 §11 values kept verbatim; offers are A-009's)

- **Pull surge** — "harbor clamp": the +15% pull; enlarges the hazard-capture
  circle too (A-007 note; §4.1 mines become toll gates).
- **Wide maw** — "survey dish": +10% absorption radius; bodies absorb nearer
  the core sweep.
- **Thruster calibration** — "vanework": +10% movement speed; does not shrink
  the ring.
- **Filter matrix** — "comber grid": +10% mass efficiency, capped at 1.00 total
  (A-007 §2).
- **Anomaly beacon** — "signal flare": +1 quantum anomaly spawn; +1 colossal
  decision available (§3 spawn rules; §5.3 setups).

Stacking cap (any single stat ≤ 3× per run) is A-007's; choices stay meaningful,
not strictly additive (spec 11.1).

## 7. Modifiers

A modifier changes **field composition and spawn behavior only** — never a stat
table, drain, timer, or economy number (A-007's remain canonical). Which levels
use which modifiers, and their stacking in endless/daily, is A-009's. Each
modifier names what it changes and keeps the dial alive:

| Modifier | What it changes |
| --- | --- |
| Dense cluster | Bodies spawn in seeded clusters (3–7, §3 rule) instead of scatter; combo chains get denser, hazard lanes get tighter. |
| Drift current | The whole field drifts on one seeded axis (same drift physics as §4.3 currents, no new velocity number); routes must be planned up-axis. |
| Anomaly bloom | +2 anomaly-tier spawns beyond the recipe (spawn count only; A-007 §9 anomaly-count rule for Quantum Nebula still applies). |
| Quiet field | Hazard budget 0 for that level; the dial becomes pure oversized-absorb + timer pressure. |
| Anchor lattice | Hazard set restricted to gravity anchors (§4.2): a tool-only field that teaches slingshots. |
| Gravefall | Ship fragments and Metal shards spawn at 2× recipe weight; mines guard them (§4.1 rule). |
| Frost sheets | Ice fragments (Dust tier) spawn in sheets; a dust-heavy field that rewards Sift phase (§5.2). |
| Collapsing cascade | Collapsing debris respawn on the seeded 12 s cadence after consumption; timing pressure repeats instead of one-shot shells. |
| Monolith alley | Massive objects spawn in pairs with value clusters in the thread gap (§4.5 rule). |
| Volatile seed | Only available on Quantum runs; forces one extra anomaly spawn and no Quiet field — the risk lane stated, not taxed. |

## 8. Rewards (in-game content; placement of ads/IAP is A-016's)

Rewards give in-game things only; the economy numbers are A-007 §12 (Stellar
Cores, earned by play only — no pay-to-win, PLAN 5.14):

- **Stellar Cores** — the currency; clear/first-clear/secondary-goal/3-star
  values are A-007's. Spend on §6.1 ranks; unlock system is A-009's.
- **Star rating (1–3)** — a content state, not a number owned here: it drives
  A-007's +50% 3-star bonus. What earns 3 stars per level is A-009's.
- **Type unlocks** — Star Core / Quantum Core ranks (§6.1) unlock §1.2 and §1.3.
- **Codex entries** — completing a level for the first time unlocks that level's
  codex card: the lore line for each object/hazard first seen, the anomaly
  dossier on first chain reaction, and the type epigraph on unlock. Pure content
  reward; no stat effect, no currency.
- **Replay tokens** — clearing a level with zero hazard drains (clean run, A-007
  §7.2 language) marks a token on the codex card. Cosmetic only; a self-imposed
  precision badge that proves the stay-precise lane was played.

No reward here grants stat relief: nothing reduces drain tables (A-007 rule),
and no reward touches efficiency past the 1.00 cap.

## 9. Determinism and pool accounting (A-005 contract, honored)

- Every random event in this catalog consumes the seeded run PRNG only: respawn
  tier/object sampling, cluster anchors, mine placement seeds (§4.1), field
  axes (§4.3), shell cadence phase (§4.4), particle headings (§5.1), Quantum
  massGain rolls (A-007). Chain reactions consume **no** PRNG (§5.3 BFS is
  ordered by spatial-hash cell order).
- Content constants introduced (all integer or fixed-point friendly, none touch
  A-007 tables): particle emission every 4 absorbs; particle cap 8; particle
  age 20 s; phase-shift cost 1 particle; shockwave radius 1.5 × eventRadius;
  chain depth 2 (3 with a consumed particle); contribution halving `mass >> 1`;
  shell cycle 12 s with 2 s telegraph; shell shards 6–10; anchor well 0.6 ×
  eventRadius; field length {1, 1.5, 2} × eventRadius; cluster size 3–7;
  cluster-anchor rejection ≤ 8 tries.
- Pool worst case: 140 live bodies + 8 particles + 24 hazards = 172 ≤ 300
  (A-005 BodyPool cap). Chain BFS uses the existing spatial hash and a visited
  scratch buffer; no new pools, no per-frame allocation.
- DT = 1/60 and MAX_CATCHUP = 4 are untouched; every mechanic above resolves in
  integer steps per tick.

## 10. Open questions left downstream

For A-009 docs/levels.md:

1. Which levels use which §7 modifiers, and how modifiers stack in endless/daily
   (including whether Volatile seed may coexist with Quiet field — I forbade it
   by rule; A-009 confirms the level-level policy).
2. Per-level tier recipes (the ±10% mix A-007 §3.2 references), anomaly counts,
   hazard placements, and secondary goals ("absorb anomalies", "avoid hazards"
   families from PLAN 5.8).
3. Where mid-run upgrade offers appear (A-007 §11 said values only).
4. Whether the 20 s no-anomaly-at-field-start rule (§3) needs a per-level
   override in Quantum Nebula; default is the rule holds everywhere.

For A-013 docs/test-plan.md:

5. Determinism tests: chain-reaction BFS order stability across spatial-hash
   rebuilds; particle consumption tie-break; Quantum massGain replay from
   (seed, inputDigest).
6. Budget tests: chain wave worst-case tick cost; 172 ≤ 300 pool accounting;
   collapsing-debris tier-down map cannot spawn bodies over recipe.
7. Rule tests: every hazard has a clean route within the timer (§4.1 promise);
   no modifier combination produces a hazard-free *and* anomaly-free field that
   switches the tension dial off (Quiet field still runs oversized-absorb).
8. Balance cross-check: §5.3 chain burst total mass vs A-007 §8 targets — the
   wave must not let a Quantum run skip the strained-absorb decision.
