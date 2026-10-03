# Test Plan — Event Horizon

Artifact A-013. Agent: qa. Depends on A-005 (architecture, done); honors approved A-006, A-007, A-008, A-009, A-010, A-011, A-012. Consumed by A-014 (which runs these tests), A-019 (which must make the scaffold testable against this plan), and all code work.

## 1. Purpose and inherited contract

A-013 defines **what must be true** and **what tends to break**. It does not define how to run a test (A-014), how to build a harness (A-019/programmer), or any store behavior (release). Every test below is a statement about the running game, stated in terms of the architecture's own contracts so it can be checked mechanically.

Four contracts are inherited and are the spine of every category:

| Contract | Source | What a test may assert |
| --- | --- | --- |
| Determinism | A-005 | Sim is pure C#, signed 64-bit 32.32 fixed-point, seeded PRNG only, DT = 1/60 s, MAX_CATCHUP = 4. Results are device-independent; slow devices use time scale, never DT. Render interpolation never feeds back into sim. |
| No-softening | A-012, PLAN §8 | No accessibility configuration changes absorb gating, pull strength, decay rate, or hazard contact. A settings change never changes a balance-owned number. |
| SL-1 | A-012 §8 | The game stays legible with color off, motion frozen, and audio muted **at the same time**. A-013 samples SL-1; it does not invent it. |
| Canonical numbers | A-007 | The 40-level table, 25–40 absorbs/level, ≤26 worst single drain, live bodies 110–140, hazard budgets 6/10/14/18/22, world-L8 timers 190/230/270/310/350 s, 2,195 / 10,200 cores, mass ceiling 2^24, speed 14.3 at 1M, efficiency cap 1.00. Tests compare against these; they never renumber them. |

The tension rule (PLAN §8) governs test authorship: mass growth buys reach but costs speed, control, and drain pressure. **A test that passes only when the tension has been removed is a design bug, not a test fix.** When a test fails because the game is too hard, the fix is a level or content change (A-008/A-009), never a balance renumber and never a weakened assertion.

Test naming convention used throughout: `S-` simulation, `B-` balance, `U-` UI, `I-` input, `A-` accessibility, `P-` performance, `R-` regression risk. A-014 assigns pass/fail procedures to these IDs.

## 2. Simulation tests

All S-tests run headless against the sim core (A-005's pure-C# core), with no renderer present, so that render behavior cannot mask sim behavior.

### 2.1 Fixed-point determinism

- **S-01 Bitwise reproducibility.** Run the same level twice in two separate processes with the same (seed, inputDigest). Assert the final sim state (body positions, masses, drain, timer) is byte-identical. Any float (`float`/`double`) reaching sim state is a failure.
- **S-02 No platform float drift.** Same run on the three target platforms (or their nearest available equivalents). Assert identical sim state. A divergence caused by libm differences is a determinism failure, not a tolerance issue — fixed-point leaves no tolerance.
- **S-03 Overflow boundaries.** Exercise the mass ceiling 2^24 and the 32.32 representation near it: mass at 2^24 − 1 plus an absorb must saturate, not wrap. Assert no negative mass, no wraparound, and that the cap terminates rather than overflows (A-009 target 9).
- **S-04 PRNG purity.** Assert every random draw in a run comes from the seeded stream, in call order, and that a replayed run draws the same sequence. Assert the chain-reaction BFS draws zero PRNG values (A-005: no PRNG in BFS).

### 2.2 Replay from (seed, inputDigest)

- **S-05 Replay equality.** From (seed, inputDigest) alone, reconstruct a run and assert it reproduces the recorded outcome. Nothing else may be a seed of the run: no wall-clock time, no frame count, no device identity.
- **S-06 Digest purity.** Assert the digest contains only quantized intents and discrete events — no raw pointers, no pixel coordinates, no frame indices. Two different raw-pointer traces that quantize to the same intents must produce the same digest.
- **S-07 Replay under stress.** Replay a recorded run at a throttled frame rate (see P-tests, §7.2). Assert the replayed sim state matches the reference run exactly — catch-up and time scale must not change results.

### 2.3 Timestep and catch-up

- **S-08 Fixed DT.** Assert the sim advances in exact 1/60 s steps; no variable-dt path exists. Assert the number of sim steps over a fixed wall-clock interval equals floor(elapsed × 60) plus bounded catch-up.
- **S-09 Catch-up cap.** Force a frame hitch of 1, 2, 4, 8, and 16 steps. Assert at most 4 steps are consumed per frame (MAX_CATCHUP), that the excess is dropped as *sim time debt* rather than run through, and that the sim clock and the visual clock stay consistent afterward. A hitch that lets the sim run 8 steps in one frame is a failure.
- **S-10 Time scale, not DT.** On a simulated weak device, assert slow-down is applied as time scale and that DT remains 1/60. Assert the same (seed, inputDigest) still yields the same sim outcome under time scale.

### 2.4 Pool accounting

- **S-11 Capacity invariant.** Assert live bodies never exceed 300 pool slots and hazards never exceed 24, with the Quantum Nebula worst case (140 + 8 + 24 = 172 of 300) reachable and inside budget. Assert the free list never double-allocates or leaks a slot: allocate/free a full cycle and assert the free list returns to its initial contents.
- **S-12 SoA integrity.** Assert no body index is simultaneously free and live; assert the free-list length plus live count equals capacity at every step.
- **S-13 Spatial hash.** Assert uniform-grid queries return the same neighbor set as a brute-force query at sampled steps, and that cell iteration order is stable across runs (this is the ordering S-14 depends on).

### 2.5 Chain reactions

- **S-14 BFS order.** Assert chain reactions are a deterministic depth-2 BFS iterated in spatial-hash cell order, with mass>>1 transfer, no extra drain, and no PRNG. Two runs of the same trigger configuration must produce identical chain trees.
- **S-15 Depth bound.** Assert no chain resolves deeper than 2 levels; assert a chain cannot be extended by level content into a third level.
- **S-16 Chain cost.** Assert a chain grants reach/mass without granting a free drain refund — the tension rule must survive the chain path (see B-tests, §3.4).

### 2.6 Cross-cutting sim invariants

- **S-17 Monotonic invariants.** Across a full run assert: mass never decreases outside documented transfers, drain never becomes negative, timer never increases, and no body leaves the field bounds without a documented despawn.

## 3. Balance tests

All numbers below are A-007's canonical display/reference values. A-013 compares against them; it never restates them as new numbers and never re-tunes them. A failing B-test is reported as a balance or level defect to its owner, not softened here.

### 3.1 A-007 regression anchors

- **B-01 Absorb count band.** For each of the 40 levels, assert a designed solution needs 25–40 absorbs. Assert the worst single drain on any level is ≤ 26.
- **B-02 Live-body band.** Assert live bodies stay inside 110–140 at every sampled instant on every level (this is the same band A-009 target 4 checks; both must agree).
- **B-04 Economy totals.** Assert permanent upgrade totals sum to 2,195 cores and total economy supply is 10,200 cores. Assert no purchase path grants cores faster than the play-only economy allows — a pay-to-win shortcut is a failure (PLAN 5.14).
- **B-05 Ceiling and caps.** Assert mass ceiling 2^24, speed 14.3 at 1M mass, efficiency capped at 1.00. Assert **no upgrade reduces drain tables** — an upgrade that lowers drain is a tension-rule violation.

### 3.2 A-009 regression targets 4–9

A-009 states these as "targets, not tests"; A-013 turns each into a checkable assertion over the shipped seeded recipes.

- **B-06 (target 4) ±10% live-mix band.** Assert the §9.1 live-mix band holds at every one of the 40 levels under the seeded recipes, with live bodies inside A-007's 110–140.
- **B-07 (target 5) Hazard budgets.** Assert live hazard counts never exceed 6/10/14/18/22 by tier, at any instant, **including under the modifiers named on that level**. Sampling must include the modifier-active window, not only the base layout.
- **B-08 (target 6) Clean route + 3-star line.** Assert a clean route inside the timer exists on all 40 levels (A-008's mine guarantee), and that the 3-star line (≥ 35% timer left) is achievable on all 40. Timers are A-007's world-L8 values 190/230/270/310/350 s.
- **B-09 (target 7) Mass-threshold avoidance.** Assert L31 and L37 are clearable without ever touching the mass-threshold.
- **B-10 (target 8) Anomaly bloom placement.** Assert anomaly bloom never places an anomaly inside the first 20 s of field on L33 or L40.
- **B-11 (target 9) Endless tier crossings.** Assert a tier crossing never carries a hazard budget above the tier's own ceiling, and that the 2^24 cap terminates rather than overflows (pairs with S-03).

### 3.3 Tension-rule checks — no path removes cost

These are the tests that keep the game "not a pure power fantasy." Each asserts a *cost still exists*, so a buff that deletes the cost fails.

- **B-12 Reach costs speed.** Assert that at higher mass the achievable traversal time across a fixed route increases (speed at 1M mass = 14.3 is the reference). A mass tier that is strictly faster is a failure.
- **B-13 Reach costs control.** Assert the control affordance degrades with mass on the documented curve; a mass tier with unchanged control is a failure.
- **B-14 Reach costs drain pressure.** Assert drain pressure rises with mass on the documented tables; a mass tier with flat drain is a failure.
- **B-15 Every gain path has a cost.** For each gain path (plain absorb, chain reaction, Quantum mode, Star mode, upgrade purchase), assert an associated cost is paid. A gain path with no cost is a design bug and must be reported to A-007/A-008, not patched here.
- **B-16 Quantum not too strong / Star not too weak.** Assert Quantum mode does not clear the level's absorb band (B-01) by itself, and Star mode remains a viable route to the 3-star line (B-08). These are the PLAN §5.17 balance lines, expressed as bounds rather than opinions.
- **B-17 Endless does not stall.** Assert endless mode keeps producing reachable content past the last authored tier and that no state leaves the player with zero viable absorbs. A stall is a failure.

### 3.4 Unvalidated parameters (do not move)

Flagged from A-009's own open items: endless tier boundaries **600,000** and **2,000,000** and the **55%** mass-threshold are A-009 parameters, not A-007's. **3,000** and **120,000** are A-007's and must not move. B-tests read whichever is authoritative per owner; they never assert a value for the A-009-owned set beyond "it is the shipped constant," and any disagreement between owners is an open question (§10), not a test decision.

## 4. UI tests

A-010's testability list is the contract; each item becomes one U-test. `ru` is A-010's reference unit. The three frames under test are 320 × 568, 390 × 844, and 430 × 932.

### 4.1 Safe areas and ring-zone exclusion

- **U-01 Safe-area containment.** Assert every HUD bounding box lies outside the ring-zone rect at s = 1, s = 0.82, and the largest inset clamp, and that zero HUD pixels fall inside the ring zone at any zoom, across all three frames.
- **U-02 Safe-area fit.** Assert HUD elements sit inside device safe areas on all three frames, including notch/chin-bar geometry as modeled by A-010.
- **U-03 Pause button size.** Assert the pause target is ≥ 64 ru at every frame and with the larger-controls tier on.

### 4.2 Touch floors and indicator limits

- **U-04 Touch floor.** Assert no interactive element is below 44 ru after scaling, including level-select cells.
- **U-05 Indicator cap/floor.** Assert at most 8 glyphs, that the `+n` badge is the only overflow form, and that glyph ≥ 8 ru with stroke ≥ 2 ru on the smallest frame.

### 4.3 Reflow and state legibility

- **U-06 Reflow coverage.** Assert each rule in A-010 §7.3 is reachable by some viewport and that the rule-5 fallback is exercised by at least one tested viewport.
- **U-07 Color-independent state.** Assert stability stage and pressure-cluster state are distinguishable with the colorblind palette on and high contrast off, and that pips are distinguishable by shape alone.
- **U-08 Motion-independent state.** Assert that with reduced motion on, the flicker stage of §4.2 is still distinguishable from the dashed stage.
- **U-09 Audio-independent state.** Assert that with audio off, stability decay is still legible through segment count and tick spacing.

### 4.4 Render and number fidelity

- **U-10 Draw-call fit.** Assert the HUD renders as a fixed set of batched layers, that the indicator pool size equals 8, and that no element costs its own draw call (pairs with P-04).
- **U-11 Number fidelity.** Assert the timer, star line, efficiency cap, and mass ceiling displayed match A-007 exactly, with no UI rounding beyond the documented abbreviation. A display that disagrees with the sim value is a failure even when the sim is correct.

### 4.5 UI-vs-sim equality

- **U-12 Readout equality.** At sampled frames, assert every numeric readout equals the sim's own value at that step (not a render-interpolated value). This is the render-only rule of A-005 observed from the UI side: interpolation may move a sprite, never a number.

## 5. Input tests

A-011's testability list is the contract. The PLAN §5.17 input lines (drag on small and large phones, joystick works, pause button large enough, UI fits safe areas) are covered by I-01, I-02, I-04, and U-02.

### 5.1 Drag and cross-frame equality

- **I-01 Drag end-to-end.** Assert a drag works end-to-end on the smallest (320 × 568) and largest (430 × 932) A-010 frames.
- **I-02 Cross-frame intent equality.** Assert the same intent produces the same sim result across frames. This is the load-bearing determinism test for input: it fails if intent quantization depends on viewport size or scale.

### 5.2 Joystick and tilt

- **I-03 Joystick.** Assert the base can be placed anywhere in the ≥ 180 ru zone; the 15% inner dead zone reads as Hold; the 10% outer clamp applies; and a mode switch cancels intent.
- **I-04 Tilt.** Assert tilt works after calibration, and that uncalibrated tilt defaults to a safe neutral (Hold) rather than drifting.
- **I-05 Pause target.** Assert the pause target is ≥ 64 ru and single-tap, resume is one tap, and no other interactive target lies within 12 ru.

### 5.3 Contact policy

- **I-06 Multi-touch.** Assert a second contact during play does not steer.
- **I-07 Pause cancels intent.** Assert pausing cancels in-flight intent — no steering across a pause.
- **I-08 Edge/palm rejection.** Assert rejection behaves as specified on small frames, and that a normal thumb drag is never rejected.
- **I-09 Micro-move suppression.** Assert sub-8 ru contacts do not twitch (dead-zone micro-move suppression).

### 5.4 Replay determinism at the input boundary

- **I-10 Replay determinism.** Assert (seed, inputDigest) reproduces play, and that the digest contains only quantized intents plus discrete events — no raw pointers. This is the input-side half of S-05/S-06; a digest that carries raw pointers fails here even if the sim replays.
- **I-11 Intent quantization stability.** Assert the same physical gesture, sampled at different frame rates, quantizes to the same intent sequence. A frame-rate-dependent intent is a determinism failure that would otherwise hide in S-tests.

## 6. Accessibility tests

A-012's testability list is the contract. SL-1 (A-012 §8) is the simultaneous-legibility contract: color off, motion frozen, audio muted at the same time. A-013 samples it; it does not invent it.

### 6.1 SL-1 sampling

- **A-01 SL-1 legibility.** With monochrome + reduced motion + audio off + haptics off **all on at once**, assert the four stability stages are pairwise-distinguishable. Sample at the stages' boundaries and at the worst-case frame A-012 names.
- **A-02 Stage machine identity.** Assert stability labels, ring dash count, tick spacing, and the percentage readout all report the same stage in one frame — no disagreement. A disagreement is a cross-system bug, not a legibility complaint.
- **A-03 Reduced-motion equivalence.** Assert each static variant in §3 carries the same event as its animated form at the same frame index; nothing is deleted.
- **A-04 Flicker vs dashed.** Assert the 4-segment halved-tick flicker stage is distinguishable from the dashed stage with motion frozen and color off.

### 6.2 Geometry and palette integrity

- **A-05 High-contrast margin.** Assert the 3 ru stroke fits the §3.3 margin budget on every supported aspect, and that the 4 ru shrink branch fires only when margin < 1 ru.
- **A-06 Palette purity.** Assert switching palettes changes hue assignments only — glyph families, dash counts, and silhouettes are byte-identical. A palette swap that changes a glyph is a failure (it would silently change meaning).
- **A-07 Touch geometry.** Assert the 72 ru pair fits the 180 ru zone with ≥ 12 ru spacing and pause unmoved, at every UI-scale tier.
- **A-08 One-handed default.** Assert `intent: tilt` is the offered default and requires zero screen contact; assert joystick uses one screen contact in the control zone (A-011's floating base).
- **A-09 Pause confirm-state.** Assert it is only reachable on the 64 ru target, never adds a verb, and is default-on only under one-handed or larger-controls configurations.

### 6.3 No-softening — machine check

This is the highest-priority accessibility test and must be automated, not eyeballed.

- **A-10 No-softening (machine check).** Enumerate accessibility settings (colorblind palette, high contrast, reduced motion, audio off, haptics off, larger controls, one-handed, UI-scale tiers). For each setting, run the identical (seed, inputDigest) with the setting off and on, and assert the resulting sim state is **byte-identical**. Any setting that changes absorb gating, pull strength, decay rate, or hazard contact fails here.
- **A-11 Balance-number immutability.** Snapshot every balance-owned number (A-007's set in §3) with settings at defaults, then snapshot again with every accessibility setting toggled. Assert the two snapshots are equal. A settings change that moves a balance number is a violation of PLAN §8 and A-012's no-softening rule.
- **A-12 Accessibility render budget.** Assert static variants and thicker strokes fit the same pooled, tier-budgeted draw calls (A-005) — no new render layers, no new pools (pairs with P-04 and U-10).

### 6.4 Accessibility-vs-input interaction

- **A-13 Larger controls do not change intent.** Assert that with larger controls on, the joystick zone, dead zone, and outer clamp still produce the same intents as at defaults (A-011's numbers are not rescaled by accessibility). A control tier that rescales the dead zone is a failure and an open question for A-011/A-012 (§10).

## 7. Performance tests

Targets are PLAN §5.16 verbatim: 60 fps on modern phones, 30 fps fallback on weak devices, low memory, low draw calls, low particle count. Techniques named there — object pooling, spatial hashing, culling, LOD, simple shaders, fixed timestep simulation — are the mechanisms the P-tests observe.

### 7.1 Frame budget

- **P-01 60 fps on modern devices.** Assert a sustained 60 fps across a representative level set (early, mid, L8-timer levels, Quantum Nebula worst case) on a modern phone class. Assert the sim step budget per frame is bounded by MAX_CATCHUP = 4, so sim cost cannot be the thing absorbing the frame.
- **P-02 30 fps fallback on weak devices.** Assert the game runs at a stable 30 fps on a weak device class, and that the fallback is achieved through time scale and LOD, **not** by changing DT (see S-10).
- **P-03 No shader spikes.** Assert no first-use shader compile or variant switch causes a frame spike during play. Assert shader variants are limited to the simple set A-005 allows.

### 7.2 Memory and leaks

- **P-04 Stable memory.** Assert resident memory is flat across a long session (level after level, endless included): no growth trend across 10+ consecutive runs.
- **P-05 No leaks.** Assert no allocation in the steady-state loop: body, hazard, particle, and indicator use pools only (BodyPool: SoA + free list, 300 + 24; indicator pool 8 per U-10). Assert pool highmarks never exceed declared capacity.
- **P-06 Particle budget.** Assert particle counts stay inside the low-particle-count target, using A-005's Quantum Nebula accounting: particles every 4th absorb + 2 per chain, cap 8; 140 + 8 + 24 = 172 of 300 pool slots. A particle system that exceeds its declared cap is a failure.
- **P-07 Culling and spatial hashing.** Assert off-field bodies are culled from render work and that neighbor queries go through the uniform grid, not brute force. Assert query cost scales with cell occupancy, not with total bodies.
- **P-08 Draw calls.** Assert draw calls stay inside the low-draw-call target with a fixed, batched layer set (pairs with U-10 and A-12). Assert no per-element draw call anywhere, including accessibility variants.
- **P-09 LOD.** Assert LOD transitions fire at their declared thresholds and never change sim state — an LOD that changes a hitbox or a mass value is a determinism failure, not a visual one.

### 7.3 Performance-vs-determinism cross-check

- **P-10 Same result, different device.** Run one (seed, inputDigest) at 60 fps and at the 30 fps fallback and assert identical sim state. This is the test that keeps performance work from quietly becoming a gameplay change; it is the highest-priority P-test.
- **P-11 Hitch behavior.** Under an injected hitch, assert catch-up stays ≤ 4 steps (S-09) and that the game degrades by dropping render work, never by dropping sim steps.

## 8. Store checks — placeholder

The Store category of PLAN §5.17 is **release-owned**. A-013 does not invent store tests; it reserves the category and points at its owners.

| PLAN §5.17 store line | Owner |
| --- | --- |
| Screenshots clear, trailer short | A-015 docs/store.md |
| Age rating correct, privacy policy exists | A-015, A-017 docs/privacy-policy.md |
| IAP works, ads work | A-016 docs/monetization.md |
| Analytics works, crash reporting works | A-015/A-016, A-020 docs/ship.md |

Placeholder rule: A-014 lists this category as "deferred to release artifacts" and runs nothing here. The one thing A-013 does assert is the hard rule from PLAN: monetization must never block completion of the core game — if a monetization gate can prevent a level from being finished, that is reported to A-016, not tested here.

## 9. Regression risks

Each risk names a change that tends to break something far from it. A-014 must re-run the listed tests after each named change, not only the tests near the edit.

### 9.1 Economy retune

- **R-01 Economy retune.** Changing any upgrade cost or core reward threatens B-04 (2,195 / 10,200 totals), B-01 (absorb bands shift with economy), B-15 (a cheaper path can silently delete its cost), and B-16 (Quantum/Star strength). Re-run all of §3 after any economy edit. A retune that keeps totals but breaks a band is a tension-rule violation.
- **R-02 Upgrade table edits.** Any upgrade addition threatens B-05's "no upgrade reduces drain tables" and A-11 (balance-number immutability). Upgrade tables and drain tables must stay disjoint.

### 9.2 Hazard budget and level edits

- **R-03 Hazard budget edits.** Changing a tier budget threatens B-07 at that tier **and** B-11 at endless crossings, plus S-11 (hazard pool capacity 24). A budget raise that exceeds 22 or the pool is a double failure.
- **R-04 Modifier edits.** Adding or changing a modifier threatens B-07 (budgets under modifiers) and B-08 (clean route still exists). Modifiers must be re-sampled, not assumed safe.
- **R-05 Level recipe edits.** Any recipe change threatens B-06 (±10% live-mix at all 40 levels) and B-02/B-06 agreement with A-007's 110–140 band. A single level edit can move the band at neighbors.
- **R-06 Anomaly bloom edits.** Threatens B-10 (L33/L40 first-20s rule) and B-08 (route still clean). Anomaly bloom is seeded — an unseeded bloom is also an S-04 failure.

### 9.3 UI scale tiers and accessibility

- **R-07 UI scale tiers.** Changing a scale tier threatens U-01 (ring-zone containment at s = 1, 0.82, largest inset clamp), U-03 (≥ 64 ru), U-04 (44 ru floor), A-07 (72 ru pair in 180 ru zone), and I-02 (cross-frame intent equality). Tiers and safe areas must be re-checked across all three frames together, not per tier.
- **R-08 Accessibility toggles.** Any accessibility change threatens A-10/A-11 (no-softening) and P-08/A-12 (render budget). The no-softening check is the guard: it exists precisely because accessibility edits are the most likely place a designer touches a balance number.
- **R-09 Palette edits.** Threaten A-06 (palette purity) and U-07/A-01 (color-independent legibility). A hue-only edit must remain hue-only.

### 9.4 Replay drift and determinism

- **R-10 Replay drift.** The highest-value regression family. Any change to sim code, quantization, spatial-hash iteration, or pool ordering threatens S-01/S-05/S-06/S-14 and I-02/I-10/I-11. Keep a golden set of recorded (seed, inputDigest) runs and re-assert byte equality after **every** change of any kind, including UI-only changes.
- **R-11 Pool ordering change.** Free-list or SoA ordering changes silently change chain-reaction resolution (S-14 depends on cell order). A pool refactor must be paired with a golden-replay run.
- **R-12 Time-scale change.** Threatens S-09/S-10/P-10. A fallback that touches DT is a determinism failure that only shows up on weak hardware — P-10 is the test that surfaces it.
- **R-13 Render interpolation leak.** Threatens U-12 and S-01. If a render-interpolated value ever reaches sim or HUD, determinism and readouts both degrade; A-005's rule is that this never happens.

### 9.5 Cross-category risks

- **R-14 Tension erosion.** Any buff, mode, or content addition must be re-run against B-12..B-16. This is the standing guard on PLAN §8: the game drifting toward a power fantasy is a regression, not a redesign.
- **R-15 SL-1 erosion.** Any visual or HUD change must be re-run against A-01..A-04. Legibility is easiest to lose during content polish, which is exactly when nobody re-checks it.

## 10. Open questions for downstream artifacts

These are questions A-013 leaves open on purpose. They are not test decisions and must not be answered here.

### For A-014 docs/qa.md (runs these tests)

1. **Pass/fail mechanics.** A-013 states assertions; A-014 must decide how each is executed, what counts as a pass, and which are automated versus manual. Priority for automation: S-01, S-05, A-10, A-11, P-10, R-10 (golden replays).
2. **Sampling density.** B-02, B-06, B-07, U-01 say "at every instant" or "at any zoom." A-014 must choose sampling that is defensible for the "never exceeds" claims, and state its own limits honestly.
3. **Device classes.** PLAN §5.16 says "modern phones" and "weak devices" without naming hardware. A-014 must pick the reference devices and record them, so P-01/P-02 are reproducible.
4. **Golden replay set.** Which (seed, inputDigest) runs form the regression set for R-10, and how they are stored, is A-014's decision.
5. **Ordering.** Which tests gate a merge. A-013 implies determinism and no-softening first, but the gate is A-014's.

### For A-019 docs/prototype-scaffold.md (must be testable against this plan)

6. **Harness hooks.** A-013 requires headless sim runs (S-tests), byte-level state snapshots (S-01, A-10, A-11), pool highmarks (P-05), and injected hitches (S-09, P-11). Whether A-005's architecture exposes these hooks is A-019's to resolve; if a hook is impossible, A-019 must say so rather than let the test be skipped silently.
7. **State snapshot format.** Byte-identical comparison needs a canonical serialization of sim state. A-019 owns that format; A-013 only requires it be stable.
8. **InputDigest encoding.** I-06/I-10 require a digest of quantized intents. A-019 owns the encoding; A-013 only requires it exclude raw pointers.
9. **Weak-device simulation.** P-02 and P-10 need a way to force the 30 fps path on a strong machine. If A-019 cannot provide that, P-10 becomes hardware-only and its regression value drops — flag it, do not drop the test.

### For release (A-015..A-017, A-020)

10. **Store category ownership.** The Store lines stay with release; A-013's placeholder must not be replaced by tests here.
11. **Analytics/crash reporting and determinism.** Crash reporting and analytics run alongside the sim. Release must confirm they never touch sim state or the digest (they would break S-05/I-10). A-013 cannot test this; it is a release-side constraint.
12. **Age rating and monetization gates.** Whether an ad or IAP gate can interrupt a level is release's to verify against the PLAN rule that monetization never blocks core completion.

### Conflicts to flag upstream (A-013 cannot resolve)

13. **A-009 vs A-007 parameter ownership.** 600,000 / 2,000,000 / 55% are A-009's; 3,000 / 120,000 are A-007's and must not move. If a future edit moves an A-007 value, that is a conflict for A-007, not a test change here.
14. **Accessibility vs input geometry.** A-011's fixed geometry (180 ru zone, 15%/10% zones, 8 ru suppression) and A-012's scale tiers both apply to the same controls. If a tier implies rescaling A-011's numbers, that is a conflict for A-011/A-012; A-13 exists to surface it, not to resolve it.
15. **Tension-rule vs content.** If a B-test fails because content is genuinely unfair, the fix belongs to A-008/A-009. A-013 records the failure and stops; weakening the assertion is not an option.
