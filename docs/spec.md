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

# Event Horizon — Game Specification (A-004)

This is the high-level game specification for Event Horizon. It is the canonical
description of what the game is, how the player interacts with it, what the player
is trying to achieve, and how the player can lose. It deliberately does **not**
define technical implementation (A-005 `docs/architecture.md`), game feel and
presentation (A-006 `docs/design.md`), or numeric tuning and economy (A-007
`docs/balance.md`). Those artifacts answer the open questions listed in Section 17.

Every rule in this document is derived from `PLAN.md`. If this document and
`PLAN.md` ever disagree, `PLAN.md` is the source of truth and must be updated
first. Where `PLAN.md` is silent, this document marks the item as a **proposal**
and flags it as an open question rather than a decision.

## 1. Identity and scope

| Field | Value |
| --- | --- |
| Artifact ID | A-004 |
| File | `docs/spec.md` |
| Agent | planner |
| Depends on | A-001 `PLAN.md` (done) |
| Consumed by | A-005 `docs/architecture.md`, A-006 `docs/design.md`, A-013 `docs/test-plan.md`, A-019 `docs/prototype-scaffold.md` |
| Scope | Core loop, controls, camera, objectives, failure conditions, player experience, stat model shape, progression shape |

Acceptance criteria for A-004 (PLAN.md section 2): "Defines core loop, controls,
goals, failure conditions." Contract (PLAN.md section 4): "High-level game
specification. Defines core loop, controls, objectives, failure conditions, and
player experience."

## 2. Overview and design intent

### 2.1 What the game is

The player controls a small black hole moving through space. The black hole
attracts nearby bodies with a gravitational field. Bodies that reach the black
hole are absorbed, increasing its mass, which in turn increases its event
horizon and gravitational pull (PLAN 5.1). Larger objects become absorbable
only as the black hole grows.

The core fantasy chain, quoted from PLAN 5.1:

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

### 2.2 Design intent: tension, not power fantasy

PLAN section 8 is a hard rule, and this spec restates it as a constraint on
every later artifact:

> Do not make the game a pure power fantasy. More mass should create tension:

```text
more mass = stronger pull
more mass = larger collision risk
more mass = slower precise movement
more mass = harder navigation
more mass = stability may decay
```

This gives players real decisions (PLAN 5.12 wording in section 8):

```text
Grow fast and risk instability?
Stay smaller and move precisely?
Upgrade for efficiency or pull?
Absorb large objects or avoid hazards?
```

Spec-level consequence: the player's biggest growth moments are also the
player's most dangerous moments. No mechanic may be added later that removes
this tradeoff outright (for example, an upgrade that makes large mass free of
collision risk or stability cost). Balance (A-007) tunes the tradeoff; it may
not delete it.

### 2.3 What this spec fixes vs. defers

- **Fixed here:** the loop structure, the control scheme, the camera contract,
  the objective and failure sets, the stat list, the shape of the growth
  formulas, and the scope boundaries below.
- **Deferred:** every constant, curve, level layout, visual style, HUD layout,
  and technical structure — see Section 15.

## 3. Core loop

### 3.1 Minute-to-minute loop, as discrete steps

The loop a player performs minute to minute is exactly this, repeated:

```text
1. Scan      Read the field: which bodies can the current pull move, and
             which are dangerous to touch at the current size.
2. Choose    Pick one target (or one avoidance) and a route. This is the
             decision point: grow fast vs. stay precise (Section 2.2).
3. Position  Drag the black hole so the target sits inside the field and
             the route stays clear of hazards.
4. Attract   The field applies force; the target drifts toward the black
             hole while the player keeps steering.
5. Absorb    When the target crosses the absorption radius, it is consumed
             and massGain = bodyMass * efficiency is added (PLAN 5.6).
6. Grow      Mass increases; event horizon and pull strength increase
             (PLAN 5.6, 5.10); speed decreases slightly (PLAN 5.10).
7. Reassess  Objects that were unmovable are now in range; objects that
             were safe may now be too close to the bigger profile. The
             loop repeats at a larger scale.
```

Steps 1–2 are the decision; steps 3–5 are execution; steps 6–7 are the
escalation that makes the next decision harder. The loop never changes shape
during a run — only its scale and danger change, which is what keeps the
tension rule of Section 2.2 true at every size.

### 3.2 Loop nesting

The same loop nests at four layers. The durations below are **proposals**
(PLAN.md is silent on pacing numbers); they are flagged in Section 17.

| Layer | Duration (proposal) | What the player is doing |
| --- | --- | --- |
| Micro loop | 1–5 s | Attract and absorb one body or cluster |
| Meso loop | 20–60 s | Chain absorptions, build combo, clear a region |
| Macro loop | one level | Reach the level goal before failure conditions trigger |
| Meta loop | one session | Finish levels, spend permanent upgrades, unlock worlds |

Combo exists as a stat (PLAN 5.4: "Streak bonus for chaining absorptions"), so
the meso layer is a real layer, but the combo window length is owned by A-007.

### 3.3 Level loop

```text
Level start
  -> field overview (the level's opening state is readable at a glance)
  -> play loop (Section 3.1, repeated until win or failure)
  -> win: primary goal met before the time limit expires (Section 6)
  -> lose: any failure condition in Section 7 is met
  -> result screen: outcome, mass vs. goal, time, stability, combo
  -> upgrade / reward step (Section 11; contents owned by A-007 and A-009)
  -> retry or advance
```

### 3.4 Session loop

```text
Campaign map (PLAN 5.12 worlds)
  -> select level
  -> level loop
  -> permanent upgrade choice (PLAN 5.9 meta-progression; values owned by A-007)
  -> unlock next level or world (structure owned by A-009)
  -> optional Endless / Daily / Weekly challenge (PLAN 5.13; owned by A-009)
```

## 4. Controls

### 4.1 Default control scheme

Default mobile control is **direct drag** (PLAN 5.2):

- The player drags the black hole across the screen with one finger.
- The camera follows the black hole (Section 5).
- Dragging translates the black hole; there is no separate "attract" input.
  The gravitational field is always on. Absorption is positional: the player
  absorbs by moving. (The absence of an absorb button is a **proposal**
  derived from PLAN 5.2 listing only movement controls; flagged in Section 17.)

### 4.2 Alternative controls

PLAN 5.2 recommends:

```text
Default: drag
Alternative: virtual joystick
Alternative: device tilt
```

| Control | What each input does | Notes |
| --- | --- | --- |
| Drag (default) | Finger drag translates the black hole directly | One finger; occlusion is the known tradeoff |
| Virtual joystick | Stick displacement translates the black hole | For players who dislike finger-over-target occlusion |
| Device tilt | Tilting steers the black hole | One-handed / accessibility option |

Alternatives are selectable in settings. Touch handling details, dead zones,
gesture disambiguation, and accessibility variants belong to A-011
`docs/input.md` and A-012 `docs/accessibility.md`.

### 4.3 Input verbs

The full verb list is deliberately small — skill comes from positioning,
timing, and route choice, not from button combos:

| Verb | Input | Effect |
| --- | --- | --- |
| Move | Drag / joystick / tilt | Translate the black hole in the play field |
| Hold | No input | Black hole stays; the field keeps pulling (enables baiting) |
| Retreat | Drag away from a threat | Movement manages hazard risk and profile size |
| Line up | Move so several bodies drift on one bearing | Chain absorptions and combos (vocabulary proposal, Section 17) |

There is no jump, dash, fire, aim, or absorb button in the default scheme.
Any later ability-like mechanic (for example Quantum phase-shift, PLAN 5.5)
must be specified as content in A-008 and may not turn the control scheme
into an action game.

### 4.4 Gestures

- One-finger drag is the only required gesture.
- Pinch/rotate are **not** player camera controls: camera zoom is automatic
  per the contract in Section 5 (PLAN 5.3). Reserving pinch/rotate for settings
  only is a **proposal** flagged in Section 17.
- No multi-finger gesture is required for core play (accessibility rule;
  detailed gesture handling owned by A-011).

## 5. Camera and screen space

### 5.1 Camera contract (PLAN 5.3)

```text
camera follows black hole
zoom = f(event horizon radius, mass)
```

As the black hole grows:

- Zoom out smoothly.
- Show more field.
- Keep the event horizon visible.
- Keep the mass meter visible.

The camera is never player-controlled in the default scheme. The black hole
is the camera anchor at all times.

### 5.2 Screen space rules

- The play field is larger than the viewport; the camera window moves with
  the black hole. (Field extent per level is owned by A-009.)
- The event horizon ring must remain on screen at all times — the camera
  contract guarantees the player always sees "what is mine." If growth would
  push the horizon off-screen, the zoom function must compensate (the exact
  function is owned by A-005/A-006/A-007).
- The mass meter (HUD element showing progress toward the goal) must remain
  visible; its layout is owned by A-010 `docs/ui.md`.
- Bodies outside the viewport: whether off-screen bodies get edge indicators
  is a **proposal** left open; owned by A-006 (visual language) and A-010
  (HUD). Flagged in Section 17.
- Readability target: at any zoom, the player can distinguish, at a glance,
  bodies inside the event horizon from bodies outside it, and hazards from
  absorbable bodies. How this is drawn is owned by A-006.

## 6. Objectives and win/lose conditions per run

### 6.1 Primary objective

Reach the level's target mass before the time limit expires. This combines
PLAN 5.8's "Reach target mass" goal with the recommended failure pressure of
PLAN 5.11 ("Time limit + stability + hazard pressure"). The pairing of
target-mass + timer as the *default* primary goal is a **proposal** (PLAN 5.8
lists goals as examples); flagged in Section 17.

### 6.2 Goal types (PLAN 5.8)

```text
Reach target mass.
Survive time.
Maintain combo.
Avoid hazards.
Absorb anomalies.
Use efficiency challenge.
```

A level may combine one primary goal with at most one secondary goal. The
secondary goal grants an extra reward and never replaces the primary goal.
Which levels use which goals, and what the rewards are, is owned by A-009
(rewards economy owned by A-007).

### 6.3 Win condition

The run wins when the primary goal is satisfied before any failure condition
fires. On win, the run ends immediately; play does not continue past the goal
(the result screen shows the surplus).

### 6.4 Lose condition

The run loses the moment any one failure condition in Section 7 fires. There
is no partial-credit system: one mistake does not lose the run, but the
failure conditions themselves are cumulative resources (stability drains), so
pressure accumulates without sudden single-hit losses (hazard damage values
owned by A-007/A-008).

## 7. Failure conditions and how they create decisions

### 7.1 Possible conditions (PLAN 5.11)

```text
Time limit.
Stability reaches zero.
Too many hazard collisions.
Mass drops below threshold.
```

### 7.2 Canonical set (PLAN 5.11 recommendation)

```text
Time limit + stability + hazard pressure
```

Mapping of the three canonical conditions:

| Condition | How it fails | Player response (the decision it creates) |
| --- | --- | --- |
| Time limit | Target mass not reached before the timer expires | Take riskier, higher-value routes or invest in pull/efficiency upgrades |
| Stability | Stability resource reaches zero | Hazard contact and oversized absorptions drain stability; the player must decide whether to keep growing or hold a smaller, safer profile |
| Hazard pressure | Hazard collisions drain stability (and/or mass; exact damage owned by A-007/A-008) | Route planning, retreat, and shrinking the profile |

"Mass drops below threshold" is **not** in the canonical set. It may appear
as a modifier on specific challenge levels (proposal; owned by A-009). Flagged
in Section 17.

### 7.3 Failure creates decisions, not punishment

Failure conditions exist to force the tension decisions of Section 2.2, not
to punish the player:

- Every failure condition is visible on the HUD as a resource with headroom
  (timer, stability bar) — the player can always see *how close* they are to
  losing, so they can choose to press on or play safe.
- Stability is the decision resource: growing bigger and faster is how the
  player spends stability pressure; playing precise conserves it. The
  player is choosing a risk dial, not rolling on luck.
- No failure may be caused by an unavoidable hazard spawn (a hazard must be
  readable before it is dangerous — readability presentation owned by A-006).
- Retry is immediate and free: no lives system, no currency cost, no ad or
  IAP may gate a retry or a level. This is consistent with PLAN 5.14
  ("No pay-to-win", ads only at game over / retry / between levels) and the
  hard rule that monetization must not block core game completion. The
  "retry is free" wording itself is a **proposal** flagged in Section 17.

### 7.4 Failure presentation

Failure must be readable in one glance on the result screen:

- The failing condition is named (time, stability, or — in variant modes —
  mass).
- The result screen shows the gap to the goal (mass short, time left,
  stability remaining).
- Visual and audio language of failure is owned by A-006; HUD layout by A-010.

## 8. Player experience

### 8.1 Feel targets (what the player should feel)

- **Small and curious** at level start: "what can my field pull?"
- **In control** while absorbing: every drag changes what gets pulled in.
- **Increasingly large** as the field visibly reclaims the screen.
- **Under pressure** as mass grows: bigger means slower, riskier, less
  precise (PLAN section 8).
- **Rewarded** for chaining absorptions and choosing routes, not for mashing
  input (there is nothing to mash — Section 4.3).

### 8.2 Pacing targets

- The micro loop must resolve in a few seconds so growth is visible
  continuously (numbers are proposals, Section 3.2 / Section 17).
- The player must always have a readable decision, one of:

```text
Grow fast and risk instability?
Stay smaller and move precisely?
Upgrade for efficiency or pull?
Absorb large objects or avoid hazards?
```

- Tension peaks at the moment of largest growth: the biggest absorb is also
  the most dangerous maneuver. This is the emotional target of the design
  rule, and A-006 (feedback) and A-007 (tuning) must both serve it.

### 8.3 Readability on mobile

- One decision at a time on screen: the field, the horizon ring, and the
  goal meter are the only always-read elements.
- The player reads the game through the horizon ring: "inside = mine."
  Everything else is communicated by A-006's visual language.
- Text is minimal; the tutorial teaches by play, not text walls
  (proposal; flagged in Section 17).
- All of this must survive the smallest supported phone; concrete UI
  scaling is owned by A-010.

## 9. Black hole stats and how they change during a run

### 9.1 Stat list (PLAN 5.4)

| Stat | Meaning | How it changes during a run |
| --- | --- | --- |
| Mass | Main progression stat | Grows on every absorb: `massGain = bodyMass * efficiency` (PLAN 5.6) |
| Event Horizon Radius | Visual gravitational boundary | Grows sublinearly: `eventRadius = baseRadius + a * sqrt(mass)` (PLAN 5.6) |
| Pull Strength | Gravitational attraction | Grows sublinearly: `pullStrength = pullScale * mass^0.75` (PLAN 5.6) |
| Absorption Radius | Distance required to absorb | Grows with the profile (linkage to horizon/mass owned by A-007) |
| Speed | Player-controlled movement speed | Decreases slightly as mass grows (PLAN 5.10) |
| Stability | Prevents runaway growth or overload | Drains under hazard pressure and oversized absorptions; failure at zero (PLAN 5.11) |
| Efficiency | How much absorbed mass becomes useful mass | Raised by upgrades and clean play (values owned by A-007) |
| Combo | Streak bonus for chaining absorptions | Grows with consecutive absorptions; decay rules owned by A-007 |

### 9.2 Growth model shape (PLAN 5.6, arcade physics not real astrophysics)

```text
massGain      = bodyMass * efficiency
eventRadius   = baseRadius + a * sqrt(mass)
pullStrength  = pullScale * mass^0.75
```

Attraction (PLAN 5.6):

```text
direction       = blackHole.pos - body.pos
distanceSquared = direction.lengthSquared() + epsilon
force           = pullStrength * bodyMass / distanceSquared
body.velocity  += direction.normalized() * force * deltaTime
```

Absorption (PLAN 5.6):

```text
if distance(body, blackHole) < absorptionRadius:
    absorb(body)
```

This spec fixes the **shape** of the model: mass gain is direct, the horizon
grows with `sqrt(mass)`, pull grows with `mass^0.75`, attraction force scales
with body mass and falls off with squared distance. The constants `a`,
`pullScale`, `epsilon`, base radii, and every threshold belong to A-007.
The simulation structure (fixed timestep, spatial hashing, body counts)
belongs to A-005 (PLAN 5.16 lists the performance tools; choosing them is
A-005's job).

### 9.3 How stats change during a run (narrative)

Early run: small horizon, weak pull — only Small bodies (PLAN 5.7) move; the
player steers precisely; stability is at headroom; speed is at maximum.

Mid run: horizon reclaims more screen; Medium bodies start drifting; the
profile is now large enough that hazard contact matters; the first real
grow-or-stay-small decision appears.

Late run: pull reaches Large bodies; speed is reduced; each additional absorb
is riskier (larger collision risk, stability may decay — PLAN section 8).
The endgame of a run is the tension phase, not a free-fantasy phase.

## 10. Starting black hole types and how they differ

Two starting types express the two poles of the Section 2.2 tension (PLAN 5.5).

### 10.1 Star Mass Black Hole

Theme: stable, reliable, slow-growth.

- Stable mass growth, reliable pull, better absorption efficiency, lower
  volatility, good for beginners (PLAN 5.5).

```text
Mass:       stable
Pull:       medium
Efficiency: high
Stability:  high
Speed:      medium
```

### 10.2 Quantum Black Hole

Theme: unstable, high-power, high-risk.

- Stronger pull, faster growth, more volatile, can emit quantum particles,
  can phase-shift absorption modes, can chain-react with anomalies, higher
  risk of instability (PLAN 5.5).

```text
Mass:       volatile
Pull:       high
Efficiency: medium
Stability:  low
Speed:      high
```

### 10.3 Spec rule for types

Neither type may be strictly better (proposal derived from PLAN 5.17 QA:
"Quantum mode not too strong. Star mode not too weak."). Star Mass must win
on precision and long-session reliability; Quantum must win on burst growth
and anomaly interaction. Exact values and volatility mechanics are owned by
A-007 (values) and A-008 (quantum particle, phase-shift, and chain-react
mechanics as content).

## 11. Progression: what grows over a run and over the campaign

### 11.1 Within a run

Grows: mass, event horizon radius, pull strength, absorption radius, combo
streak, and (via temporary run upgrades) stat modifiers.
Shrinks: movement speed (PLAN 5.10), and stability under pressure (PLAN 5.11
/ section 8).

Temporary run upgrades (PLAN 5.9, reference values tuned in A-007):

```text
+15% pull strength
+10% absorption radius
+10% movement speed
+10% mass efficiency
+1 quantum anomaly spawn
```

Spec rules for run upgrades (proposals derived from PLAN 5.9's two-system
requirement; flagged in Section 17): choices must be meaningful, not strictly
additive — a player must be able to build pull-heavy or efficiency-heavy;
active temporary upgrades must be visible on the HUD while active.

### 11.2 Across the campaign

Permanent meta-progression (PLAN 5.9) uses the named set:

```text
Gravity Lens
Accretion Core
Horizon Stabilizer
Inertial Dampener
Combo Anchor
Quantum Core
Star Core
```

This spec does not assign effects to these upgrades; effects and costs are
owned by A-007 (values) and A-008 (definitions). Permanent upgrades must be
reachable through play; no upgrade may require purchase (PLAN 5.14 "No
pay-to-win").

Progression curve (PLAN 5.10): diminishing returns — mass gain direct,
horizon `sqrt(mass)`, pull `mass^0.75`, speed decreasing. Reference target
masses, quoted from PLAN 5.10 (these are PLAN's fixed reference points, not
final tuning):

```text
Level 1: 1,000
Level 5: 10,000
Level 10: 50,000
Level 20: 250,000
Level 40: 1,000,000
```

Campaign structure: five worlds, each teaching one escalation (PLAN 5.12):

```text
World 1: Dust Belt        teaches movement, attraction, absorption
World 2: Asteroid Field   teaches larger objects and clustering
World 3: Ship Graveyard   teaches high-value targets and path planning
World 4: Planet Ring      teaches large mass and stability management
World 5: Quantum Nebula   teaches quantum mode, phase shifts, chain reactions
```

World-level layout, count (PLAN 5.18 Phase 4 says 30–60 levels), unlock
system, and the difficulty curve are owned by A-009.

### 11.3 Modes (PLAN 5.13)

| Mode | Spec rule | Owner of details |
| --- | --- | --- |
| Campaign | Must be completable without any monetization | A-009 |
| Endless | One long run with escalating pressure; must end on a failure condition, must not stall (PLAN 5.17) | A-009 |
| Daily challenge | Shared daily goal | A-009 |
| Weekly challenge | Longer shared goal | A-009 |
| Leaderboards | Compare score in shared modes | A-009 (scoring), A-005 (seed reproducibility, proposal) |

## 12. Object categories (overview only)

The spec names categories only; the full object list, values, and behaviors
are defined in A-008 `docs/content.md`. Categories quoted from PLAN 5.7:

| Category | Examples (PLAN 5.7) | Spec rule |
| --- | --- | --- |
| Small | Dust, pebbles, ice fragments, small debris | Absorbable at level start |
| Medium | Rocks, metal shards, ship fragments, asteroids | Require mid-run growth |
| Large | Asteroid chunks, small planets, star fragments, quantum anomalies | Require late-run growth; high value |
| Hazards | Mines, gravity anchors, magnetic fields, collapsing debris, massive objects | Punish careless growth; some may function as tools (open question, A-008) |

Spec rule: hazards must be readable before they are dangerous — a hazard may
never strike without a visible warning state (proposal; presentation owned by
A-006, hazard behavior owned by A-008).

## 13. Upgrade systems (summary; values owned by A-007)

Both systems are required by PLAN 5.9:

1. **Permanent meta-progression** — persistent power across levels (Section
   11.2 named set).
2. **Temporary run upgrades** — choices made inside a level (Section 11.1
   reference values).

The spec-level rule is the tension rule applied to upgrades: an upgrade may
shift the dial between "grow fast" and "stay precise," but may not switch
off the tension of Section 2.2.

## 14. Failure of the tutorial and first-time-player experience

- The first level must be completable using the default drag control only,
  with no upgrade choices required (derived from PLAN 5.17 QA "Level 1
  completable" and 5.12 World 1 teaching scope). The specific time budget
  for clearing level 1 is **not** fixed here (the prior draft claimed 60 s;
  removed — see Deviations). Owned by A-009/A-007.
- World 1 introduces, in order: move, attract, absorb (PLAN 5.12). Baiting
  (Hold + drift) is available from the start but never required.
- Teaching through play, not text walls, is a proposal flagged in Section 17.

## 15. Scope boundaries: what is NOT specified here

Each open item names the artifact that owns it, so downstream agents know
where to answer it:

| Topic (not answered here) | Owning artifact |
| --- | --- |
| Modules, simulation loop, fixed timestep, data structures, spatial hashing, object pooling, platform/tech-stack choice | A-005 `docs/architecture.md` |
| Visual language, event-horizon rendering, particles, sound, camera curves, screen shake, feedback juice, readability styling | A-006 `docs/design.md` |
| All constants (`a`, `pullScale`, `epsilon`), base radii, thresholds, stability budgets, combo window, timers, upgrade values, economy, difficulty targets, target-mass schedule | A-007 `docs/balance.md` |
| Full object list, hazard behaviors (which hazards are tools), modifiers, rewards, quantum particle/phase-shift/chain-react mechanics | A-008 `docs/content.md` |
| World and level layouts, level goals per level, campaign structure and unlock system, endless/daily/weekly rules, leaderboard scoring | A-009 `docs/levels.md` |
| HUD layout, menus, settings, buttons, mobile scaling, safe areas, mass meter and stability bar placement | A-010 `docs/ui.md` |
| Touch handling, dead zones, gesture disambiguation, joystick/tilt implementation details | A-011 `docs/input.md` |
| Reduced motion, high contrast, larger controls, colorblind-safe palette, alternative controls | A-012 `docs/accessibility.md` |
| Testing strategy, simulation/balance/perf tests | A-013 `docs/test-plan.md` |
| Monetization placement and fairness | A-016 `docs/monetization.md` (rules anchored by PLAN 5.14) |

## 16. Deviations from prior art

A previous out-of-order draft of this artifact exists on branch
`archive/out-of-order-planner-spec` (superseded per PROGRESS.md). This
version reuses its structure but corrects places where it stated decisions
`PLAN.md` does not fix. Deviations from that draft:

1. **Pacing numbers demoted to proposals.** The prior draft set micro/meso
   loop durations and a "first level under 60 seconds" claim as decisions.
   PLAN.md fixes no pacing numbers; these are now labeled proposals and
   flagged as open questions (owners: A-006 feel, A-007 balance, A-009
   levels).
2. **Upgrade effects removed.** The prior draft claimed Horizon Stabilizer
   and Inertial Dampener restore stability. PLAN 5.9 only names upgrades;
   effects are now deferred to A-007/A-008.
3. **Reward vocabulary removed.** The prior draft said secondary goals grant
   "stars, currency, upgrade tokens." PLAN.md defines no economy; the text
   now says "extra reward" and defers the economy to A-007 and rewards to
   A-008/A-009.
4. **Seed-reproducible leaderboard scoring demoted.** The prior draft stated
   scores must be reproducible from a seed as a decision; now a proposal
   owned by A-005/A-009.
5. **Camera moved to its own section.** The artifact contract requires
   "camera and screen space"; the prior draft buried the camera contract
   under controls. Section 5 now covers camera and screen space explicitly.
6. **Minute-to-minute loop restated as seven discrete steps.** The prior
   draft's six-step loop merged decision and execution; the new Section 3.1
   separates Scan/Choose (decision) from Position/Attract/Absorb (execution)
   to make the tension decision explicit per the acceptance criteria.
7. **Free-retry and no-lives wording kept but labeled a proposal**, anchored
   to PLAN 5.14's "No pay-to-win" and ad-placement rules rather than stated
   as a PLAN decision.
8. **Mass-below-threshold failure kept out of the canonical set** (as in the
   prior draft) but explicitly labeled a PLAN 5.11 "possible" variant owned
   by A-009 rather than a spec decision.

No deviation contradicts PLAN.md; all are demotions of claims PLAN.md does
not make.

## 17. Open questions for downstream artifacts

Proposals flagged above are collected here with their owners:

1. **A-005:** How is the field simulated — per-body force integration vs.
   field texture — and what fixed timestep governs absorption checks (PLAN
   5.16 requires fixed timestep; the value is A-005's)?
2. **A-005:** Maximum simultaneous body count and how the "readable field"
   rule of Section 5.2 survives it (spatial hashing and culling are named in
   PLAN 5.16; choosing them is A-005's).
3. **A-005:** Is leaderboard scoring reproducible from a seed (proposal in
   Section 11.3)?
4. **A-006:** How is the event horizon drawn so "mine" vs "not yet mine" is
   readable at a glance on the smallest phone?
5. **A-006:** How is stability loss communicated without relying on color
   alone (accessibility anchor for A-012)?
6. **A-006:** Do off-screen bodies get edge indicators (proposal in Section
   5.2)?
7. **A-006:** Pacing feel targets: confirm or replace the micro/meso duration
   proposals of Section 3.2.
8. **A-007:** Exact stability budget per level; how much does each hazard hit
   and each oversized absorption cost?
9. **A-007:** Combo window length; does combo decay with mass?
10. **A-007:** Is target-mass + timer the default primary goal for campaign
    levels (proposal in Section 6.1), and what are the per-level timers?
11. **A-007:** Effects and costs of the seven permanent upgrades (PLAN 5.9
    names only); do any of them touch stability, and how without violating
    the tension rule of Section 2.2?
12. **A-008:** Which hazards are tools rather than pure punishment (gravity
    anchors, magnetic fields)?
13. **A-008:** Exact mechanics of quantum particles, phase-shift absorption
    modes, and chain reactions (PLAN 5.5 traits).
14. **A-009:** Does any level use the mass-below-threshold failure variant
    (Section 7.2)?
15. **A-009:** Time budget for level 1 and the tutorial teaching order
    (Section 14).
16. **A-010:** Where do the mass meter and stability bar sit within mobile
    safe areas?
17. **A-011:** Does tilt control need a calibration step for accessibility
    (anchor for A-012)?
18. **A-011:** Confirm the no-absorb-button scheme (proposal in Section 4.1)
    and pinch/rotate reservation (proposal in Section 4.4).

## 18. Acceptance criteria check

- [x] Core loop defined as discrete minute-to-minute steps (Section 3.1)
      plus nesting, level, and session loops (3.2–3.4).
- [x] Controls defined: default drag, joystick and tilt alternatives, verbs,
      gestures (Section 4).
- [x] Camera and screen space defined with the PLAN 5.3 contract (Section 5).
- [x] Goals and win/lose conditions per run defined (Section 6).
- [x] Failure conditions defined with the canonical PLAN 5.11 set and the
      decisions-not-punishment rule (Section 7).
- [x] Player experience: feel targets, pacing, mobile readability (Section 8).
- [x] Black hole stats and their in-run changes, with PLAN 5.6 formula shapes
      (Section 9).
- [x] Starting black hole types and their differences (Section 10).
- [x] Progression within a run and across the campaign (Section 11).
- [x] Scope boundaries with owning artifact for every open item (Section 15).
- [x] Open questions flagged with owners (Section 17).
- [x] No design decision contradicts PLAN.md; all demotions listed in
      Section 16.
