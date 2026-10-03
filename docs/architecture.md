# A-005 — docs/architecture.md

Technical architecture for Event Horizon.

- Owner: architect
- Depends on: A-004 docs/spec.md (done)
- Consumed by: A-013 docs/test-plan.md, A-019 docs/prototype-scaffold.md, all code work
- Status: draft

This document defines modules, the simulation model and loop, data structures, rendering,
the performance plan, and the platform choice. It answers the three open questions of
spec section 17 owned by A-005. It does not decide game feel (A-006), balance numbers
(A-007), content definitions (A-008), level structure (A-009), UI (A-010), testing
(A-013), or the first code scaffold (A-019).

---

## 1. Platform and tech stack

| Decision | Choice | Rationale |
| --- | --- | --- |
| Engine | Unity 2D + C# + URP | PLAN 5.15 names this as "best overall"; Unity Ads / IAP and the Unity Input System are first-party, which the mobile store plan depends on. |
| Primary target | Android mobile phone | The QA plan (PLAN 5.17) is phone-centric; Unity Ads / IAP (PLAN 5.15) is the mobile store path. |
| Secondary target | iOS, same project | Reuse of the same scene, code, and assets; enabled by a build-target switch, not a fork. |
| Dev/QA target | Desktop standalone | Same simulation core; used by A-013 for deterministic replay tests. |
| Godot 4 alternative | Not chosen | PLAN 5.15 lists it as a lightweight alternative; rejected because Unity's Ads/IAP/Input tooling removes third-party shims. |

Project layering (Unity):

```text
Assets/
  Runtime/        simulation core (pure C#, no Unity API calls)
  Bridge/         Unity MonoBehaviour layer: render, input, UI, platform
  Data/           data-driven tables (level spawn tables, object tables)
```

Decision: the simulation core has zero Unity API dependencies. Rationale: it can be
re-simulated headless on desktop for leaderboard verification and deterministic testing.

## 2. Module architecture

One attractor (the player black hole) plus static hazard attractors interact with a flat
pool of bodies. There is no physics engine involvement; the gravity model is the custom
per-body integration of PLAN 5.6.

```text
SimCore (deterministic, headless-capable)
 ├─ FixedStepDriver      accumulator, fixed DT, catch-up cap
 ├─ PRNG                 seeded, deterministic (splitmix64 -> xoshiro256+)
 ├─ SpatialHash          uniform grid, rebuilt cheaply each step
 ├─ PlayerController     intent -> velocity using Speed stat (PLAN 5.4)
 ├─ AttractionSystem     per-body force from attractor list (PLAN 5.6 formulas)
 ├─ AbsorptionSystem     absorption-radius checks per step (PLAN 5.6)
 ├─ HazardSystem         attractor + damage hooks; mechanics defined by A-008
 ├─ GrowthSystem         massGain, eventRadius, pullStrength (PLAN 5.6)
 ├─ StabilitySystem      state slots; values owned by A-007
 ├─ ComboSystem          streak counters; window owned by A-007
 ├─ ScoringSystem        score accumulation slots; formula owned by A-007
 ├─ SpawnDirector        consumes level spawn tables; content owned by A-008/A-009
 ├─ BodyPool             SoA arrays + free list, fixed capacity
 └─ InputDigest          per-step player intent log for reproducibility

Non-simulation modules
 ├─ LevelLoader          reads data tables into SpawnDirector + static layout
 ├─ RenderLayer          interpolation, LOD, culling, particles, horizon ring
 ├─ InputAdapter         Unity Input System -> per-step intent (mapping: A-011)
 ├─ UIRoot               HUD and menus (A-010)
 ├─ MetaLayer            save, leaderboard entry packaging (seed + score + digest)
 └─ PlatformBridge       Unity Ads / IAP, analytics, crash reporting (A-015..A-017)
```

Data flow per step:

```text
InputAdapter -> intent
FixedStepDriver -> SimCore.step(intent)
  PlayerController -> attractor list refresh -> SpatialHash
  -> AttractionSystem -> AbsorptionSystem -> GrowthSystem
  -> Stability/Combo/Scoring -> SpawnDirector (spawns) -> BodyPool
RenderLayer.read(SimCore state, alpha)
MetaLayer <- ScoringSystem / leaderboard entries
```

Module contracts for downstream code:

- Every SimCore subsystem is a pure function of `(state, stepIndex, intent)`; no wall
  clock, no Unity API, no unordered-iteration reads.
- HazardSystem exposes two hooks only: `applyInfluence(body, hazard)` and
  `onContact(body, hazard)`. Which hazards are tools vs punishment is A-008.
- StabilitySystem, ComboSystem, ScoringSystem expose slots, not values. A-007 fills
  them. The tension rule of PLAN section 8 constrains those values, not the slots.

## 3. Simulation model and loop

### 3.1 Field simulation method (open question 1)

**Answer: per-body force integration, not a field texture.**

Rationale: there is one moving attractor (the player) plus a small list of static
hazard attractors, and force depends on each body's mass (PLAN 5.6:
`force = pullStrength * bodyMass / distanceSquared`). A field texture would have to be
re-baked every step as the black hole moves and grows, and cannot express per-body
mass weighting. A field texture may still be used as a purely visual layer — that is
A-006's decision, and the architecture keeps it optional and decoupled from the sim.

Attraction is exactly the PLAN 5.6 model, applied per body against the attractor list
in fixed attractor-index order (deterministic accumulation order):

```text
direction = blackHole.pos - body.pos
distanceSquared = direction.lengthSquared() + epsilon
force = pullStrength * bodyMass / distanceSquared
body.velocity += direction.normalized() * force * dt
```

Absorption is exactly PLAN 5.6: `if distance(body, blackHole) < absorptionRadius:
absorb(body)`, checked once per simulation step.

Decision: bodies do not collide with each other. Rationale: PLAN 5.6 defines only
body-vs-black-hole and hazard interactions; body-body collision adds cost and breaks
readability without adding a decision the player can act on.

### 3.2 Fixed timestep (open question 1, value)

**Decision: DT = 1/60 s (one simulation step per 60 fps rendered frame).**

Rationale: matches the 60 fps target of PLAN 5.16; absorption checks run every step, so
60 Hz is finer than any absorption window a player could perceive, and the 30 fps
fallback (PLAN 5.16) renders every second step while the sim stays at 60 Hz.

Loop (frame-driven, sim-deterministic):

```text
accumulator += frameDelta
steps = 0
while accumulator >= DT and steps < MAX_CATCHUP:
    sim.step(DT)          # absorption + all checks happen here
    accumulator -= DT
    steps += 1
alpha = accumulator / DT  # render interpolation factor, render-only
```

Decision: `MAX_CATCHUP = 4` steps per frame. Rationale: bounds worst-case frame cost
and prevents spiral-of-death; dropped catch-up steps are discarded (no time credit), so
a lagging device slows the game clock identically on replay only if the digest records
step indices — see 3.4.

Decision: quality tiers change render only (LOD distance, particle budget, resolution
scale), never DT. Rationale: keeps simulation results device-independent, which
leaderboard reproducibility (3.4) requires.

### 3.3 Determinism rules (required by 3.4 and by A-013)

1. **Fixed-point arithmetic in the sim core.** Positions, velocities, mass, force:
   signed 64-bit, 32.32 fixed-point. `sqrt` for `eventRadius = baseRadius + a*sqrt(mass)`
   uses integer `isqrt` on the fixed-point mass. Rationale: cross-platform float
   determinism is not guaranteed; 32.32 gives ~4e-10 precision, far finer than the
   world scale, and mul/div fit in `int128`-style helpers (two 64-bit muldiv routines,
   implemented in A-019).
2. **Seeded PRNG.** splitmix64 to seed xoshiro256+; one stream per level
   (`seed = hash(levelId, sessionSeed)`). All spawn jitter, drift, and hazard timing
   draw from it.
3. **Input is quantized to steps.** The player's intent per step is a fixed-point
   velocity delta sampled once per step, not per render frame. The InputDigest is the
   array of per-step intents.
4. **Iteration order is index order.** BodyPool arrays and the attractor list are
   iterated by index; the SpatialHash is used only for queries that return index sets
   processed in ascending index order.

### 3.4 Leaderboard reproducibility (open question 3)

**Answer: yes — endless-mode scoring is reproducible from a seed.**

Design: an endless-mode run is fully identified by
`(sessionSeed, inputDigest)`. The sim core is deterministic per 3.3, so a headless
re-simulation of the same seed + digest yields the same score on any device.

- A leaderboard entry stores: `seed`, `score`, `inputDigest` (run-length encoded
  per-step intents; ~10 bytes/minute expected), and a run hash (FNV-1a over final
  sim state, cheap integrity check).
- Verification policy (re-sim on submission vs. accept-with-heuristics) is a
  release/QA decision — flagged to A-013 and A-015/A-016, not decided here.
- Campaign levels use fixed authored spawn tables (A-009), so their seeds are stable
  by construction.

## 4. Data structures

All sim structures are preallocated at boot; zero allocation in the hot loop (PLAN 5.16
"low memory", "stable memory").

### 4.1 Body pool — struct-of-arrays, fixed capacity

```text
capacity N = 300                      # see 5.1
active:      u8[N]                    # 0/1
category:    u8[N]                    # 0 small, 1 medium, 2 large (PLAN 5.7 classes)
typeId:      u8[N]                    # index into object table (A-008)
posX, posY:  i64[N]                   # 32.32 fixed-point
velX, velY:  i64[N]                   # 32.32 fixed-point
mass:        i64[N]                   # 32.32 fixed-point
radius:      i64[N]                   # visual/collision radius, fixed-point
flags:       u8[N]                    # bit0 absorbing, bit1 drifting, bit2 hazard-tag
freeList:    i32 stack                # recycle indices (object pooling, PLAN 5.16)
```

SoA chosen over AoS: the attraction and absorption loops stream one array per pass,
which is the layout the 60 Hz budget needs on mobile CPUs.

### 4.2 Black hole state (PLAN 5.4 stats)

```text
mass, eventRadius, pullStrength, absorptionRadius,   # all 32.32 fixed-point
speed, stability, efficiency, combo                  # slots; values A-007
pos, vel                                             # 32.32 fixed-point
```

`eventRadius` and `pullStrength` are recomputed per step from PLAN 5.6 formulas;
`a`, `pullScale`, `baseRadius`, `epsilon` are data constants supplied by A-007
(`epsilon` floor: 1 fixed-point unit = 2^-32).

### 4.3 Attractor list

```text
attractors: array <= 32 entries
  { kind: player|hazard, pos, strength, radius, hazardTypeId }
```

Static hazard attractors (gravity anchors, magnetic fields — PLAN 5.7) are loaded from
the level table and sorted by index; the player is always index 0.

### 4.4 Spatial hash

```text
uniform grid, cellSize = sqrt(worldArea / N)     # ~ average body spacing
buckets: u16 flat array + per-cell start offsets, rebuilt each step (O(N))
queries: circle(absorptionRadius) for absorption,
         rect(cameraRect + margin) for culling,
         circle(hazard.radius) for hazard influence
```

Chosen per PLAN 5.16 ("spatial hashing"). Cell size derives from level bounds and pool
capacity so it never depends on A-007's radii values; rebuild is a counting sort over
300 entries, well under budget.

### 4.5 Data-driven tables (formats only; content is A-008/A-009)

```text
ObjectTable:  { typeId, category, mass, radius, absorbable, hazardFlags }
LevelTable:   { seed, worldBounds, spawnEvents: { time, typeId, count, region } }
```

The prototype (PLAN phase 1) consumes the same tables with one entry each.

## 5. Rendering architecture

RenderLayer is strictly read-only against sim state; it never feeds the sim (except
player intent, which enters through InputAdapter). It interpolates positions with the
`alpha` from 3.2, so visual motion is smooth at any render rate while the sim stays
fixed.

- **Batching:** one texture atlas per world; URP 2D dynamic batching; target <= 20
  draw calls per frame (PLAN 5.16 "low draw calls").
- **Culling:** bodies outside `cameraRect + margin` are not drawn; the count drawn is
  bounded by the visible density budget (5.2), typically 60-120.
- **LOD (PLAN 5.16):** tier A near = full sprite + glow; tier B mid = sprite only;
  tier C far = 1-4 pixel point. Tier boundaries are render-quality constants, not sim
  state.
- **Particles:** pooled, budgeted per quality tier (PLAN 5.16 "low particle count",
  "simple particles"); look and counts are A-006.
- **Event horizon ring:** drawn by a simple shader on a dedicated draw call; the
  readability design ("mine" vs "not yet mine") is A-006's (spec open question 4).
- **Edge-indicator hook:** RenderLayer exposes a `visibilityLayer` output (which
  bodies are off-screen and pulled). Whether edge indicators are drawn is A-006's
  (spec open question 6).

## 6. Performance plan (PLAN 5.16)

Targets: 60 fps on modern phones, 30 fps fallback on weak devices, low memory, low
draw calls, low particle count.

Per-frame budget at pool capacity (mobile, mid-range):

| Stage | Budget |
| --- | --- |
| Sim step (300 bodies, <= 32 attractors) | <= 1.5 ms |
| Spatial hash rebuild + queries | <= 0.3 ms |
| Render (<= 120 drawn, batched) | <= 8 ms |
| Particles + horizon shader | <= 2 ms |
| GC | zero allocation in hot loop; no Unity object churn |

Mechanisms (all named in PLAN 5.16): object pooling (BodyPool free list), spatial
hashing (4.4), culling (5), LOD (5), simple shaders (5), fixed timestep (3.2).
Memory: pool 300 x ~64 B = ~19 KB; atlas <= 2048x2048 RGBA compressed per world;
total runtime budget <= 150 MB on Android.

Decision: quality tiers are `high` (LOD A/B, full particles, 60 fps) and `low`
(LOD B/C only, reduced particles, resolution scale 0.75, 30 fps render). Tier is
chosen by device probe at boot and is a render-only switch (see 3.2).

## 7. Body count and readable field (open question 2)

**Answer: hard cap of 300 simultaneous dynamic bodies, plus <= 24 static hazard
entities, and readability survives through density budgets plus render bounds.**

Pool capacity split (PLAN 5.7 classes):

```text
small  <= 220
medium <= 56
large  <= 24
hazard (static attractors, separate pool) <= 24
```

Rationale for 300: at 60 Hz the sim cost is ~300 attraction integrations + 300
absorption checks per step, ~100x under the 1.5 ms budget; the binding constraint is
readability, not compute, so the cap is set where the readable-field rule of PLAN 5.2
still holds on the smallest phone.

How the readable-field rule survives the cap:

1. **Density budget:** SpawnDirector never places more than `maxPerScreenArea` bodies
   inside the camera rect (a data constant tuned by A-006/A-009); when the budget is
   full, spawns wait outside the camera margin. The field therefore never becomes a
   dot-cloud too dense to read, regardless of the global 300 cap.
2. **Render bounds:** culling + LOD (section 5) cap what is actually drawn at ~120
   sprites; the remaining bodies are off-screen simulation state the player cannot see
   and need not be readable.
3. **Hooks, not decisions:** the architecture provides `visibilityLayer` and the
   density-budget readout so A-006 can finish the readability decisions (horizon
   rendering, edge indicators) without touching the sim.

Spatial hashing and culling are the mechanisms PLAN 5.16 named; choosing them here is
A-005's, and both are specified above.

## 8. Open questions for downstream artifacts

For A-013 docs/test-plan.md:

1. How to test determinism: golden replay files (`seed + inputDigest -> score + runHash`)
   re-simulated headless on desktop and Android; what tolerance (expect exact match —
   fixed-point leaves none) and what fuzzing policy over seeds?
2. How to test the density budget and pool cap (spawn pressure tests at capacity)?
3. Does the verification policy for leaderboard entries (re-sim vs heuristics) get a
   test of its own, and where does it sit with A-015/A-016 store work?

For A-019 docs/prototype-scaffold.md:

4. Which SimCore modules are stubbed vs implemented in PLAN phase 1 (one black hole,
   one level, move/attract/absorb/grow)? Proposal: implement FixedStepDriver, PRNG,
   BodyPool, PlayerController, AttractionSystem, AbsorptionSystem, GrowthSystem,
   SpatialHash; stub HazardSystem, Stability, Combo, Scoring, SpawnDirector tables.
5. Where do the 32.32 fixed-point helpers (muldiv, isqrt) live — shared runtime library
   vs per-project — so A-013's headless re-sim and the game share one implementation?
6. World-unit definition: the architecture is unit-agnostic; A-019 must pick the world
   unit scale (proposal: 1 unit = starting body visual radius) before tables are authored.

For A-007 (informational, not blocking): all data constants (`a`, `pullScale`,
`baseRadius`, `epsilon`, radii, masses) must be representable in 32.32 fixed-point;
`epsilon >= 2^-32`.
