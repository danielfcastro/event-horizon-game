# A-012 — Accessibility (docs/accessibility.md)

Agent: designer. Depends on A-010 docs/ui.md (done); honors approved A-006 docs/design.md and A-011 docs/input.md. Consumed by A-013 docs/test-plan.md, A-019 docs/prototype-scaffold.md, A-020 docs/ship.md.

## 1. Purpose and inherited anchors

This document turns the accessibility **anchors** set in A-006 §7 into concrete, shippable options with defaults. It owns behavior; it does not redraw layout (A-010), add input verbs (A-011), or define new render layers (A-005).

Anchors honored, never renegotiated:

- **Multi-channel redundancy.** Every critical state — stability, hazard, combo, goal gap, failure type — is carried by at least two channels: shape, motion, rhythm, audio. **Color is never the sole carrier of any state.**
- **Stability anchor.** Ring fracture ladder (smooth → broken → dashed → flickering) + pulse rhythm + audio creak (A-006 §6.1). Reduced-motion replaces animated flicker with **static dash density (more breaks = less stability)**; high-contrast **thickens the ring stroke**.
- **Hazard language is shape-first.** Spiky silhouette + marker glyph; colorblind-safe palettes **keep the glyph, not just swap hues**.
- **Ring contrast.** The ring passes contrast against every world background; concrete values are set here (§4, §5).
- **Minimum sizes.** Ring stroke ≥ 2 px screen-space; edge indicators ≥ 8 px on the smallest supported phone.
- **Reduced motion.** All feedback (punch, shake, ripple, flicker) has static-equivalent variants that keep information — defined here in §3.
- **Text.** The tutorial teaches by play, not text walls; text aids are additive and never required.

Hard requirement inherited from A-006: **every stability state must stay legible with color disabled, motion frozen, and audio muted simultaneously.** §8 states this as a contract A-013 will test.

Settings names are fixed by A-010 S7 (UI scale, safe-area reflow, reduced motion, high contrast, colorblind palette, larger controls, text size, audio cues on/off, haptics). This document owns the behavior of each.

## 2. The no-softening rule (PLAN §8 check)

**Options change how information is carried and how intent is expressed — never what the decisions are.** No option may soften the cost of mass, auto-aim, auto-absorb, or hide a lever. Every option below is checked against that rule.

| Option | What it changes | What it must not change |
| --- | --- | --- |
| UI scale | Pixel size of the fixed ru layout | Positions, hit-test semantics, play-area scale; mass growth and collision risk are untouched |
| Safe-area reflow | Where the bottom band sits | The band's contents and the visibility of the play area |
| Reduced motion | Animation of feedback | Timing windows, hazard contact, stability decay rate, pull strength |
| High contrast | Stroke weight, background separation | Glyph shapes, hazard silhouettes, ring geometry |
| Colorblind palette | Hue assignment only | Shape-first encoding, glyph families, dash counts, tick spacing |
| Larger controls | Touch-target size (64 → 72 ru tier) | Control-zone width, pause anchor, spacing floor |
| Text size | Numeral and label size (tabular) | Which numbers exist; no new readouts by default |
| Audio cues on/off | Audio channel | Visual/rhythm channels stay complete with audio off |
| Haptics on/off | Physical pulse | Nothing visual; never the sole carrier of any state |
| Tilt calibration variants | Hold tolerance, re-calibration prompt | Neutral pose semantics, drift behavior |
| One-handed modes | Which intent channel expresses intent | The verbs available, the hazards, the costs |
| Pause confirm-state | Number of taps on the existing button | Any gameplay state; pause is not a gameplay verb |

Explicit non-softening guarantees:

- **No auto-aim, no auto-absorb, no assist of any kind** is offered by any option. The absorb gate and the pull model are identical across every accessibility configuration.
- **No option hides a lever.** Stability decay, pressure clusters, hazard fields, and the goal gap remain visible and legible in every configuration, including the most permissive one.
- **No option changes a number that the balance artifact owns** (mass growth, pull strength, decay rate, upgrade values). Accessibility touches presentation only.
- Reduced motion freezes *presentation*, not *simulation*: a frozen ripple still occupies the same frames and the same hazard still kills on the same contact.

## 3. Reduced motion

Default: **off**. Offered in the settings sheet (A-010 S7) and auto-detected from the OS reduced-motion preference at boot. When on, every animated carrier is replaced by a static carrier that keeps the same information. No feedback is simply deleted.

| Animated carrier | Normal form | Reduced-motion static equivalent | Information preserved |
| --- | --- | --- | --- |
| Punch (absorb) | radial scale pop + brief flash | one static ring outline at 1.15× for 1 frame-pair, plus the absorb pip count | "absorbed", count of pips |
| Shake (collision/failure) | screen offset | two static offset outlines of the ring (ghost outline), no screen movement | "impact happened", which side |
| Ripple (pull wave) | expanding wave rings | 2–3 concentric static arcs, density = pull strength | direction and strength of pull |
| Flicker (critical stability) | opacity flicker | **static dash density: 4 segments, halved tick spacing** | stability stage, decay direction |
| Pulse (stability rhythm) | breathing scale | fixed scale; rhythm moves to **tick spacing only** | decay rate |
| Chip transition | interpolated morph between stages | discrete stage swap, no in-between frames | stage identity (4 stages, never 3) |

Rules:

- Static equivalents are **pooled and tier-budgeted** (A-005): a frozen ripple is the same draw call as an animated one, one fewer vertex update. No new render layers.
- Frame-pair holds are presentation-only; the underlying event timing is unchanged.
- With reduced motion on, **haptics remain available** as the non-visual, non-color carrier for impact events.

### Answer to A-010 Q3

Q3: is the flicker-stage static fallback (4 segments + halved tick spacing) enough under reduced motion for a player who cannot parse arc geometry, or is a text stability readout needed beyond the percentage?

**Answer: the geometric fallback is enough for the general population, but not for the non-arc-parsing cohort, so a word-tier readout ships as an additive option.**

- The 4-segment + halved-tick fallback is **sufficient** for players who can read the ring as a whole: segment count and tick spacing are two independent channels, and they survive color-off and audio-off.
- It is **not sufficient** for players who cannot parse arc geometry (low acuity, partial vision). For them the ring is a texture, not a measure.
- Decision: option **`stability labels`** (default **off**, forced **on** whenever reduced motion is on and text size is at the maximum tier). It adds one four-word label bound to the ring: **STABLE / FRACTURED / CRITICAL / FAILING**. It is additive — it never replaces the ring, never appears as a sentence, and never states a number the player must compute. The percentage readout from A-010 stays tabular and unchanged.
- The label is bound to the same stage machine as the ring, so it cannot disagree with it (A-013 tests this).

## 4. High contrast

Default: **off**. Auto-offered when the OS high-contrast or increased-contrast setting is present. Raises stroke weight and background separation; **never changes glyph shapes, dash counts, or tick spacing**.

| Element | Normal | High contrast |
| --- | --- | --- |
| Ring stroke | 2 ru | **3 ru** |
| Hazard silhouette outline | 1 ru | 2 ru (spiky shape unchanged) |
| Marker glyph (bracket / X family) | as set by A-008 | +1 ru stroke, **same shape** |
| Edge indicators | 8 ru minimum | 10 ru |
| HUD numerals | normal weight | heavier weight, still tabular |
| World background | as authored | background pushed one step away from the ring's luminance |

Background separation per world (worlds declare a background class; A-008 maps each world to one):

| Background class | Normal ring luminance gap | High-contrast ring luminance gap |
| --- | --- | --- |
| Dark field | ≥ 4.5:1 | ≥ 7:1 |
| Mid belt | ≥ 4.5:1 | ≥ 7:1 |
| Light storm | ≥ 4.5:1 | ≥ 7:1 |

### Answer to A-010 Q4

Q4: high contrast raises stroke weight — confirm a 3 ru ring stroke still fits the ring guarantee's margin budget (A-010 §3.3 sizes the ring to the narrower axis with margin), or shrink ring diameter by 4 ru in that mode.

**Answer: keep the diameter and take 3 ru; the 4 ru shrink is a fallback, not the default.**

- A 2 → 3 ru stroke consumes 0.5 ru outward and 0.5 ru inward. A-010 §3.3 sizes the ring to the narrower axis **with margin**, and that margin absorbs the outward half on every supported phone width; the inward half only reduces the interior dead zone, which carries no state.
- Therefore the default high-contrast configuration is **stroke 3 ru at unchanged diameter**. The ring guarantee (stroke ≥ 2 px screen-space) is met with more headroom, not less.
- The **4 ru diameter shrink is retained as a fallback** for the narrowest supported aspect where the computed margin is under 1 ru. A-019 computes this at layout time; if margin < 1 ru, shrink diameter by 4 ru and keep 3 ru stroke. This is a per-device branch, not a per-player choice.
- Consequence to keep: high contrast must not change the ring's stage geometry, so the same stage machine drives both modes.

## 5. Colorblind-safe palette

Default: **off** (authored palette). Options: `deuteranopia-safe`, `protanopia-safe`, `tritanopia-safe`, `monochrome`. All four are built from one rule set: **hue is a secondary carrier; shape stays primary.** A palette may replace a hue assignment and nothing else — dash counts, tick spacing, silhouettes, and the A-008 bracket/X hazard glyph family are untouched in every palette.

Slots are role-based, so a world only declares its background class (dark field / mid belt / light storm, per §4).

| Role (shape carrier in brackets) | Dark field | Mid belt | Light storm | Monochrome (all classes) |
| --- | --- | --- | --- | --- |
| Player ring core [ring, thickness] | `#E8E8E8` | `#F0F0F0` | `#1A1A1A` | `#FFFFFF` / `#000000` outline |
| Player ring fracture [dash pattern] | `#56B4E9` | `#56B4E9` | `#0072B2` | luminance step only |
| Hazard body [spiky silhouette] | `#D55E00` | `#D55E00` | `#AC2204` | `#000000` fill, 2 ru outline |
| Hazard marker [bracket / X glyph] | `#F0E442` | `#F0E442` | `#7A6B00` | `#FFFFFF` glyph on dark fill |
| Absorb target [pulsing pip cluster] | `#009E73` | `#009E73` | `#005E45` | dashed outline |
| Upgrade / lever [distinct silhouette] | `#E69F00` | `#E69F00` | `#8A5F00` | double outline |
| Goal marker [arrow chevron] | `#0072B2` | `#0072B2` | `#E69F00` | `#FFFFFF` chevron |
| Pressure cluster [contour bands] | `#CC79A7` | `#CC79A7` | `#7A3F5E` | banded luminance |

Rules:

- Every palette keeps **≥ 4.5:1** between adjacent role pairs and **≥ 3:1** between a role and its world background; the high-contrast mode of §4 raises these to ≥ 7:1 and is the tested worst case.
- **No role pair differs by hue alone.** Two roles that share a shape always differ in luminance by at least one step, so the `monochrome` palette is a legal configuration and the strongest test target.
- The deuteranopia/protanopia/tritanopia sets are drawn from the Okabe–Ito family; they are never used to signal "danger" or "safe" — the bracket/X glyph family and the spiky silhouette do that, per A-006 and A-008.
- A palette may not introduce a new color for a new state; new states get new shapes (that is A-008's domain).

## 6. Larger controls

Default: **off**. The 72 ru tier is an accessibility tier above A-010's 64 ru primary tier; it grows touch targets only, and it must fit A-011's ≥ 180 ru control zone **without moving pause**.

Geometry (bottom band, smallest supported phone):

- Two primary targets grow 64 → **72 ru**. Pair width = 72 + 12 + 72 = **156 ru**, inside the 180 ru zone, leaving **12 ru slack per side**.
- Spacing floor **≥ 12 ru is preserved** between the pair and between every target and the safe-area edge.
- Pause stays at its A-010 anchor as a **64 ru** single-tap button; it does not move, does not grow, and is not inside the 180 ru zone.
- Secondary actions (edge indicators, corner targets) keep their A-010 anchors and their ≥ 8 ru minimum; they do not grow, so the band does not widen past the zone.

Reflow deltas against A-010 §7.3:

| Element | Delta |
| --- | --- |
| Bottom band height | +8 ru (target tier growth only) |
| Play-area bottom edge | shifts up by the same +8 ru; play area never shrinks past A-010's floor |
| Safe-area reflow | absorbs up to 8 ru before the band moves |
| HUD numerals | unchanged |
| Ring sizing | unchanged (ring is sized by §3.3, not by the band) |

Interaction with UI scale (A-010's band clamp, §7.3):

- Larger controls are applied **inside** the clamp, never by exceeding it. If the clamp is reached, the band reflows to its compact single-row form rather than scaling further.
- Larger controls and UI scale at maximum together are legal only if the pair still fits the 180 ru zone after clamping; if it does not, the **larger-controls tier wins** and UI scale is reduced one tier. Accessibility never silently drops a touch target.
- Applied at boot per A-010; UI scale changes take effect at the next run boundary (A-019 implements).

## 7. Alternative controls

This section adds **variants** to A-011's intent channels. It adds no verbs.

### One-handed default (A-011 open question — my call)

**Default for one-handed players: tilt.** Joystick is the offered alternative, not the default.

- Tilt requires **no screen contact**: the free hand never enters the play area, so the whole field stays visible and no target is occluded. A joystick uses one screen contact on A-011's floating base in the control zone; that contact competes with the one-handed grip and drags the calibrated neutral pose, so tilt stays the one-handed default.
- Joystick is offered as the alternative for players who **cannot hold a steady neutral pose** (fatigue, low wrist control) but can operate the floating base with one contact in the control zone. It keeps thumbs out of the play area per A-011.
- Selection is a single settings choice — `intent: tilt` / `intent: joystick` — surfaced in the one-handed setup flow, not buried in the settings sheet.
- Neither alternative changes the verbs available, the absorb gate, or the cost of mass.

### Tilt calibration variants (A-011 §4 hook)

A-011 sets neutral-pose capture and a **±4° default Hold tolerance**. Variants owned here:

| Variant | Hold tolerance | For whom | Default |
| --- | --- | --- | --- |
| Standard | ±4° | two-handed, steady hold | yes |
| Relaxed | ±8° | one-handed, seated, device on lap | offered with one-handed mode |
| Coarse | ±12° | low wrist control, high fatigue | opt-in |

- Wider tolerance **does not** reduce steering precision in play: it widens the band in which the neutral pose is *held*, so the player is not punished for drift; it does not widen hazard hitboxes and does not slow the game.
- **Re-calibration prompt:** fires when the neutral pose drifts beyond the active tolerance for 20 s, or on explicit request. It is a prompt on the existing pause affordance — no new verb, no new button.
- One-handed grip profiles: `left`, `right`, `lap`, `mounted`. They set the neutral pose only; they never mirror the level layout.

### Pause confirm-state (A-011 hook — decision)

**It ships, as an opt-in variant, default off.**

- Form: the existing **single-tap 64 ru pause button** gains a confirm state — first tap opens a "hold to confirm" window, a second tap within 600 ms commits the pause; a single tap alone does nothing. No new verb, no new control, same target size and position.
- Ships **on by default only when** one-handed mode or larger controls are enabled, because those configurations have measurable accidental-contact risk. Otherwise it stays off so the default scheme remains one tap.
- It never affects gameplay: pausing is not a gameplay verb, and the confirm state cannot delay, hide, or soften any decision in the field.

## 8. The simultaneous-legibility requirement (contract for A-013)

**Contract SL-1.** In the configuration `colorblind palette = monochrome` + `reduced motion = on` + `audio cues = off` + `haptics = off`, every one of the four stability stages must remain distinguishable from every other, and each of the following must remain identifiable: hazard presence and type, combo state, goal gap direction, failure type, pressure-cluster presence, stability decay direction.

The channel inventory that survives SL-1, per state:

| State | Channel 1 (survives SL-1) | Channel 2 (survives SL-1) |
| --- | --- | --- |
| Stability stage | ring dash/segment count (4 → 3 → 2 → 4 dense) | tick spacing (full → halved) |
| Stability decay | tick spacing trend | label tier (when `stability labels` on) |
| Hazard presence | spiky silhouette | bracket / X marker glyph |
| Hazard type | glyph family identity | silhouette variant |
| Combo | pip count | pip arrangement |
| Goal gap | chevron direction | distance band count |
| Failure type | ring break pattern | ghost-outline offset direction |
| Pull strength | concentric arc count | arc spacing |

Rules of the contract:

- No state may rely on a channel that SL-1 disables. If a state's only surviving channel is shape, it must survive at the **minimum sizes** (ring stroke ≥ 2 px screen-space, edge indicators ≥ 8 px) on the smallest supported phone.
- SL-1 must hold in **all four** high-contrast states (on/off) crossed with **all four** palettes, and at **every UI-scale tier inside A-010's clamp**. That cross-product is the space A-013 samples.
- SL-1 is a legibility contract, not a difficulty contract: it says nothing about whether the player can *win*, only whether the player can *read the decision*.

## 9. What this makes testable (A-013 samples these; it does not invent them)

- **SL-1 legibility** (§8): the four stability stages pairwise-distinguishable under monochrome + reduced motion + audio off + haptics off.
- **Stage machine identity**: `stability labels`, ring dash count, tick spacing, and the percentage readout all report the same stage in one frame — no disagreement.
- **Reduced-motion equivalence**: each static variant in §3 carries the same event as its animated form at the same frame index; nothing is deleted.
- **Flicker vs dashed**: the 4-segment halved-tick flicker stage is distinguishable from the dashed stage with motion frozen and color off.
- **High-contrast margin**: 3 ru stroke fits the §3.3 margin budget on every supported aspect; the 4 ru shrink branch fires only when margin < 1 ru.
- **Palette purity**: switching palettes changes hue assignments only — glyph families, dash counts, and silhouettes are byte-identical.
- **Touch geometry**: 72 ru pair fits the 180 ru zone with ≥ 12 ru spacing and pause unmoved, at every UI-scale tier.
- **No-softening**: no accessibility configuration changes absorb gating, pull strength, decay rate, or hazard contact; a settings change never changes a balance-owned number.
- **One-handed default**: `intent: tilt` is the offered default and requires zero screen contact; joystick uses one screen contact in the control zone (A-011's floating base).
- **Pause confirm-state**: only reachable on the 64 ru target; never adds a verb; default on only under one-handed or larger-controls configurations.
- **Performance**: static variants and thicker strokes fit the same pooled, tier-budgeted draw calls (A-005) — no new render layers, no new pools.

## 10. Open questions for downstream artifacts

For **A-013 (qa)**:
- Q1: how to sample the SL-1 cross-product (4 palettes × high contrast on/off × UI-scale tiers) without an unbounded matrix — is a fixed worst-case set (monochrome + high contrast on + max/min scale) sufficient coverage?
- Q2: how to assert "distinguishable" for shape channels in a headless sim — pixel-difference thresholds on the ring band, or direct stage-state reads from the stage machine?

For **A-019 (programmer)**:
- Q3: where the OS-preference auto-detection (reduced motion, high contrast) is read — boot only, or at the next run boundary like UI scale?
- Q4: how `stability labels` renders without a new draw call — can it reuse the HUD numeral pool with tabular glyphs?

For **A-020 (ship)**:
- Q5: which store fields must list these options (accessibility feature list), and whether `monochrome` as a selectable palette must be called out as colorblind support specifically.

For **A-008 (content)**:
- Q6: whether any hazard introduced later can be legible with the bracket/X family alone; if a hazard needs a new glyph family, it must be added to the shape-first inventory, not to a palette.

