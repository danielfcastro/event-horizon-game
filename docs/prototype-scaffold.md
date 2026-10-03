# A-019 Prototype Scaffold — Event Horizon

## 1. Purpose, scope, and inherited contract

A-019 is the first code scaffold: the module skeleton the programmer starts from for the
black hole, bodies, gravity, absorption, camera, UI, and input in PLAN phase 1 (one black
hole, one level, move/attract/absorb/grow). It is consumed by the programmer as the entry
point for all code work and by A-013's test families through the harness in section 9.

Inherited contract, unchanged by this artifact:

- **Determinism (A-005).** The sim core is pure C# with zero Unity API dependencies and is
  re-simulable headless on desktop. Every SimCore subsystem is a pure function of
  `(state, stepIndex, intent)`: no wall clock, no Unity API, no unordered-iteration reads;
  fixed-point 32.32 (signed 64-bit) for positions, velocities, mass, force; splitmix64 seeds
  xoshiro256+; input quantized to steps; index-order iteration. A run is identified by
  `(sessionSeed, inputDigest)`.
- **No-softening (PLAN section 8).** No affordance may soften the mass tension. The scaffold
  is forbidden to define or stub aim assists, attraction magnets, catch-up rubber-banding,
  mass buffers or shields, undo/rewind, auto-absorb, or difficulty scaling. The rule
  constrains values, not slots: Stability, Combo, and Scoring may be stub slots without
  values, and a stub returns identity, never a cushioning default.
- **ID spine (A-013).** Families S, B, U, I, A, P, R are the spine and are not redefined
  here. This artifact claims a fresh namespace **H-** for harness commands (section 9).
  Not reused elsewhere: M-, SH-, SG-1..SG-7, SG-6.1..SG-6.6, PR-1..PR-10.
- **Numbers.** Balance values are A-007's and content is A-008/A-009's; any number needed to
  compile here is marked `PLACEHOLDER` and replaced by the approved table value. Nothing
  here renumbers A-007.

## 2. Answers to A-005 open questions 4-6

**4. Phase-1 stub/implementation split.** Adopted as proposed; the full split is section 4's
status column (implemented: FixedStepDriver, PRNG, SpatialHash, BodyPool, PlayerController,
AttractionSystem, AbsorptionSystem, GrowthSystem, InputDigest; stubbed: HazardSystem,
StabilitySystem, ComboSystem, ScoringSystem, SpawnDirector; absent: MetaLayer), with two
refinements: (a) `InputDigest` is a container plus codec from the first commit, because
A-013's I-family and A-014's golden replays need it; the intent *policy* stays A-011's and
is injected through `InputAdapter`. (b) `SpawnDirector` is stubbed, not absent, because
AbsorptionSystem and BodyPool need a spawn call site; the stub replays a fixed
`LevelTable.spawnEvents` list and consumes no PRNG draws, so phase-1 runs stay reproducible
without a director. HazardSystem exposes only A-005's `applyInfluence` and `onContact`, no-ops.

**5. Fixed-point helper placement.** One implementation, in `Assets/Runtime/Fixed/`,
compiled by both targets: the Unity build takes it as ordinary Runtime sources, the headless
harness compiles the same files with `dotnet build` (no Unity headers needed, the Runtime
layer is API-free). No per-project copy, no fork. Enforcement: the harness build hashes the
`Fixed/` and `SimCore/` file sets and fails with `FIXED-FORK` on a mismatch, which is what
keeps A-013's re-sim and the shipped game on one arithmetic.

**6. World-unit scale.** Adopted as proposed: **1 world unit (WU) = the visual radius of the
player's starting black hole at starting mass.** Section 6's table fixes the unit of every
quantity; `LevelTable.worldBounds` is `{minX, minY, maxX, maxY}` in WU with a `PLACEHOLDER`
phase-1 value owned by A-009, and Unity `pixelsPerUnit` is a render-only constant in
`PlatformBridge` that never enters SimCore, so art rescaling cannot change a replay.

## 3. Scaffold directory layout

The A-005 layering is kept literally; the harness sits outside `Assets/` so Unity's asset pipeline never sees test code.

```text
Assets/
  Runtime/     pure C#, zero Unity API
    Fixed/     FixedQ.cs, MulDiv.cs, ISqrtQ.cs                    (section 5)
    SimCore/   FixedStepDriver, PRNG, SpatialHash, BodyPool, InputDigest, PlayerController,
               AttractionSystem, AbsorptionSystem, GrowthSystem, HazardSystem, Stability,
               Combo, Scoring, SpawnDirector (.cs each) + Sim/SimState.cs, SimStep.cs
  Bridge/Unity/ GameLoop.cs (entry, accumulator, MAX_CATCHUP), InputAdapter.cs (telemetry ->
               A-011 intent channel -> intent), RenderLayer.cs (camera, render only),
               UIRoot.cs (HUD slots, no layout), LevelLoader.cs, PlatformBridge.cs (switch,
               PPU, tier probe); MetaLayer.cs absent
  Data/        ObjectTable.cs {typeId, category, mass, radius, absorbable, hazardFlags},
               LevelTable.cs {seed, worldBounds, spawnEvents}   both generated
tools/
  harness/     main.cs, Commands.cs (H-01..H-09), Snapshot.cs, Counters.cs, HitchInjector.cs,
               Replays.cs                    desktop-only, no Unity
replays/                         golden replays, section 7 layout
artifacts/harness/               harness run output, section 11
```

`SimState` is one struct (not one object per system) so that a snapshot is a single
deterministic walk, and `SimStep` fixes the system order once, in this file, so no
contributor can reorder it locally: `PlayerController -> AttractionSystem ->
AbsorptionSystem -> GrowthSystem -> HazardSystem -> StabilitySystem -> ComboSystem ->
ScoringSystem`, each over ascending body index.

## 4. Module-by-module phase-1 status

Signatures are the contract the programmer implements; `Q` is the 32.32 type from section 5.
Implemented means the file ships working phase-1 logic; stub means the file ships with the
signature and an identity body.

| Module | Status | Signature and phase-1 body |
| --- | --- | --- |
| FixedStepDriver | implemented | `void step(SimState s, long stepIndex, Intent i)`; owns DT, accumulator, `MAX_CATCHUP = 4`, dropped-step rule |
| PRNG | implemented | `ulong next(SimState s)`; splitmix64 seeds xoshiro256+; one stream per level, `seed = hash(levelId, sessionSeed)` |
| SpatialHash | implemented | `void rebuild(SimState s)`; uniform grid, counting sort, ascending-index query sets |
| BodyPool | implemented | SoA, capacity 300 (small <= 220, medium <= 56, large <= 24) + separate hazard pool <= 24; `alloc/freeList`; preallocated at boot, zero allocation in the hot loop |
| PlayerController | implemented | `Intent sample(SimState s)`; black hole is body index 0; movement intent only |
| AttractionSystem | implemented | `void apply(SimState s, int body)`; the A-005 formula verbatim, attractor-index order, attractor list <= 32, player always index 0 |
| AbsorptionSystem | implemented | `void apply(SimState s, int body)`; `distance < absorptionRadius`, once per step; bodies do not collide with each other |
| GrowthSystem | implemented | `void apply(SimState s, int body, ObjectTableRef t)`; mass -> `eventRadius = baseRadius + a*isqrt(mass)`; `a` is `PLACEHOLDER` from A-007 |
| HazardSystem | stub | `void applyInfluence(SimState s, int body, int hazard)` and `void onContact(SimState s, int body, int hazard)`; both no-op in phase 1 |
| Stability / Combo / Scoring | stub | `void apply(SimState)`, `void apply(SimState, bool absorbedThisStep)`, `long score(SimState)`; slots only, no values (A-007 fills) |
| SpawnDirector | stub | `void apply(SimState s, long stepIndex)`; replays fixed `LevelTable.spawnEvents`, consumes no PRNG draws |
| InputDigest | implemented (codec only) | `void push(SimState s, Intent i)`; `byte[] encode()`, `Intent decode(byte[], long step)`; policy injected by InputAdapter |
| LevelLoader | implemented (minimal) | loads the single `LevelTable` entry into `SimState`; no streaming |
| RenderLayer | stub (camera only) | `void draw(SimState s, float alpha)`; `alpha` is render-only interpolation, never fed to SimCore |
| InputAdapter | implemented (transport) | maps device telemetry to A-011's intent channel, then to one fixed-point `Intent` sampled once per step; holds no input scheme of its own |
| UIRoot / PlatformBridge | stub slots / implemented minimal | HUD slot registration only, no layout (A-010) / build-target switch, `pixelsPerUnit`, tier probe (section 10) |
| MetaLayer | absent | no file; monetization must never block the core game |

`Intent` is the phase-1 record `{ Q thrustX, Q thrustY, Q rate }` plus section 8's quantized
codes; it carries no device identifiers, no timestamps, and no pointers.

## 5. Fixed-point helper spec and placement

`FixedQ` = signed 64-bit, 32.32: range +/- 2^31 WU, resolution 2^-32 WU. No `float` and no
`double` anywhere in `SimCore`.

```csharp
// Assets/Runtime/Fixed/FixedQ.cs
static long fromInt(int v);   // v << 32
static long fromArtOnly(double v);  // boot-time table load only, never inside a step
static int toInt(long a);     // truncation toward zero
static int cmp(long a, long b);
// Assets/Runtime/Fixed/MulDiv.cs
static long muldiv(long a, long b, long d); // (a*b)/d via a full 128-bit intermediate,
//   half-away-from-zero rounding, d == 0 hard-asserts (never a silent 0)
// Assets/Runtime/Fixed/ISqrtQ.cs
static long isqrt(long x);    // floor(sqrt(x)) for x >= 0, fixed-point aware
```

`muldiv` is required because A-005's attraction term `pullStrength * bodyMass /
distanceSquared` overflows int64 in the intermediate at late-game mass and distance, exactly
what the P-family exercises; `isqrt` is required by `eventRadius = baseRadius + a*sqrt(mass)`
with `a` a `PLACEHOLDER` from A-007. C# has no 128-bit integer, so `muldiv` must decompose
into 32-bit limbs (Hodgson-style), never promote to `double`; the scaffold ships a reference
limb implementation whose correctness is gated by the S-family. The harness self-tests both
helpers before any command runs, `H-01` refuses to start on failure, and a `FIXED-FORK`
source-set mismatch fails the build.

## 6. World-unit scale decision and how tables express it

Adopted: **1 WU = the visual radius of the player's starting black hole at starting mass.**

| Quantity | Unit | Declared in |
| --- | --- | --- |
| `posX, posY` / `velX, velY` | WU / WU per second | `BodyPool` SoA |
| `radius, absorptionRadius, eventRadius` | WU | `BodyPool`, `ObjectTable` |
| `mass` | body-mass units (dimensionless) | `BodyPool`, `ObjectTable` |
| `worldBounds`, `cellSize = sqrt(worldArea/N)` | WU (cellSize derived at boot) | `LevelTable`, `SpatialHash` |
| `pixelsPerUnit` | px per WU, render-only | `PlatformBridge` |

Rules:

1. Tables are authored in WU (`radius: 0.375`, never a pixel count); A-007's masses are in
   mass units.
2. `SpatialHash` derives `cellSize` from `worldBounds` at boot and asserts
   `1 WU <= cellSize <= 32 WU`, so a level with bad bounds fails at load, not at runtime.
3. The starting hole is exactly 1 WU of radius, which keeps the unit legible on screen.
4. Nothing in SimCore reads `pixelsPerUnit`; art rescaling cannot invalidate a golden replay.

## 7. Snapshot serialization format (canonical, byte-stable)

A snapshot is the whole observable sim state at a step boundary. Format `EHSNAP1`.
All integers little-endian, no padding, no alignment, no floats, no locale text, no
addresses, no timestamps, no Unity object references. Field order is fixed by this
document and by nothing else; a field added later is appended, never inserted.

```text
Snapshot :=
  magic      6 bytes  "EHSNAP1"
  ver        u16      = 1
  stepIndex  i64
  seed       u64
  prng       4 x u64            xoshiro256+ state
  hole       posX posY velX velY mass radius   (7 x i64, FixedQ)
  attractors u16 count, then count x i64       (FixedQ radii, index order)
  bodies     u16 capacity(300), then per index ascending:
               u8 flags | i8 category | i8 typeId | posX posY velX velY mass radius (6 x i64)
  hazards    u16 capacity(24), same record as bodies
  freeList   u16 count, then count x u16 indices, ascending
  counters   score | stability | combo (i64) | absorbedTotal u32 | highmarkSmall,
             highmarkMedium, highmarkLarge, highmarkHazard (u16) | stepsRun, droppedSteps (i64)
  digest     FNV-1a 64 over every preceding byte
```

Rules that make it byte-stable:

- Every pool index 0..capacity-1 emits a record, active or not, so an inactive slot is a zero
  record rather than an omission and two runs with different free-list history stay comparable;
  `freeList` itself is emitted sorted ascending, independent of allocation order.
- `counters` carries pool highmarks, which is how P-05 reads them (A-013 open question 6).
- The trailing FNV-1a is A-005's run hash; a leaderboard record is `{seed, score, inputDigest,
  digest}` and nothing else.

Golden-replay file layout (A-014's `replays/<seed>-<digest8>.json`):

```text
replays/<seed>-<digest8>.json
  { "format": "ehreplay1", "seed": "0x…", "levelId": "p1-level-01",
    "inputDigest": "<base64 of section 8 encoding>",
    "snapshots": [ {"step": 0, "b64": "<base64 EHSNAP1>"}, {"step": 600, "b64": "…"},
                   {"step": -1, "b64": "…"} ],        // step -1 = final state
    "digest8": "<first 8 hex of final FNV-1a>", "score": 0 }
```

JSON is the container only; comparisons are always over the `EHSNAP1` bytes, so JSON key
order and whitespace cannot affect a result. `digest8` is verified against the decoded final
snapshot before any diff runs, so a corrupt golden fails clearly rather than as a confusing
byte diff.

## 8. InputDigest encoding

The digest is indexed by **step index only**, never frame index, which is what makes it
stable across frame rates (A-014 I-11) and excludes raw pointers, device telemetry, and
timestamps. A-011's quantization granularity (16-way angle, 5 % rate steps) is restated
here as the wire format only; the policy that fills it stays A-011's.

```text
IntentCode := 2 bytes per step
  byte0  angle code 0..15 = the 16 directions; 16 = coast (zero thrust)
  byte1  rate code  0..20 = 0 %, 5 %, … , 100 %; 10 = neutral 50 %

Digest :=
  magic   6 bytes  "EIDIG1"
  ver     u16      = 1
  steps   uvarint  total step count the digest covers
  pairs   repeated { value u16 = byte0<<8 | byte1, runLength uvarint >= 1 }
  pad     zero-length run-length padding is illegal; sum(runLength) == steps
  fns     u64      FNV-1a over magic..last pair
```

- RLE is A-005's leaderboard requirement applied once, so in-memory and stored digests are the
  same bytes; a trailing coast/neutral run is trimmed to the last non-neutral step and recorded
  in `steps`, so a longer session cannot silently change an old replay.
- `decode()` is total: unknown magic, version mismatch, sum mismatch, or angle code > 16 is a
  hard rejection with a named reason, never a silent neutral fill (a repaired digest would make
  an I-family test pass by accident).
- The harness takes digests only as `@file` or hex of these exact bytes; there is no
  alternate intent format that could diverge from the canonical one.

## 9. Harness command surface (namespace H-)

All commands run from `tools/harness` on desktop against the same `Assets/Runtime/**` sources
the game builds. Exit code 0 = pass, 1 = fail, 2 = harness error; every command writes a
machine-readable record under `artifacts/harness/<runId>/` (section 11).

| ID | Command | Serves (A-013 family) |
| --- | --- | --- |
| H-01 | `harness sim --seed <hex> --digest @file --level <id> [--out <path>]` runs the sim headless to completion; prints final `digest`, score, steps, dropped steps | S-*, I-*, A-*, B-* |
| H-02 | `harness replay @replays/<file>.json [--assert-steps 600]` re-runs a golden replay and byte-compares every snapshot | S-01, A-10, A-11 |
| H-03 | `harness snap --seed … --digest … --step N` emits one `EHSNAP1` blob to stdout/file | S-01, A-10, A-11 |
| H-04 | `harness diff <blobA> <blobB>` reports the first differing byte offset plus the field name from section 7 | A-10, A-11 |
| H-05 | `harness counters [--seed … --digest …]` prints frame-time histogram, RSS, per-frame allocation count, pool highmarks | P-01..P-05 |
| H-06 | `harness hitch --at 120,341,902 --ms 42` injects deterministic hitches into the frame driver, not into SimCore | S-09, P-11 |
| H-07 | `harness fps --target 30 [--time-scale 2.0]` forces the 30 fps step budget on a strong machine | P-02, P-10 |
| H-08 | `harness pointer --script @file` injects synthetic pointer events into `InputAdapter` (A-014 T2) | I-*, U-* |
| H-09 | `harness tier --probe` reports the quality tier the device selects and asserts it changed render state only | P-* tier checks |

Hook availability against A-013 open question 6, stated explicitly:

- **Headless sim runs: available.** Runtime has no Unity API, so H-01 is a real re-simulation.
- **Byte-level snapshots: available.** Section 7 is the format; S-01, A-10, A-11 compare
  `EHSNAP1` bytes, and pool highmarks are counters in that blob, so P-05 reads the same bytes
  as the game. H-06 perturbs the frame driver only, so an injected hitch changes `droppedSteps`
  and nothing else, which is the S-09 / P-11 assertion.
- **Weak-device simulation: partially available, flagged, not dropped.** H-07 reproduces the
  30 fps step pattern, accumulator behavior, and dropped-step accounting on strong hardware,
  so P-02 and the deterministic half of P-10 stay regression-testable. It cannot reproduce
  thermals, cache behavior, or a mobile GPU, so P-10 keeps a hardware-only component on a
  reference device and its record carries `hardware-only: true`, visibly split rather than
  silently skipped.
- **RSS: desktop-only.** The record sets `rss: null` for mobile targets; P-05's portable half
  is the per-frame allocation counter, instrumented in the sim loop on every target.

## 10. Weak-device switch and quality-tier probe

1. **Time-scale / target-fps knob (A-014 need 4).** `PlatformBridge` exposes `dev.targetFps`
   (60 or 30) and `dev.timeScale` (default 1.0). `timeScale` scales the *wall-clock* advance fed
   to the accumulator and never changes `DT = 1/60 s`, so a 30 fps target runs two sim steps per
   rendered frame through the same accumulator and the weak-device path is exercised on a strong
   machine. The knob is reachable from `Runtime/` so the harness can set it without Unity, and
   `PlatformBridge` gates it to non-shipping builds.
2. **Quality-tier probe (H-09).** Tiers change render only, never `DT` (A-005). The probe
   returns the selected tier and asserts `SimState` is byte-identical across tiers for the
   same `(seed, inputDigest)`, the regression that keeps a quality setting out of difficulty.

`FixedStepDriver` asserts `DT` is the constant `1/60` every step and that `droppedSteps`
only grows through the `MAX_CATCHUP = 4` rule; a `timeScale` implying another `DT` is
rejected at startup, so no test can measure a different simulation.

## 11. Build and test entry points, and where harness output is filed

Entry points the programmer starts from:

```sh
dotnet build game-standalone.csproj   # desktop dev target (Android/iOS: same project, switch)
dotnet build tools/harness/harness.csproj -o build/harness   # desktop only
./build/harness sim --seed 0x1F4A --digest @replays/p1-level-01.digest.bin --level p1-level-01
```

`GameLoop.cs` `Start()` is the Unity entry (build `SimState` via `LevelLoader`, then call
`FixedStepDriver.step` in the accumulator loop); `tools/harness/main.cs` builds the same
`SimState` and calls the same `step`. There is exactly one simulation loop and both entries
reach it.

Filing (A-018's fail routing and its third open question):

- Each run writes `artifacts/harness/<runId>/manifest.json` plus one record per command,
  `artifacts/harness/<runId>/<test-id>.json`, with fields
  `{testId, command, seed, digest, result, owner, hardwareOnly}`.
- `owner` is resolved from A-018 section 5's fail-filing table (an S- or A- failure routes to
  the architecture/scaffold owner, a B- failure to A-007); the harness never decides
  ownership locally.
- A failure is filed on the artifact branch owning the failing test's subject, and the
  coordinator opens the fix on that artifact's `feature/<agent>-<id>` branch. Harness runs get
  **no branch type of their own** (the PLAN 1.3 branch list is fixed): they ride on the artifact
  branch they test, and `artifacts/harness/` is run state, never design. `replays/` goldens are
  committed fixtures; per-run output is gitignored except a failing run's manifest, committed
  so the failure is reviewable in the PR that fixes it.

## 12. Open questions for downstream (programmer, A-020) only

1. (programmer) `muldiv` limb decomposition: the scaffold ships a reference implementation; any
   optimization must stay inside the P-family budget and be re-verified by the S-family.
2. (programmer) Whether `ObjectTable`/`LevelTable` stay generated C# constants (the scaffold's
   assumption) or move to `ScriptableObject`s; if they move, the numbers must stay identical.
3. (programmer) How `dev.targetFps` is gated out of shipping Android/iOS builds without
   introducing a second simulation path.
4. (programmer) Whether `SimState` stays one struct as phase 2 adds content; section 7's field
   order may only be appended to, or goldens break.
5. (A-020 ship) Confirm `tools/harness`, the harness binary, and `replays/` goldens are absent
   from shipped player and store packages.
6. (A-020 ship) Confirm no `dev.*` knob is reachable in a store build, since a reachable
   time-scale knob is an affordance that could soften the mass tension.
7. (programmer) Platform tilt API mapping (which axis is yaw vs pitch per device orientation,
   calibration anchor captured from device telemetry) is implemented inside `InputAdapter`;
   A-011's intent channel is the required output and the digest format in section 8 is fixed.
