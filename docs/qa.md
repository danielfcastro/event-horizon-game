# docs/qa.md — A-014 QA checklist (qa agent)

## 1. Purpose and inherited contract

A-014 executes A-013's test IDs; it defines no new assertions and never weakens
A-013's. The spine of every checklist row is an A-013 ID: S-01..S-17, B-01..B-17,
U-01..U-12, I-01..I-11, A-01..A-13, P-01..P-11, R-01..R-15. A-014 assigns pass/fail
mechanics, execution tiers, sampling, devices, golden replays, and the merge gate.

Inherited contract (from A-013, summarizing A-005 / A-012 / A-007):

| Contract | What QA enforces |
| --- | --- |
| Determinism (A-005) | Pure C# sim, signed 64-bit 32.32 fixed-point, seeded PRNG only, DT = 1/60, MAX_CATCHUP = 4, device-independent via time scale (never DT), render interpolation never feeds back into sim. |
| No-softening (A-012, PLAN §8) | Accessibility settings never absorb gating, pull strength, decay rate, or hazard contact; a settings change never changes a balance-owned number. Machine-checked (A-10/A-11), never eyeballed. |
| SL-1 (A-012 §8) | Legible with color off, motion frozen, and audio muted at the same time; A-014 runs A-01..A-04 as the SL-1 sample. |
| Canonical numbers (A-007) | 40-level table, 25–40 absorbs/level, ≤26 worst drain, live bodies 110–140, hazard budgets 6/10/14/18/22, timers 190/230/270/310/350 s, 2,195/10,200 cores, 2^24 cap, speed 14.3 at 1M, efficiency cap 1.00. Compare, never renumber. |

A-013 flags: endless tier boundaries 600,000/2,000,000 and the 55% mass-threshold
are A-009 parameters (unvalidated); 3,000/120,000 are A-007's and must not move.

## 2. Execution tiers

Every A-013 test ID is assigned to exactly one tier. Tier 1 runs headless in CI
(no device needed); tier 2 runs on reference devices with synthetic input
injection, frame-time counters, memory sampling, and screenshot diffing; tier 3
is human playtest that confirms the same IDs by eye and hand. A tier-3 pass
never substitutes for a tier-1 or tier-2 fail.

| Tier | What it is | Test IDs |
| --- | --- | --- |
| T1 — automated headless | Deterministic sim harness, golden replays, machine checks. Automation priority (A-013 Q1): S-01, S-05, A-10, A-11, P-10, R-10. | S-01..S-17, B-01..B-17, I-09, I-10, I-11, U-12, A-10, A-11, P-10, R-01..R-15 (re-run policy driver) |
| T2 — automated on-device | Reference-device runs: synthetic pointer injection, frame capture, screenshot diff. | U-01..U-11, I-01..I-08, A-01..A-09, A-12, A-13, P-01..P-09, P-11 |
| T3 — manual playtest scripts | Human sessions (§4) re-confirming tier-1/2 IDs by eye and hand. | U-01..U-11, I-01..I-08, A-01..A-09 re-confirmed in sessions M-1..M-7 |

R-01..R-15 are re-run rules, not standalone assertions: the T1 driver maps a
change category to the risk and re-executes the full test list named under it.

## 3. Pass/fail mechanics

- **Pass.** The assertion holds at every sample point of the tier's sample set
  (§7 defines density), with zero violations and no tolerance beyond what A-013
  states. Bitwise tests (S-01, R-10) are exact byte equality; "never exceeds"
  tests are exact comparisons.
- **Fail.** Any single violating sample fails the test. Fails are filed to the
  owner of the broken contract, never to QA:

  | Broken assertion | Filed to |
  | --- | --- |
  | B-01..B-17, A-11 | A-007 balance |
  | Level content, hazard budgets, endless tiers | A-008/A-009 content |
  | U-*, I-*, A-01..A-09 | A-010/A-011/A-012 design |
  | S-*, P-*, determinism mechanics | A-005 architecture / programmer |

- **No-weakening rule.** A failing test is never made to pass by loosening a
  threshold, shrinking a sample set, skipping a device class, or renumbering a
  canonical number. If a fix would require weakening an A-013 assertion, that
  change is a design bug, not a bug, and QA blocks it.
- **Bug vs design-bug routing.** Bug = implementation disagrees with the
  approved documents (fix in code, re-run the test). Design-bug = the documents
  themselves disagree (file to the owner above; QA changes nothing).
- **Result record.** Every run logs: test ID, tier, device class, seed, input
  digest, pass/fail, first violating sample, and filing owner.

## 4. Manual QA checklist — scripted playtest sessions

Sessions are scripted: fixed seed, fixed step list, expected observation per
step. A session fails if any step's observation deviates from the script.

| Session | Covers | Script |
| --- | --- | --- |
| M-1 small-phone input | I-01, I-05, I-06, I-10, U-01..U-04 | On the smallest frame: drag through every ring zone, joystick both thumbs, tilt to safe-neutral, pause with one thumb; verify pause cancels intent. |
| M-2 large-phone input | I-02, I-03, I-04, I-07, I-08 | On the largest frame: same intents as M-1; palm rejection while resting; multi-touch must not steer; sub-8 ru jitter suppressed. |
| M-3 pause & safe areas | I-05, I-06, U-05, U-06, U-07 | Pause button 64 ru + 12 ru clearance inside safe area on all three frames; UI fits safe areas; indicator cap 8 visible. |
| M-4 accessibility walkthrough | A-01..A-09, U-09, U-10 | Toggle each accessibility option alone, then all together; confirm stage identity, reduced-motion equivalence, flicker-vs-dashed, stroke margin, palette purity, 72 ru pair, one-handed tilt default, pause confirm-state. |
| M-5 SL-1 session | A-01..A-04, U-10 | Color off + motion frozen + audio muted at the same time; play one full level (fixed seed); every state must remain legible; screenshot at 10 checkpoints. |
| M-6 no-softening by hand | A-10, A-12 | Play one level, record digest; replay with every accessibility setting changed; digest must be byte-identical (machine-checked; the human pass is confirmation only). |
| M-7 larger controls | A-13, U-08 | Larger-controls mode: UI must not rescale the playfield; reflow coverage intact. |

## 5. Device testing — reference device classes

PLAN §5.16 says "modern phones" and "weak devices" without naming hardware; A-014
picks and records the reference set so P-01/P-02 are reproducible (A-013 Q3).

| Class | Reference | Used for |
| --- | --- | --- |
| Modern phone | Mid-range Android (2023–2025 class, ≥8 GB RAM, 60 Hz+ display) plus one current-gen iOS device | P-01 60 fps, P-03 no shader spikes, P-08 draw calls, I-02 drag on largest frame |
| Weak device | Oldest still-shippable Android (≤2 GB RAM, throttled CPU) | P-02 30 fps fallback via time scale (never DT), P-04 flat memory, P-05 pool-only steady state |
| Smallest frame | 320×568 logical | U-01..U-04 ring-zone exclusion, I-01 drag, safe areas |
| Mid frame | 390×844 logical | Third sampling point for all U-* layout tests |
| Largest frame | 430×932 logical | I-02, U-01..U-04, reflow coverage |

Recorded result format (one row per test per device):
`device-class, os-build, test-ID, metric, measured, bound, pass/fail, timestamp, build-hash`
Result rows are committed by the harness to `docs/qa-results/`; QA records the
format, not the files. A device class with no recorded row counts as a fail.

## 6. Performance testing

Measurement windows: 120 s steady-state per test after a 10 s warm-up; report
p50/p95/p99.9 frame time and worst hitch. Targets: 16.6 ms p95 on modern phones
(P-01), 33.3 ms p95 on weak devices via time scale (P-02).

| Test | Procedure |
| --- | --- |
| P-01 | Modern phone, level 40 peak load, 120 s; p95 frame ≤ 16.6 ms, no hitch > 50 ms. |
| P-02 | Weak device; time-scale fallback engaged; sim result identical at scaled budget; 33.3 ms window; DT never changes. |
| P-03 | Shader compile/warm-up sweep over every material; no frame > 2× p50 during first use. |
| P-04 | Memory: sample RSS every 5 s over 10 min endless play; flatness = max−min ≤ 2 MB after warm-up; sawtooth growth = fail (leak). |
| P-05 | Steady state allocates from pools only; per-frame allocation counters must read 0 after warm-up (nonzero = leak). |
| P-06 | Particle cap: count live particles at worst bloom; must not exceed the A-013 cap. |
| P-07 | Culling and grid: offscreen entities not drawn; spatial hash cell counts match S-13 expectations. |
| P-08 | Draw calls ≤ A-013 bound with batched layers (U-10) active. |
| P-09 | LOD switch observed at distance; digest unchanged (cross-check with S-05). |
| P-10 | Same seed/digest run on two device classes; final digest byte-equal (also T1 headless). |
| P-11 | Hitch test: inject a 40 ms render hitch; sim digest unchanged, catch-up ≤ 4 steps, drop is render-only. |

## 7. Simulation and balance automation

**Golden replay set (R-10)** — A-014's decision (A-013 Q4):
- Seeds: `GOLDEN = {1, 2, 3, 7, 42, 1337, 900000007, 2^31−1}` (8 seeds).
- Per seed, 3 input digests: idle-drift, scripted level-1 completion (M-1
  transcript), and a scripted endless 10-minute run — 24 replays total.
- Storage: `replays/<seed>-<digest8>.json` holding seed, inputDigest (hex), sim
  build hash, final state digest (hex), assertion digest (hex). The harness
  checks these in; QA records only the digests.
- Policy: after **every change of any kind, including UI-only**, re-run all 24
  replays and byte-compare final digests. Any mismatch fails the merge gate.

**Sampling density** (B-02, B-06, B-07, U-01 "at every instant / any zoom") —
A-014's decision (A-013 Q2):
- Sim-time assertions: every sim tick (60 Hz) for the full scripted run — exact,
  no sampling loss within the run.
- Zoom/layout: 12 zoom values — {min, 1.0, max} × 4 insets — plus the exact
  s = 1/0.82/max-inset boundaries from U-01 sampled exactly.
- Stated limits, honestly: assertions hold for every tick of the scripted runs
  and the named zoom set; they are **not** proven for unscripted input. 100 fuzz
  seeds × 60 s extend coverage but never replace the scripted set.

**R-01..R-15 re-run policy.** After a change, re-run the full test list named
under each risk — not only tests near the edit. CI driver mapping: economy →
R-01 → B-*; upgrades → R-02; hazards → R-03; modifiers → R-04; recipes → R-05;
bloom → R-06; UI tiers → R-07; a11y toggles → R-08; palettes → R-09; **any
change → R-10** (golden replays); pool order → R-11; time scale → R-12;
interpolation → R-13; tension erosion → R-14; SL-1 erosion → R-15. Unknown
change category = re-run everything.

## 8. Store readiness checklist

Store content is release-owned; A-014 only points. Nothing here is decided by
QA (PLAN §5.17 Store category maps to these artifacts).

| Store item (PLAN §5.17) | Owner | Ready when |
| --- | --- | --- |
| Screenshots clear | A-015 store | Listing assets approved |
| Trailer short | A-015 store | Trailer shipped |
| Age rating correct | A-015 store | Rating declared in listing |
| Privacy policy exists | A-017 privacy-policy | Policy page live |
| IAP works | A-016 monetization | IAP flow tested at ship |
| Ads work | A-016 monetization | Ad fill/test parity tested at ship |
| Analytics works | A-016 monetization | Event schema verified |
| Crash reporting works | A-020 ship | Crash report received from a forced crash |

QA's contribution: the build under review must pass the release gate (§9)
before any store item may be marked ready.

## 9. Merge gate

**Merge gate (every feature branch into develop).** In order; a fail at any
stage blocks the merge:
1. Determinism core: S-01, S-05, S-09, S-17, I-09, I-11, P-10, R-10 (golden
   replays byte-equal).
2. No-softening machine checks: A-10, A-11.
3. SL-1 sample: A-01..A-04.
4. Canonical-number comparison: B-01..B-17 (compare, never renumber).
5. Remaining tier-1 set green; tier-2 green on every reference device class;
   tier-3 sessions M-1..M-7 completed with notes.
6. R-01..R-15 re-run for the change category green.

**Release gate (shipping, read by A-015).** All merge-gate stages green on the
ship build, plus P-01/P-02 on the named device classes, P-04/P-05 memory
flatness, and the §8 store checklist fully owned and marked ready by the
release artifacts. Monetization items (IAP/ads) never block core-game
completion; they block only ship.

## 10. Open questions and conflicts

For A-015 (store):
1. Does the store submission plan consume the release gate (§9) as-is, or does
   it add store-side gates? A-014 exposes the gate; A-015 may extend, not
   weaken, it.
2. Age-rating evidence: does the listing need QA's SL-1 screenshots (M-5) as
   proof for rating claims? A-015 decides.

For A-019 (prototype scaffold) and programmer — needs, not design:
3. Harness hooks: headless sim entry (seed, inputDigest → final digest),
   snapshot serialization for golden replays, inputDigest encoding stable
   across frame rates (I-11), synthetic pointer injection for T2, frame-time
   and RSS counters, per-frame allocation counters (P-05).
4. Weak-device simulation switch: P-02 needs the time-scale knob exposed in the
   build under test.

Upstream conflicts flagged (not resolved here):
- Endless tier boundaries 600,000/2,000,000 and the 55% mass-threshold are
  A-009's, unvalidated per A-013; B-* tests touching them report
  "unvalidated-parameter" rather than pass/fail until A-009 validates.
  3,000/120,000 are A-007's and must not move.
- No conflict found between this checklist and A-013; A-014 adds no assertion
  and weakens none.
