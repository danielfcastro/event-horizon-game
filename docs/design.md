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
| Black hole | Interstellar presentation (extension, 2026-10-16): a true-black sphere — still the darkest thing on screen — wrapped by a bright accretion disc of glowing gas at the horizon rim, plus a faint lensing halo around the sphere's edge (all visual only). The bare-disc look this row used to carry failed on device: a pure-black disc with a thin rim on the near-black field did not read as a hole; the rim-wrapped glow is what makes the void's silhouette readable, exactly as in the film | The hole is the player; nothing may read as darker. The decoration must not out-contrast the ring: the readability hierarchy of 3.1 (event horizon ring > hazard markers > large bodies > medium bodies > small bodies > field texture) must survive the extension, so the ring stays the strongest element on screen |
| Event horizon ring | Single bright ring, constant screen-space stroke width (min 2 px), slow calm pulse at low mass | One anchor for "mine"; screen-space width keeps it visible at every zoom |
| Absorbable bodies | Rounded/organic silhouettes; brightness below the ring, above the background; at small sizes they read as glowing points/stars with soft halos (extension, 2026-10-16 — see the bodies-as-stars note below this table) | Silhouette says "food" without color reliance; the glow is what makes small bodies read as proper stars instead of the flat discs the device report saw, without changing the silhouette |
| Hazards | Sharp/spiky silhouettes plus a marker glyph (bracket/X family), never color-only; the same soft glow may sit *behind* the core but must not soften the spiky outline or the glyph | Spec 5.2 requires hazard-vs-food readability; shape carries it for colorblind players (anchor for A-012); the glow must never blur the shape family that carries the identity |
| Inside vs outside | See 3.3 (answers open question 4) | The core readability decision |
| Bodies per world | One texture atlas per world (A-005); palette per world; LOD A near = sprite + glow, B mid = sprite only, C far = 1-4 px point | A-005 LOD contract, not contradicted — the star language below is exactly this contract's A/C glow, stated as ratios |

**Bodies as stars (extension, 2026-10-16).** The device report read the bodies
as "stars, but very poorly made" — flat discs on a near-black field. Every body
must instead read as a proper glowing star at small sizes: a bright core with
one or two larger, lower-alpha soft sprites behind it (the soft-alpha vocabulary
of 3.5). This applies to the whole family list of PLAN 5.7 — absorbables (dust,
pebbles, ice fragments, debris / rocks, metal shards, ship fragments, asteroids /
asteroid chunks, small planets, star fragments, quantum anomalies) and hazards
(mines, gravity anchors, magnetic fields, collapsing debris, massive objects).
The shape language of the table above is **not replaced**: food stays rounded/
organic, hazards stay sharp/spiky plus marker glyph, and identity is never
color-only — the colorblind anchor for A-012 stays intact.

Sizes and brightness are stated as **ratios**, never pixels or hex palettes:

- Drawn size follows the body's own sim radius through the camera mapping
  (unchanged); the design adds only the halo — a soft glow of 2-3x the drawn
  core radius, capped at 1/8 of the horizon radius so glows never merge into a
  field that hides the ring.
- Core brightness: clearly above the background field at the same screen
  position, and below the ring; the ring stays the brightest element (3.1).
- Background stars (the field layer of 3.3) stay dimmer and smaller than even
  the smallest body's core, so bodies never compete with background stars.

The concrete palette is A-023's (it already mirrors A-026's view.c palette); the
tier bits (kind>>2)&3 stay the identity channel — A-023 must not encode
identity in color.

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
4. **Accretion decoration** (extension, 2026-10-16): the accretion disc of
   glowing gas wrapped at the horizon rim and the faint lensing halo around the
   true-black sphere (3.2). This is a FOURTH, purely decorative channel: it
   exists because the bare-disc look did not read as a hole on device, and it
   must never carry readability by itself — with channels 1-3 unchanged,
   removing the disc and halo must still answer "mine vs not yet mine".

At a glance: ring + dark interior + drifting bodies = mine; the disc and halo
make the void itself legible against the field but never replace that reading.
On the smallest phone, channels 1 and 2 carry the reading; channel 3 confirms;
channel 4 decorates. Faint gravitational streamlines (field texture, visual
only) may be added at high quality tier; they must never imply a sim effect
that does not exist.

**Starfield field layer (extension, 2026-10-16).** The field may read as a
starfield: a purely visual layer of small static points, decoupled from the
sim exactly like the interior-vacuum gradient above (A-005 allows field
texture). It must stay the WEAKEST element of the 3.1 hierarchy — background
stars dimmer and smaller than even the smallest body's glowing core (the
bodies-as-stars ratios in 3.2) — so bodies never compete with background
stars. Lensing, the accretion disc, and the starfield are visual only: they
never imply a sim effect that does not exist, and they never change digest
identity — digest identity is defined by sim state alone, and these layers
never feed the sim (no-softening, A-012/A-014). The pull/absorb model stays
PLAN 5.6's, untouched.

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

### 3.5 Player-package presentation contract — the layers A-023 implements

The Interstellar look above must be expressible on the player package as
measured (PLAN.md §4 A-023): SpriteRenderer with boot-generated textures only
(`Texture2D.SetPixels` + `Sprite.Create`); the built-in sprite lookup returns
NULL on this package, so the package ships **no asset files** — every disc,
glow, and star texture is generated at boot. The camera is runtime-created,
orthographic, at z = -10, and z-ordered sorting is proven to work on it (body
> disc > rim sorts correctly), so the layer list below is stated as z-order,
not as shaders or materials:

Farthest to nearest:

| Layer | What it is | Contract |
| --- | --- | --- |
| Starfield | Boot-generated points scattered across the world bounds — as individual point sprites or as one boot-generated background texture, whichever fits the A-005 draw-call budget (section 9); A-023 chooses | Farthest z. Purely visual (3.3): never carries readability, never feeds the sim, weakest element of 3.1 |
| Accretion glow | Soft-alpha radial-gradient sprite(s) wrapped at the horizon rim | Behind the true-black sphere; the glow reads as gas around the void, as in the film |
| True-black sphere | The darkest thing on screen (3.2) | In front of its own glow, so the glow only ever appears as a rim-wrapped band and a halo |
| Rim ring | The bright constant-width ring (3.2/3.3 channel 1) | In front of the sphere; the only perfect circle stays unbroken by decoration |
| Bodies | The nearest layer (3.2 bodies-as-stars) | Drawn above the ring where they overlap; the ring stays readable because it is the only perfect circle and the brightest element (3.1), not because it is on top |

Glow rule: every glow must be expressible as **layered soft-alpha sprites**.
Additive blending is NOT guaranteed on this package, and the design must not
depend on unproven shaders, materials, or blend modes — the programmer settles
API names by compiling, so the design states layering and alpha only. A
"glowing star" is a bright core sprite plus one or two larger, lower-alpha
soft sprites behind it; that composition is the whole vocabulary, and it is
also how the accretion disc and the lensing halo are built.

### 3.6 Motion readability (device-report follow-up)

The 2026-10-16 device report read the run as slow. Presentation may add
motion cues without touching any number:

- The accretion disc rotates slowly around the sphere — decoration; its speed
  is a render constant owned by A-023, not a sim quantity.
- Bodies inside the horizon drift toward the hole — that drift IS sim
  behavior rendered as-is (3.3 channel 3); the design adds no motion of its
  own to bodies.
- The starfield is static; it must not drift, so it can never fake or dilute
  the sense of motion.

Boundaries: this does not change the PLAN 5.3 camera rule (the merged two-
phase hybrid of A-026 stays as approved), does not change any A-007 number,
and does not change any A-009 level number. If the run still reads slow after these cues,
the cause is pacing or telemetry (A-007, A-014), not presentation, and this
document must not paper over it.

Reduced motion (A-012 anchor, kept honest): at reduced motion the design turns
OFF — the accretion disc's rotation, the lensing halo's flicker/shimmer, the
ring's slow calm pulse, the camera punch and shake, and all particle motion —
and keeps ON — the static disc and halo shapes, the ring, the interior-vacuum
gradient, and the bodies-as-stars glow. Body drift is rendered sim state;
A-012 may freeze it, but then the inside/outside reading must still survive on
channels 1 and 2, which is exactly the guarantee of 3.3 above. A-012 owns the
option implementation; this names what must flip.

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

---

## 11. Extension record — 2026-10-16, the Interstellar presentation

| Field | Record |
| --- | --- |
| Asked | The user looked at the game on a phone (S24 FE) and reported: no black hole is visible; the dots read as "stars but very poorly made"; the run reads as slow. The user's design decision: the black hole must be presented as in the film Interstellar — a true-black sphere wrapped by a bright accretion disc of glowing gas, with a lensing halo — and the field must read as a starfield with the bodies as proper glowing stars, not flat discs. Recorded in PLAN.md §4 A-006 and §4 A-023 (commit 4d8c5de docs(plan):); PLAN.md wins on disagreement. The measured gap this extension must fix: a pure-black disc with a 4 px rim on the near-black field did NOT read as a hole on device. |
| Decided here | §3.2: the black-hole row now carries the Interstellar presentation (true-black sphere + rim-wrapped accretion disc + faint lensing halo, visual only); the body rows read as glowing points/stars with soft halos while keeping the shape language (rounded/organic food, sharp/spiky + glyph hazards, never color-only) and stating sizes and brightness as ratios of the horizon radius and of the background. §3.3: the disc and halo become a fourth, purely decorative readability channel that must never carry readability by itself; the starfield becomes the weakest field layer, decoupled from the sim. §3.5: the player-package layer contract — z-order starfield farthest, accretion glow behind the true-black sphere, bright rim ring, bodies nearest; boot-generated textures only, no asset files; glow expressible only as layered soft-alpha sprites, no dependence on unproven shaders, materials, or blend modes. §3.6: motion cues (disc rotation, rendered-as-is body drift, static starfield) and the honest reduced-motion list of what turns off. |
| Deliberately untouched | Sim numbers, the PLAN 5.3 camera rule, every A-007 balance number, every A-009 level number, golden replays, and digest identity. Lensing, the accretion disc, and the starfield are visual only and never feed the sim; the pull/absorb model stays PLAN 5.6's. The contrast hierarchy of 3.1 and the hazard shape family of 3.2 stay intact — the presentation must not hide the tension cues (PLAN section 8: not a pure power fantasy). No UI layout (A-010), no input (A-011). |
| Deferred to A-023 | The concrete palette (A-023 already mirrors A-026's view.c palette; the field clear stays pack_px(8,10,18) unless A-023's own extension says otherwise — this document states ratios only, never hex values); the boot texture-generation recipes; Unity API names, settled by compiling; the disc rotation speed as a render constant; halo sizes within the ratios stated in 3.2. The tier bits (kind>>2)&3 stay the identity channel — A-023 must not encode identity in color. Whether the perceived slowness is an older APK or the phase-1 world-fit view is settled by a fresh device capture (A-023/A-014), not by this document. |
