# Brief: A-006 extension — visual language: the Interstellar presentation

Artifact: A-006 docs/design.md, EXTENSION (the artifact is done; this extension
adds one presentation decision the user asked for from a real device report).
Agent: designer. Branch: `feature/designer-visual-language` (created from
develop; registered in PLAN.md §1.3). Commit prefix: `docs(design):`.
The coordinator reviews against PLAN.md and merges; you do not merge.

## The directive (user, 2026-10-16, verbatim intent)

The user looked at the game on a phone and reported: NO black hole is visible;
the dots read as "stars but very poorly made"; the run reads as slow. The
user's design decision: **the black hole must be presented as it is in the film
Interstellar** — a true-black sphere wrapped by a bright accretion disc of
glowing gas, with a lensing halo — and the field must read as a starfield with
the bodies as proper glowing stars, not flat discs.

PLAN.md §4 A-006 and §4 A-023 already carry this record (commit 4d8c5de
docs(plan):). PLAN.md wins on disagreement.

## What to write

Extend docs/design.md §3 (Visual language) — do not replace its rules, extend
them. Required content:

1. **§3.2 black-hole row, extended**: the Interstellar presentation — true-black
   sphere (still the darkest thing on screen), wrapped by a bright accretion
   disc of glowing gas at the horizon rim, plus a faint lensing halo. Keep the
   rationale column honest: the ring stays the strongest element (readability
   hierarchy `event horizon ring > hazard markers > large bodies > medium >
   small > field texture` must survive).
2. **§3.3 event-horizon rendering, extended**: the three readability channels
   (ring / interior vacuum / motion) stay; add the accretion-disc and lensing
   language as a FOURTH decorative channel that must never carry readability by
   itself, and the starfield field layer (purely visual, decoupled from the sim
   — §3.3 already allows field texture; it must stay the WEAKEST element so
   bodies never compete with background stars).
3. **Bodies as stars**: the absorbable bodies (PLAN §5.7: dust, pebbles, ice
   fragments, debris / rocks, metal shards, ship fragments, asteroids /
   asteroid chunks, small planets, star fragments, quantum anomalies; hazards:
   mines, gravity anchors, magnetic fields, collapsing debris, massive objects)
   must read as glowing points/stars with soft halos at small sizes while
   keeping the design's shape language: rounded/organic for food, sharp/spiky +
   marker glyph for hazards, never color-only (colorblind anchor for A-012).
   State sizes and brightness as RATIOS (of the horizon radius and of the
   background), never pixels or hex palettes — A-023 owns the concrete palette
   (it already mirrors A-026's view.c palette; the tier bits (kind>>2)&3 stay
   the identity channel).
4. **A "player-package presentation contract" subsection**: the concrete layer
   list A-023 will implement: z-order (starfield farthest, accretion glow
   behind the true-black sphere, bright rim ring, bodies nearest), what each
   layer is (boot-generated textures only — the package ships NO asset files),
   and the rule that glow must be expressible as layered soft-alpha sprites
   (additive blending is NOT guaranteed on this package — the built-in sprite
   archive lacks even Default-Circle; the programmer settles API names by
   compiling, so the design must not depend on unproven shaders or materials).
5. **Motion readability**: the user read the run as slow. The design may state
   presentation-side motion cues (the accretion disc's slow rotation, bodies
   drifting toward the hole — that drift IS sim behavior rendered as-is) but
   must NOT change the §5.3 camera rule, any A-007 number, or any A-009 level
   number, and must keep the reduced-motion option (A-012) honest: name what
   turns off at reduced motion.
6. An **extension record** dated 2026-10-16 in the document's own style: what
   was asked (the device report), what is decided here, and that sim numbers
   and golden replays are untouched.

## Hard boundaries (scope)

- No sim numbers, no §5.3 camera rule change, no level content, no UI layout
  (A-010), no input (A-011). Visual decisions only.
- "Must never imply a sim effect that does not exist" (existing §3.3 rule):
  lensing and the accretion disc are VISUAL ONLY; the pull/absorb model is
  §5.6's and stays untouched.
- Do not make the game a pure power fantasy (PLAN §8): the presentation must
  not hide the tension cues; the horizon ring and field readability stay
  primary.
- No-softening (A-012/A-014): visuals may never change digest identity — say
  so explicitly where the lensing/starfield layers are introduced.
- Keep the existing contrast hierarchy and hazard shape family intact.

## Upstream facts you may rely on (measured, from PLAN.md §4 A-023)

- The player package draws with SpriteRenderer + boot-generated textures
  (Texture2D.SetPixels + Sprite.Create); the built-in sprite lookup returns
  NULL on this package.
- The camera is runtime-created, orthographic, at z=-10; z-order body > disc >
  rim works; the field clear colour is pack_px(8,10,18) (A-026's palette, one
  palette for both halves of the same §5.3 rule).
- A pure-black disc on that near-black field with a 4 px rim did NOT read as a
  hole on the device — the extension must fix exactly that readability gap.

Write-first, incremental: extend the existing sections in place. Return the
edited docs/design.md path and a summary of decisions; leave committing and the
PR to the coordinator.
