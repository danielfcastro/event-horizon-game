# docs/ui.md — A-010 UI design (Event Horizon)

## 1. Purpose and contract

This document defines the user interface of Event Horizon: screen inventory,
in-run HUD, menus, settings surface, mobile scaling model, safe areas, edge-
indicator styling, result screens, and minimum touch targets. It is the layout
contract consumed by A-011 (input), A-012 (accessibility), A-013 (UI tests),
A-019 (prototype scaffold) and A-020 (ship).

Inherited contract rules, never renegotiated here:

- **Ring guarantee** (A-006 §2): the full ring plus its margin is always on
  screen and zoom never punches out, so the HUD must never cover the ring zone.
- **Off-screen indicator semantics and cap** (A-006 §3.4): pulled bodies and
  field-threatening hazards only, max 8, priority-sorted, absorbable vs hazard
  carried by glyph **shape** (chevron vs bracketed X), never color. Only
  placement, size, and styling are mine to decide.
- **Stability cue language** (A-006 §6.1): fracture (smooth → broken, dashed,
  flickering) + pulse rhythm + audio creak. The meter reuses that language; a
  red-amber-green gradient alone is forbidden.
- **Minimum sizes** (A-006 §9): ring stroke ≥ 2 px, edge indicators ≥ 8 px on
  the smallest supported phone. The touch-target floor is set here.
- **Numbers are displayed, never renumbered:** timer `150 + 5n` s, 3-star line
  ≥ 35 % timer remaining, efficiency cap 1.00, mass ceiling 2^24 (16,777,216),
  five difficulty bands (A-007); quantum mode, phase lanes Solid/Sift/Phase,
  one modifier per level with the L40 stacked pair (A-008); 40 levels / 5
  worlds, stars 0–3, endless depth tiers, daily/weekly modes (A-009).

Design-rule test (PLAN §8): every HUD element must make the **cost of mass**
legible — a glance must answer "what are the five mass levers doing to me?"
without opening a menu. A HUD showing only a mass number fails by omission.

## 2. Screen inventory

Seven screens. All are portrait-first; landscape is a reflow of the same
components, not a separate design.

| # | Screen | Role | Key contents |
| - | ------ | ---- | ------------ |
| S1 | Title | Entry point | Logo, Play button (primary), Continue, Settings, mode chips (Campaign / Endless / Daily / Weekly). No HUD. |
| S2 | Level select | Campaign map | 5 world rows × 8 levels, per-level stars (0–3), lock glyph on locked levels, best-time chip, modifier badge per level, "resume" marker. |
| S3 | In-run HUD | Overlay on play space | Timer, mass meter, stability bar, pressure cluster, quantum chip, modifier chip, edge indicators, pause button. |
| S4 | Pause | Frozen S3 + menu | Resume (primary), Restart, Quit to level select, settings shortcut. Keeps the HUD visible behind the dim so the player can re-read their state. |
| S5 | Results | End of run | Stars 0–3, timer remaining %, mass collected, efficiency (≤ 1.00), best combo, modifier badge, instability-death badge if applicable, Next / Retry / Level Select. |
| S6 | Mode menus | Endless depth tiers, Daily/Weekly | Tier ladder with the tier the player is on, best-depth record, seed/date chip, start button. |
| S7 | Settings | Persistent | Audio, haptics, UI scale, reduced motion, high contrast, colorblind palette, larger controls, text size, calibration entry point (owned by A-011). |

Screen rules:

- One primary action per screen, bottom-right in the thumb zone, always the
  largest target on that screen.
- No screen auto-advances; every transition is a tap.
- S3 is the only screen with live elements; S1, S2, S6, S7 are static and must
  do zero per-frame layout work.

## 3. HUD layout and safe areas

### 3.1 Reference frame

All layout is authored in **reference units (ru)** against a 390 × 844 ru
portrait frame — the smallest supported phone. Section 7 defines the mapping
from ru to physical pixels. Nothing in this document is authored in pixels.

### 3.2 Safe areas

Insets are read from the device (notch, home indicator, letterbox) and clamped:

```text
topInset    = clamp(deviceTopInset, 48, 120) ru
bottomInset = clamp(deviceBottomInset, 34, 110) ru
sideInset   = 16 ru (fixed)
```

The screen is then divided into four bands:

```text
top band    : topInset            .. ringTop - 8
bottom band : ringBottom + 8      .. height - bottomInset
left gutter : sideInset           .. ringLeft - 8
right gutter: ringRight + 8       .. width - sideInset
```

### 3.3 The ring zone (never occluded)

The ring is sized to the **narrower** axis, so on a 390 × 844 portrait phone
the ring diameter is 326 ru and the ring center sits at 46 % of height. That
leaves roughly 200 ru of clear band above and below the ring and only ~16 ru
of gutter at the sides. **The ring zone is a hard exclusion rect**: no HUD
element, chip, button, or indicator may enter it, at any zoom level, because
the ring guarantee (A-006 §2) keeps it on screen at all times.

### 3.4 Answer to spec open question 16

> **Where do the mass meter and stability bar sit within mobile safe areas?**

**Decision:** both sit in the **top safe band**, split left and right, as two
short segmented bars that hug the ring's upper shoulder — mass meter at the
top-left, stability bar at the top-right, timer top-center between them. The
bottom band is reserved for touch targets (pause button, control zone owned by
A-011) and carries only two small passive chips.

**Rationale:**

1. The ring guarantee reserves the vertical center, so on portrait the only
   large always-visible regions are the top and bottom bands, and the top band
   is the one that survives every device inset (insets are clamped, the band is
   recomputed, never assumed).
2. Both meters measure the player object at the ring center, so they sit at the
   ring shoulder rather than a screen corner: glance travel stays short.
3. The bottom band is where thumbs live and any thumb occludes a readout there,
   so it holds only things tapped once.
4. The left/right split encodes the two decisions traded off — mass taken
   (left) vs structure remaining (right).
5. A top band squeezed below 64 ru reflows to a stacked pair in the top-left
   corner (§7.3), never into the ring zone.

### 3.5 Top band, left to right

| Element | Anchor | Size (ru) | Notes |
| ------- | ------ | --------- | ----- |
| Mass meter | top-left, 8 ru from inset edge | 96 × 28 | Segmented bar + numeric readout (§4). |
| Pressure cluster | directly under mass meter | 96 × 20 | Five lever pips, the legibility answer to PLAN §8 (§4.3). |
| Timer | top-center | 84 × 34 | Tabular numerals, `MM:SS`, largest numeral in the band. |
| Stability bar | top-right, mirrored | 96 × 28 | Segmented arc, A-006 §6.1 cue language (§4.2). |

### 3.6 Bottom band

| Element | Anchor | Touch target (ru) | Notes |
| ------- | ------ | ----------------- | ----- |
| Pause button | bottom-right | 64 × 64 | Glyph 32 ru. Largest target on S3; acceptance criterion. |
| Control zone | bottom-left / center | reserved, ≥ 180 ru wide | Owned by A-011; this document only reserves it. |
| Modifier chip | bottom-left corner | 72 × 24 (non-touch) | Active level modifier, L40 shows both (§6). |
| Quantum chip | above modifier chip | 72 × 24 (non-touch) | Particles count + phase lane mode (§6). |

Chips are **non-interactive**: they never grow a touch target and never accept
a tap, so they cannot steal a thumb from the control zone.

## 4. Mass meter and stability bar

### 4.1 Mass meter

The meter is **not a number-first readout**; it is a pressure gauge with the
number as a secondary label.

- **Shape:** 12-segment horizontal bar, 2 ru gaps, filled from the left with a
  hard edge, never a gradient.
- **Segments are thresholds, not proportions.** Boundaries sit at the mass
  values where a lever changes state, so the bar's *shape* says which levers
  are live. The values are A-007's; this document only renders them.
- **Numeric readout** right of the bar, tabular, abbreviated above 99,999
  (`1.2M`, `16.8M`) so the 2^24 ceiling always fits in 6 glyphs.
- **Pressure cluster:** five pips under the bar, one per mass lever of A-006.
  Hollow = inert, filled = active, stroked-dashed = near tipping point. Order
  is fixed, never re-sorted. Each pip has a distinct 8 ru shape (pull,
  collision, speed, navigation, stability) so the cluster reads with color off.
- **Ceiling:** all 12 segments plus a double end-cap, the readout stops
  abbreviating, and there is no overflow animation.

### 4.2 Stability bar

Reuses A-006 §6.1's cue language; it does not invent a new one.

- **Geometry:** 10-segment arc mirroring the ring's curvature, anchored top-
  right. Segments are removed from the right end as stability decays, so the
  arc literally becomes **broken** — the same fracture the ring shows.
- **Fracture progression** matches the ring exactly: smooth → broken → dashed
  → flickering. Flicker is the only motion-only cue, so it carries a static
  fallback: the arc also drops to 4 segments and tick spacing halves. Frozen
  motion still reads.
- **Tick rhythm:** gaps widen 2 ru → 6 ru as stability decays, matching the
  ring's pulse-rhythm cue and legible with audio muted.
- **No color-only state.** A red-amber-green gradient may reinforce stage,
  never define it.
- **Numeric readout** is a 4-glyph percentage right of the arc, for players
  who cannot parse arc geometry.
- **Death state:** empty arc plus a bracket glyph (A-008 hazard family); the
  results screen carries the instability-death badge (§8).

### 4.3 What the HUD does *not* show

No health bar, no power-up glow, no "you are strong" indicator. Anything that
would soften the tension is excluded by rule, not by omission.

## 5. Edge-indicator styling within safe areas

Semantics and the cap of 8 are A-006 §3.4's. This document fixes placement,
size, and shape.

- **Placement — the indicator rail.** Indicators ride the ring-zone boundary on
  the **safe-area side**: side bearings sit in the left/right gutters at the
  vertical position matching the body's bearing, top/bottom bearings sit in the
  top/bottom bands. The rail is a fixed 24 ru track hugging the ring-zone edge,
  so indicators never enter the ring zone and never collide with top-band
  meters.
- **Anchor rule:** the chevron tip points at the ring center along the bearing
  and sits 8 ru inside the ring-zone edge, keeping the glyph in a safe area
  while encoding direction unambiguously.
- **Size:** nominal 14 ru, hard floor 8 ru on the smallest phone (A-006 §9),
  stroke 2 ru and never thinner.
- **Shape carries meaning:** absorbable = solid chevron `>`; hazard = bracketed
  X `[×]`. Two shapes only, no third family, no color distinction.
- **Priority sorting:** the 8 shown are the 8 highest-priority candidates;
  ties break by angular distance from the current heading, so the rail is
  stable frame-to-frame and does not flicker between equal candidates.
- **Overflow:** when more than 8 qualify, a single count badge (`+5`) sits at
  the rail's busiest corner. Text only, no new glyph family.
- **Pooling:** one pooled sprite batch (A-005 draw-call budget); the cap of 8
  is the pool size and no per-indicator draw call is proposed.

## 6. Menus and settings surface

### 6.1 Menu layout

Menus share one component set: a single primary button (bottom-right, 64 ru
target), secondary buttons (48 ru target), and chips (non-touch). Text is set
in a tabular UI face at 16 ru body / 22 ru heading on the reference frame.
Level select is a 5 × 8 grid of 40 ru cells with 12 ru gutters; each cell is a
touch target, so the grid needs 40 × 40 ru minimum and never shrinks below the
touch floor (§7.4).

### 6.2 Settings options (this document names the hooks; A-012 owns the
implementations)

| Setting | Group | Effect on this layout | Owner |
| ------- | ----- | --------------------- | ----- |
| UI scale | Display | Multiplies HUD ru sizes within the band clamp of §7.3 | A-012 |
| Safe-area reflow | Display | Re-runs the band computation for a different inset assumption | A-012 |
| Reduced motion | Motion | Freezes flicker, pulse, and chip transitions; static fallbacks in §4.2 and §5 must remain legible | A-012 |
| High contrast | Contrast | Raises stroke weight and background separation; must not change glyph shapes | A-012 |
| Colorblind palette | Color | Replaces the palette only; shape-first encoding in §4 and §5 stays the primary carrier | A-012 |
| Larger controls | Input | Grows touch targets to the 72 ru tier; reflows the bottom band | A-012 / A-011 |
| Text size | Text | Scales numerals and chips; numerals must stay tabular | A-012 |
| Audio cues on/off | Audio | Creak and pulse rhythm; §4.2 must stay legible with this off | A-012 |
| Haptics | Feedback | On/off, no layout effect | A-012 |
| Calibration | Input | Entry point only; scheme and calibration procedure belong to A-011 | A-011 |

Settings are stored per profile, applied at boot, and never require a restart
mid-run except UI scale, which applies at the next run boundary.

### 6.3 Settings affordances

- Reachable from Title, Pause, and Mode menus via a 48 ru chip, never smaller.
- Every option is a toggle or a 3-stop discrete slider; no free-form numeric
  entry on a phone.
- "Reset to defaults" is the only destructive action in S7 and confirms.
- Settings persist per profile and apply at boot; UI scale applies at the next
  run boundary.

## 7. Mobile scaling model and touch targets

### 7.1 Reference frame

390 × 844 ru portrait, 1 ru = 1 CSS px at 1× density. Smallest supported phone
320 × 568 ru; largest tested 430 × 932 ru. Landscape and desktop reuse the same
components with the bands rotated.

### 7.2 Scale factor

```text
s = clamp( min(width / 390, height / 844), 0.82, 1.45 )
```

`s` scales HUD geometry only. It does **not** scale the ring diameter (camera
contract, A-006 §2) or the safe-area insets (device facts). Below `s = 0.82`
the layout stops shrinking and **reflows** (§7.3).

### 7.3 Reflow rules (ordered, first match wins)

1. Top band < 64 ru: stack the mass meter above the stability bar in the top-
   left; timer stays top-center.
2. Top band < 32 ru: both meters become 64 ru compact bars at the band's inner
   edge; the pressure cluster stays under the mass meter, inside the safe area.
3. Gutter < 24 ru: that side's indicator rail collapses into the nearest
   horizontal band; the cap of 8 is unchanged.
4. Bottom band < 64 ru: the pause button moves to the band's inner edge and
   keeps its 64 ru target; chips drop to the 48 × 20 compact tier.
5. Never move an element into the ring zone. If no rule fits, hide the element
   and surface its information on the results screen — a stated degradation,
   not an occlusion.

### 7.4 Touch-target floor

| Class | Floor (ru) | Applies to |
| ----- | ---------- | ---------- |
| Primary action | 64 × 64 | Play, Resume, Next, Retry, Pause |
| Secondary action | 48 × 48 | Restart, Back, settings chip, menu rows |
| Minimum any target | 44 × 44 | Hard floor, every interactive element, all screens |
| Spacing between targets | ≥ 12 ru | Prevents mis-taps on the smallest phone |

The floor is enforced **after** scaling: `max(44, size × s)`. A target is never
scaled below 44 ru, which is why `s` clamps and reflow takes over.

## 8. Result screens and stars

### 8.1 Results (S5) — read order, top to bottom, largest first

1. **Stars** — three 40 ru slots, filled or hollow; hollow is a full outline at
   the same stroke weight, so a 0-star result cannot read as "one missing".
2. **Timer remaining %** — star line is ≥ 35 % (A-007); the screen prints the
   percentage and a tick at the 35 % position on a 12-segment track reusing
   §4.1's geometry, so the gap to the line is visible.
3. **Mass collected** — abbreviated form of §4.1.
4. **Efficiency** — capped at 1.00 (A-007); prints `1.00` never higher, with a
   hard end-cap on the track at the cap.
5. **Best combo** and **instability-death badge** (bracket glyph, §4.2).
6. **Modifier badge** — the level's modifier (A-008); L40 stacks both badges,
   never merges them.
7. **Buttons** — Next (primary, 64 ru), Retry, Level Select (48 ru).

### 8.2 Failure, unlock, and progression surfacing

- Failure reuses this screen with stars replaced by one line naming **why** the
  run ended in lever vocabulary: "stability collapsed" or "time expired". No
  apology copy and no hint copy — the diagnosis is the tension made legible,
  and hinting belongs to A-009, not the UI.
- Level select shows per-level stars and a lock glyph on locked levels; the
  unlock system itself is A-009's.
- Endless surfaces **depth tiers** as a vertical ladder; the current tier is
  marked by a bracket, not by color.
- Daily/Weekly show a seed/date chip and a "one attempt" chip and never a retry
  button.

## 9. What this design makes testable (A-013 writes the tests, not this file)

- **Safe-area containment / ring-zone occlusion:** every HUD bounding box lies
  outside the ring-zone rect at `s = 1`, `s = 0.82`, and the largest inset
  clamp, and zero HUD pixels fall inside the ring zone at any zoom, across the
  320 × 568, 390 × 844, and 430 × 932 frames.
- **Pause button size:** target ≥ 64 ru at every frame and with the larger-
  controls tier on.
- **Touch floor:** no interactive element below 44 ru after scaling, including
  level-select cells.
- **Indicator cap / floor:** at most 8 glyphs, `+n` badge is the only overflow
  form; glyph ≥ 8 ru and stroke ≥ 2 ru on the smallest frame.
- **Reflow coverage:** each rule in §7.3 is reachable by some viewport and the
  rule-5 fallback is exercised.
- **Color-independent state:** stability stage and pressure-cluster state are
  distinguishable with the colorblind palette on and high contrast off; pips
  are distinguishable by shape alone.
- **Motion-independent state:** with reduced motion on, the flicker stage of
  §4.2 is still distinguishable from the dashed stage.
- **Audio-independent state:** with audio off, stability decay is still legible
  through segment count and tick spacing.
- **Draw-call fit:** HUD renders as a fixed set of batched layers; indicator
  pool size equals 8; no per-element draw call.
- **Number fidelity:** timer, star line, efficiency cap, and mass ceiling
  displayed match A-007 exactly; no UI rounding beyond the documented
  abbreviation.

## 10. Open questions for downstream artifacts

1. **A-011:** the control zone is reserved at ≥ 180 ru in the bottom band, but
   the scheme (tilt/joystick, no-absorb-button) may need a second surface; if
   it must sit in the top band it displaces the pressure cluster, and which of
   the two stays is A-011's call.
2. **A-011:** does the 64 ru pause button need a hold-to-pause guard against
   mis-taps on the smallest phone, given the ≥ 12 ru spacing rule?
3. **A-012:** is the static fallback for the flicker stage (4 segments + halved
   tick spacing) enough under reduced motion for a player who cannot parse arc
   geometry, or does A-012 need a text stability readout beyond the percentage?
4. **A-012:** high contrast raises stroke weight — confirm a 3 ru ring stroke
   still fits the ring guarantee's margin budget in §3.3, or shrink the ring
   diameter by 4 ru in that mode.
5. **A-013:** how to test "legible with color off" deterministically — pixel
   sampling of shape masks, or a shape-only render pass?
6. **A-019:** confirm the HUD's batched-layer count fits A-005's draw-call
   budget before assigning per-chip nodes.
7. **A-020:** capture store screenshots at the 390 × 844 reference frame so
   store art matches the smallest-phone layout, not the largest.
