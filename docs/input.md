# A-011 docs/input.md — Input Design

Event Horizon input design. Defines the default drag control, the joystick and
tilt alternatives, touch handling, gesture disambiguation, the replayable input
digest, and the accessibility hooks A-012 will build on.

## 1. Purpose and inherited contract

Honored, never renegotiated:

- **PLAN §8 tension.** Movement is the whole verb set. Precision movement is
  the "stay smaller and move precisely" side of every decision. Input must make
  precise movement *possible and readable* at every mass. No rubber-banding,
  no snap-to-lane assists, no magnetism button — nothing that erases the
  tension input is supposed to expose.
- **PLAN §5.2 controls ladder.** Default: **drag**. Alternative: **virtual
  joystick**. Alternative: **device tilt**. Camera follows the hole; zoom is
  automatic (camera contract).
- **Spec §4.1 (confirmed, see §5).** Drag translates the hole; there is **no
  separate attract input** — the field is always on, absorption is positional
  (absorb by moving).
- **Spec §4.3 verbs.** Move / Hold (no input, field keeps pulling — enables
  baiting) / Retreat / Line up. No jump, dash, fire, aim, or absorb button.
  Nothing may turn the scheme into an action game.
- **A-006.** Default camera stays centered on the hole; the ring guarantee must
  hold under **every** control mode — no input may hide the ring.
- **A-010 (approved).** The bottom band reserves a **control zone ≥ 180 ru
  wide** (bottom-left/center) — this document decides what lives there. Pause
  is 64 ru bottom-right, the largest HUD target; chips are non-interactive and
  never steal a thumb. The settings surface exposes a **calibration entry
  point owned here**. Touch floors: primary 64 ru, secondary 48 ru, minimum
  44 ru, spacing ≥ 12 ru.
- **A-007 (balance).** Speed decays with mass, displayed not renumbered. Input
  adds **no speed curve of its own**: input translates intent, the sim moves
  the hole.

## 2. Drag (default)

**Finger mechanics.** The player touches anywhere in the play area and drags.
The hole steers toward the finger contact point; it never teleports to the
finger. Pickup is **offset-preserving**: on touch-down the hole keeps its
current position and the finger becomes a steering target at a virtual offset,
so a quick reposition never yanks the hole.

- **Direction + rate intent.** Finger distance from the hole maps to a rate
  (0–100 % of sim speed), not to a velocity. The sim applies its own mass-decayed
  speed to that rate. A finger far away means "go as fast as I can"; a finger
  near the hole means "crawl, be precise". This is what makes precision
  readable at high mass: small finger distance = small step.
- **Hold = no input.** Lifting the finger is a first-class verb: the field
  keeps pulling. Baiting is a finger-lift, not a button.
- **Micro-move dead zone.** Contacts moving under 8 ru within 150 ms are read
  as taps/no-op, not jitter steering, so resting thumbs do not twitch the hole.

**Lift/reposition rules.**

- Lift during play resumes Hold; the next touch-down re-anchors the offset.
- Two-finger repositioning is legal but never required: one-finger drag is the
  only required gesture (spec §4.4).
- A drag that leaves the screen edge releases steering (Hold), it does not
  "wrap" the hole.

**Occlusion tradeoff.** The finger covers the hole it steers — the accepted
cost of direct drag. Policy: the ring guarantee is about the **camera**, not
the finger. The camera keeps the ring framed under drag; the finger may
visually cover play space. Mitigations, in priority order: the hole marker and
ring render above the touch dim, contact point is echoed as a small ring glyph
under the fingertip, and joystick/tilt exist precisely for players who reject
finger-over-target. Occlusion is a tradeoff, never a hidden-state risk.

**Frame range (A-010's 320×568 through 430×932).**

- Intent is normalized: finger distance is measured in **hole radii**, so a
  320-wide phone and a 430-wide phone express the same intent with different
  pixel distances. No control tuning per device.
- Edge exclusion band of 24 ru along screen edges (see §7) keeps palm strikes
  near the bezel from becoming steering on small phones.
- On the smallest frames the control zone (§3) and pause corner are the only
  reserved touch areas; the play area above the bottom band is fully draggable.

## 3. Virtual joystick (alternative)

**Why it exists.** For players who dislike finger-over-target occlusion, and
as the one-handed alternative that keeps both thumbs free of the play area
(A-012 builds on this hook).

**Placement.** Inside A-010's reserved bottom control zone (≥ 180 ru wide,
bottom-left/center). The stick base is **floating within the zone**: touch
down anywhere in the zone places the base there; the thumb never has to find
a fixed pixel target. Nothing else interactive lives in the zone — chips
never steal a thumb (A-010).

**Behavior.**

- Stick vector = direction + rate intent, identical semantics to drag
  (same 0–100 % rate channel into the sim).
- **Dead zones:** inner 15 % of stick radius = Hold (resting thumb does not
  creep); outer 10 % clamps to full rate so max thrust needs no bottoming
  out. Values are the design's; A-013 tests them.
- **Recentering:** lifting the thumb or returning inside the inner radius
  resumes Hold. The base re-anchors on the next touch-down inside the zone.
- The joystick is an **input surface only** — it adds no speed, no assist,
  no different physics.

## 4. Device tilt (alternative)

**Mapping.** Device yaw/pitch (relative to the calibrated neutral pose) maps
to direction + rate: tilt angle beyond the neutral tolerance steers, deeper
tilt means higher rate. Tilt is the one-handed / accessibility alternative
named in spec §4.2 — it needs no screen contact at all.

**Sensitivity.** A slider (low/standard/high) scales the tilt-to-rate mapping;
standard is the default. Sensitivity changes readability of intent, never the
sim's speed.

**Calibration step — answer to spec §17 question 17: YES, tilt requires a
calibration step, and it is an accessibility anchor for A-012.**

- Entry point: the calibration slot in the settings surface (A-010's hook,
  owned here).
- Step: player holds the device in their comfortable neutral playing pose,
  taps "calibrate"; that pose becomes neutral; a ± tolerance band (default
  4°) defines the Hold window. A 3-second guided prompt shows the current
  neutral pose.
- Rationale: neutral varies with grip, seated vs standing, and one-handed
  holds; without calibration tilt is unusable for many players. Calibration
  is stored per session and re-offered on request.
- A-012 owns the option implementations (larger tolerance, one-handed
  variants, reduced-motion interplay); this section defines the hook and the
  default step.

**Ring guarantee under tilt.** Tilt steers the hole; the camera follows the
hole as always, so the ring stays framed. No tilt input may pan the camera
independently of the hole.

## 5. Gesture disambiguation

**Drag vs tap.** Tap is a **menu-context only** verb: taps act on UI targets
(HUD chips are non-interactive per A-010). There is no tap-to-move, no tap-to
absorb. In the play area, a short contact under the §2 dead-zone threshold is
a no-op, never an action.

**Pinch/rotate — answer to spec §17 question 18 (part 2): CONFIRMED as
proposed.** Pinch and rotate are **not** camera controls (zoom is automatic
per the camera contract) and are **reserved for settings-surface use only**
(e.g., a future slider nudge). They are never required for core play and no
gameplay meaning is attached to them.

**No-absorb-button scheme — answer to question 18 (part 1): CONFIRMED.**
Spec §4.1's proposal stands: no attract/absorb button, field always on,
absorption positional. PLAN §5.2 is untouched.

**Pause mis-tap guard — A-010 open question 2: NO hold-to-pause guard.**
Decision: pause stays a single tap on the 64 ru button. Reasons: hold-to-pause
would add a timing verb, which fights the "movement is the whole verb set"
rule; pause is non-destructive and instantly reversible (tap again resumes),
so a mis-tap costs nothing but a frame of attention; risk is already bounded
by A-010's ≥ 12 ru spacing and the corner-island placement (no other
interactive target within reach). A-012 may add a confirm-state variant as an
accessibility option — hook noted in §8.

**A-010 open question 1: no persistent top-band control is needed.** Every
input surface (control zone, pause, calibration via settings) lives in the
bottom band or the settings surface, so the pressure cluster stays exactly as
A-010 laid it out. Nothing here displaces it.

## 6. Input digest

What input emits for replayability (A-005 owns the encoding; this states the
contract):

- **Per simulation tick:** one quantized intent — direction (16-way or 8-bit
  angle) + rate (0–100 %, quantized to 5 % steps), plus an explicit Hold
  state. One record per DT=1/60 tick, not per-frame raw pointers.
- **Discrete events:** control-mode change (drag/joystick/tilt), pause/resume,
  calibration anchor capture, settings changes that affect input (sensitivity).
- **Never emitted:** raw finger coordinates, touch timestamps, device
  telemetry. All schemes (drag, joystick, tilt) funnel into the **same
  intent channel**, so a replay is device-independent and deterministic from
  (seed, inputDigest).

## 7. Touch handling rules

- **Multi-touch policy:** during play, the **first contact wins** as the
  steering source; additional contacts are ignored (no pinch-play, no
  accidental second-thumb steering). A second contact only takes over after
  the first lifts.
- **Cancellation on pause:** pausing discards any in-flight drag/stick intent;
  resuming requires a fresh touch. No stale finger position can resume
  steering across a pause.
- **Palm/edge rejection (small phones):** contacts whose footprint exceeds a
  thumb-sized threshold, or whose first sample lands inside the 24 ru edge
  band, are rejected as steering. Rejection is heuristic and conservative:
  when in doubt, treat as a real drag; the dead zone (§2) plus edge band
  handles the common palm-on-bezel case on 320-wide frames.
- **Touch cancellation on mode switch:** switching drag ↔ joystick ↔ tilt
  cancels the previous mode's live intent; the new mode starts at Hold.

## 8. Accessibility hooks for A-012 (hooks only)

A-012 owns the option implementations; this document guarantees these hooks
exist:

- **Tilt calibration** (§4): settings-surface entry point, neutral-pose
  capture, tolerance band — A-012 adds accessibility variants (wider
  tolerance, one-handed grip profiles, re-calibration prompts).
- **One-handed modes:** tilt needs no screen contact; joystick keeps both
  thumbs out of the play area. A-012 can offer either as the one-handed
  scheme without touching the intent channel.
- **Larger controls interplay:** the 72 ru larger-controls tier is A-012's;
  the ≥ 180 ru control zone and the bottom-band layout leave room for it —
  the zone can host a larger stick base without moving pause.
- **Reduced motion / high contrast:** input is already motion-free at the
  intent level (Hold is a first-class state); A-012's visual options do not
  change the intent channel.
- **Pause confirm-state variant** (§5 hook): if A-012 wants a guarded pause
  for low-precision players, it is an option on the existing single-tap
  button, not a new verb in the default scheme.

## 9. What this makes testable for A-013

- Drag works end-to-end on the smallest (320×568) and largest (430×932)
  A-010 frames; same intent produces same sim result across frames.
- Joystick works: base placement anywhere in the ≥ 180 ru zone; 15 % inner
  dead zone reads as Hold; 10 % outer clamp; mode switch cancels intent.
- Tilt works after calibration; uncalibrated tilt defaults to a safe neutral
  (Hold) rather than drifting.
- Pause target is ≥ 64 ru and single-tap; resume is one tap; no other
  interactive target within 12 ru.
- Multi-touch policy: second contact during play does not steer.
- Pause cancels in-flight intent (no steering across a pause).
- Edge/palm rejection behaves as specified on small frames; a normal thumb
  drag is never rejected.
- Replay determinism: (seed, inputDigest) reproduces play; digest contains
  only quantized intents + discrete events (no raw pointers).
- Dead-zone micro-move suppression (sub-8 ru contacts do not twitch).

## 10. Open questions for downstream artifacts

- **For A-012:** exact one-handed mode UX (which alternative is offered by
  default to one-handed players), larger-controls stick geometry inside the
  180 ru zone, and whether the pause confirm-state ships as an option.
- **For A-013:** thresholds to assert — dead-zone radii (15 %/10 %), 8 ru
  micro-move, 24 ru edge band, thumb footprint threshold — are design values
  here; QA should pin them and flag any that fail on real devices.
- **For A-019 (programmer):** platform tilt API mapping (which axis is yaw
  vs pitch per device orientation) and how the calibration anchor is captured
  from device telemetry; the intent channel in §6 is the required output.
- **For A-005:** digest quantization granularity (16-way vs 8-bit angle, 5 %
  rate steps) is stated here as the contract; encoding and replay harness
  remain A-005's.
