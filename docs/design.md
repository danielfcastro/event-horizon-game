# A-006 — docs/design.md

- Owner: designer
- Depends on: A-004 docs/spec.md (done)
- Consumed by: A-007 docs/balance.md, A-008 docs/content.md, A-010 docs/ui.md, A-011 docs/input.md, A-012 docs/accessibility.md
- Status: draft

This document defines game feel and presentation for Event Horizon: camera behavior,
visual language, particles, sound, feedback, and accessibility direction. It answers
the four open questions of spec section 17 owned by A-006 (questions 4-7). It does
not decide balance numbers (A-007), content lists (A-008), level structure (A-009),
HUD layout (A-010), input controls (A-011), or accessibility option implementations
(A-012); it sets the direction and requirements those artifacts must satisfy.

Every rule here is derived from the A-006 brief (PLAN.md excerpts 5.3, 5.4, 5.5,
5.7, 5.16, 5.17, section 8) and the approved spec and architecture excerpts. Where
the brief is silent, the item is marked as a **proposal**. On disagreement PLAN.md
wins.

---

## 1. Design intent

The game is a growth simulator that is **not** a power fantasy (PLAN section 8).
The player's biggest growth moments are also their most dangerous moments (spec
2.2). Every presentation choice in this document serves that rule: the screen
itself must *show* danger escalating as the field grows, so the player feels the
tradeoff without reading numbers.

Feel targets (spec 8.1), expressed as the emotional arc a level must deliver:

| Phase | Feeling | How presentation carries it |
| --- | --- | --- |
| Level start | Small and curious | Wide empty field, thin calm ring, quiet audio; the ring visibly "owns" almost nothing |
| Absorbing | In control | Every drag changes what the ring reclaims; each absorb produces instant micro-feedback |
| Growing | Increasingly large | The ring reclaims more of the screen; camera zooms out; the field darkens around the hole |
| Large mass | Under pressure | Ring degrades, ambient pressure rises, movement reads as heavy; big = risky is *felt* |
| Chaining | Rewarded | Combo streaks get shimmer and rising audio motifs; no reward for mashing (spec 4.3: nothing to mash) |

Pacing rule (spec 8.2): tension peaks at the moment of largest growth. The
biggest absorb is the most dangerous maneuver, and both feedback (this document)
and tuning (A-007) must serve that peak.

## 2. Camera

Contract (PLAN 5.3, spec 5.1):

```text
camera follows black hole
zoom = f(event horizon radius, mass)
```

Decisions:

| Decision | Choice | Rationale |
| --- | --- | --- |
| Anchor | Black hole center, always; never player-controlled in the default scheme | Spec 5.1; the hole is "what is mine" and the camera is its window |
| Zoom | Monotonic smooth zoom-out as the horizon radius grows; no snap zooms on growth | PLAN 5.3 "zoom out smoothly"; continuous visible growth (spec 8.2) |
| Ring guarantee | The zoom function must keep the full event horizon ring on screen with margin; if growth would push it off-screen, zoom compensates | Spec 5.2 hard rule; the exact function is shared A-005/A-006/A-007, and A-007 tunes it under this constraint |
| Ring margin | Ring fits inside the safe area with padding that scales with viewport size; minimum ring stroke 2 px at the smallest supported phone | Readability on the smallest phone (spec 8.3); concrete safe-area math is A-010 |
| Smoothing | Camera position is the hole position with light smoothing (interpolation alpha per A-005 render loop); no directional lookahead offset in the default scheme | "Follows" means centered; any assistive camera offset is an input concern owned by A-011, not a default |
| Punch | A large absorb triggers a brief zoom *punch-in* (never punch-out) plus capped screen shake, then settles back to the contract zoom | Juice without ever hiding the ring; punch-in only adds margin, punch-out could violate the ring guarantee |
| Shake budget | Small, capped, decaying; scales with absorb size ratio; disabled-channel direction for reduced motion is A-012 | Feel without nausea; A-005 keeps sim untouched |

The mass meter must remain visible at all zooms (spec 5.2); its layout is A-010's,
but the camera contract above is what guarantees the meter never competes with an
off-screen horizon.

## 3. Visual language

### 3.1 Readability hierarchy

Readability target (spec 5.2): at any zoom, the player distinguishes at a glance
(a) bodies inside the event horizon from bodies outside it, and (b) hazards from
absorbable bodies. Only three elements are ever read at once (spec 8.3): the
field, the horizon ring, the goal meter. Everything else is communicated by the
visual language below.

Contrast hierarchy, strongest to weakest:

```text
event horizon ring > hazard markers > large bodies > medium bodies > small bodies > field texture
```

This hierarchy must survive the 300-body density cap (A-005 SpawnDirector
`maxPerScreenArea`): readability is carried by silhouette and the ring, not by
counting bodies.

### 3.2 Shape and brightness language

| Element | Language | Rationale |
| --- | --- | --- |
| Black hole | Pure black disc, darkest thing on screen; faint accretion swirl at the rim (visual only) | The hole is the player; nothing may read as darker |
| Event horizon ring | Single bright ring, constant screen-space stroke width (min 2 px), slow calm pulse at low mass | One anchor for "mine"; screen-space width keeps it visible at every zoom |
| Absorbable bodies | Rounded/organic silhouettes; brightness below the ring, above the background | Silhouette says "food" without color reliance |
| Hazards | Sharp/spiky silhouettes plus a marker glyph (bracket/X family), never color-only | Spec 5.2 requires hazard-vs-food readability; shape carries it for colorblind players (anchor for A-012) |
| Inside vs outside | See 3.3 (answers open question 4) | The core readability decision |
| Bodies per world | One texture atlas per world (A-005); palette per world; LOD A near = sprite + glow, B mid = sprite only, C far = 1-4 px point | A-005 LOD contract, not contradicted |

### 3.3 Event horizon rendering — answers open question 4

The ring is drawn by a simple shader on a dedicated draw call (A-005 hook).
"Mine" vs "not yet mine" is readable at a glance through **three redundant
channels**, so no single channel (or color) carries it:

1. **Ring**: bright, constant-width circle; the only perfect circle on screen.
2. **Interior vacuum**: a subtle radial gradient darkening toward the hole — a
   purely visual field layer, decoupled from the sim (allowed by A-005). Inside
   reads as "claimed space."
3. **Motion**: bodies inside the ring already drift and rotate toward the hole
   (this is sim behavior, rendered as-is); bodies outside do not. Inside motion
   is the second-glance confirmation.

At a glance: ring + dark interior + drifting bodies = mine. On the smallest
phone, channels 1 and 2 carry the reading; channel 3 confirms. Faint
gravitational streamlines (field texture, visual only) may be added at high
quality tier; they must never imply a sim effect that does not exist.

### 3.4 Off-screen indicators — answers open question 6

**Decision: yes, but a restricted set.** The RenderLayer `visibilityLayer`
output (A-005) drives edge indicators only for:

- Off-screen bodies **currently being pulled** by the player's field, and
- Off-screen bodies that **threaten the current field** (hazards whose approach
  vector intersects the horizon).

Rationale: the player must know what is arriving from off-screen without
scanning, but a full indicator set would clutter the one-decision-at-a-time
rule (spec 8.3). Indicators are small edge chevrons; absorbable vs hazard is
carried by glyph shape (chevron vs bracketed X), not color. Maximum 8
indicators on screen, priority-sorted by threat/arrival time. Exact placement,
size, and styling within safe areas belong to A-010; the semantics and cap are
A-006's decision.

## 4. Particles and effects

All particles are pooled and budgeted per quality tier (A-005); they are render
only and never feed the sim (A-005 RenderLayer contract).

| Event | Look | High tier | Low tier |
| --- | --- | --- | --- |
| Absorb (small) | Inward stream wisp + 4-8 particle pop, scaled by mass ratio | ≤ 200 concurrent particles total | ≤ 60 concurrent particles total |
| Absorb (large — tension peak) | Ring ripple wave, brighter burst, brief flash, heavier punch | Same events, fewer/larger sprites | Ring ripple only |
| Hazard impact | Spiky burst + ring fracture flicker (see 6.1) | Full | Flicker only |
| Combo step | Ring shimmer + one rising pitch tone (see 5) | Full | Tone only |
| Growth milestone | Low rumble + slow ring brightening | Full | Brightening only |

Rules: simple additive sprites, no shader spikes (PLAN 5.16 / QA plan); particle
counts scale with the absorb's mass ratio so the *largest* absorb is the
loudest visual event — the tension peak of spec 8.2 is the most visible moment
on screen, not a hidden one.

## 5. Sound direction

| Channel | Direction | Rationale |
| --- | --- | --- |
| Ambient | Sparse low-frequency space drone; pressure (low rumble frequency) rises with player mass | The audio mirror of "big = dangerous" (PLAN section 8) |
| Absorb | Pitch-mapped pop: small bodies = high tick, large bodies = deep thump; pitch maps to mass ratio | Audio alone tells the player how big the meal was; readable with eyes elsewhere |
| Stability danger | Creak/tick loop whose **rhythm** tightens as stability decays | Pairs with the ring-fracture anchor (section 6.1); rhythm is a non-color, non-hue channel |
| Combo | Rising motif per streak step | Rewards chaining (spec 8.1), audible without looking |
| Failure | Distinct descending motif per failure type: time = clock-like cadence, stability = shatter/creak | Spec 7.4: the failing condition must be named at a glance; audio names it too |
| Mobile rule | No cue is audio-only and no cue is visual-only | Mute-safe and low-vision-safe; anchor for A-012 |

## 6. Feedback

### 6.1 Stability without color — answers open question 5

Stability loss is communicated through **geometry and rhythm**, never color
alone:

1. **Ring fracture (primary anchor)**: as stability decays, the event horizon
   ring's outline deforms from a smooth circle to a broken, dashed, flickering
   circle. The ring is the "mine" anchor, so degrading it is thematically
   correct: instability threatens what the player owns.
2. **Pulse rhythm**: the ring's pulse frequency accelerates as stability decays
   (slow calm pulse = healthy; fast irregular pulse = failing).
3. **Audio creak rhythm** (section 5) mirrors 1-2.
4. The stability meter itself (layout owned by A-010) must use these same cues:
   segmented geometry and tick rhythm, not a red-amber-green gradient alone.

This is the accessibility anchor A-012 must support: reduced-motion variants
replace animated flicker with static dash density (more breaks = less
stability); high-contrast variants thicken the ring stroke. A-012 owns those
implementations; A-006 requires that every stability state be legible with
color disabled, motion frozen, and audio muted simultaneously.

### 6.2 Feedback ladder

Every absorb produces simultaneous micro-feedback: visual pop + audio tick +
capped camera nudge. Escalation is continuous with mass, not stepped by menus:

```text
low mass:    thin calm ring, quiet field, light pops
mid mass:    ring brightens, ambient pressure rises, pops deepen
high mass:   ring pulses faster, field darkens, movement reads heavy,
             hazard markers read louder — the same screen is now a threat
```

The player never sees a "danger meter" tutorialize this; the ring and the
audio do it. Combo feedback (shimmer, rising motif) exists, but its window and
values belong to A-007; feedback here must not imply a specific streak length.

### 6.3 Failure presentation

Visual and audio language of failure (spec 7.4; HUD layout is A-010):

- The result moment plays a field-collapse animation: the hole visibly
  implodes/shrinks — the player's own field failing.
- The failing condition is named by icon + one word (time, stability, or mass
  in variant modes): time = hourglass-family glyph with clock cadence,
  stability = fractured-ring glyph with shatter audio, mass = shrinking-hole
  glyph.
- The gap to the goal is shown as a single visual delta (layout A-010's); the
  design requirement is that it reads in one glance, no arithmetic on screen.

## 7. Accessibility direction

A-006 sets anchors and requirements; A-012 owns options and implementations.

| Anchor / requirement | Direction |
| --- | --- |
| Multi-channel redundancy | Every critical state (stability, hazard, combo, goal gap, failure type) is carried by at least two channels: shape, motion, rhythm, audio. Color is never the sole carrier of any state |
| Stability anchor | Ring fracture + pulse rhythm (section 6.1); A-012 reduced-motion, high-contrast, and mute variants must preserve legibility of all stability states |
| Hazard language | Shape-first: spiky silhouette + marker glyph; colorblind-safe palettes (A-012) must keep the glyph, not just swap hues |
| Ring contrast | The ring color must pass contrast against every world background; concrete values are A-010/A-012 |
| Minimum sizes | Ring stroke ≥ 2 px screen-space; edge indicators ≥ 8 px on the smallest supported phone; touch-target rules belong to A-010/A-011 |
| Reduced motion | All feedback (punch, shake, ripple, flicker) must have static-equivalent variants that keep information, defined by A-012 |
| Text | Minimal; the tutorial teaches by play, not text walls (spec 8.3 proposal, confirmed here as a design decision) |

## 8. Pacing — answers open question 7

The spec 3.2 duration proposals are **confirmed** as feel targets:

| Layer | Duration | Confirmation rationale |
| --- | --- | --- |
| Micro loop | 1-5 s | A drag + pull + absorb resolves in a few seconds at starting size, so growth is visible continuously (spec 8.2) |
| Meso loop | 20-60 s | A combo chain plus one region clear; long enough for a readable decision, short enough to feel progress |
| Macro loop | one level | Unchanged |
| Meta loop | one session | Unchanged |

These are ranges, not tuning constants: A-007's pull strengths, absorb radii,
timers, and combo window must land so the micro and meso loops resolve inside
these ranges. Combo window length itself remains A-007's.

## 9. Architecture contracts honored

- RenderLayer is read-only against sim state; every effect in this document is
  render/audio only and never feeds the sim (A-005).
- Sim stays at fixed DT = 1/60 s; quality tiers (high/low) change render only —
  particle budgets and LOD in section 4 follow that rule (A-005).
- Draw-call target ≤ 20, sprites ≈ 120, one atlas per world: the ring gets its
  dedicated draw call (A-005); indicators and particles must fit the remaining
  budget (implementation guidance for A-019).
- 60 fps phones / 30 fps fallback, low particle count, simple shaders (PLAN
  5.16): the tier budgets in section 4 are the design's share of that plan.

## 10. Open questions left downstream

- **A-007 (balance):** zoom curve constants, tension-escalation thresholds,
  combo window, timers — must land inside the pacing ranges of section 8 and
  keep the ring guarantee of section 2.
- **A-008 (content):** per-hazard marker glyphs and quantum particle/phase-shift
  looks must follow the shape-first language of section 3.2.
- **A-010 (UI):** HUD layout, mass meter and stability bar placement, edge
  indicator styling within safe areas, result-screen layout, minimum touch
  targets.
- **A-011 (input):** any assistive camera/tilt behavior; the default scheme
  stays centered on the hole (section 2).
- **A-012 (accessibility):** implementations of reduced motion, high contrast,
  colorblind-safe palette, larger controls — must preserve the anchors of
  section 7, especially the ring-fracture stability cue.
