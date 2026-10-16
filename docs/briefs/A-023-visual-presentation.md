# Brief: A-023 tenth extension — implement the approved visual language in the player package

Artifact: A-023 Unity player package, TENTH extension. Agent: programmer.
Branch: `feature/programmer-player-visual-presentation` (already created from
develop; registered in PLAN.md §1.3). Commit prefix: `code(player-package):`.
You draft; the coordinator reviews against PLAN.md and merges. PLAN.md wins on
disagreement.

## What to implement (one file)

`Assets/Runtime/Bridge/Unity/RenderLayer.cs` ONLY. Implement the visual
language the designer just merged into docs/design.md (A-006 extension,
develop at f6892c8) — the user's directive: the black hole must be presented
as in the film Interstellar, and the bodies must read as proper glowing stars,
not flat discs. Read docs/design.md §3.2 (bodies-as-stars note), §3.3
(fourth decorative channel + starfield layer), §3.5 (player-package layer
contract), §3.6 (motion) — those are the approved spec; the brief below
distills the parts THIS artifact depends on.

## The approved layer contract (docs/design.md §3.5, distilled)

Farthest to nearest (z-order; the camera stands at z = -10, sprites face it
with larger z nearer):

1. **Starfield** — purely visual, static, the WEAKEST element: small faint
   points. Implementation freedom: ONE boot-generated background texture on a
   single sprite scaled to the world bounds (preferred: one draw call, static
   by construction) — never drifts, never carries readability. It must stay
   dimmer and smaller than even the smallest body's core.
2. **Accretion glow** — soft-alpha radial-gradient sprite(s) wrapped at the
   horizon rim, BEHIND the true-black sphere, so only a rim-wrapped band and
   a halo show. Slow rotation is allowed (design §3.6: a render constant
   owned by A-023; keep it in ONE place so A-012's reduced-motion option can
   flip it later — do NOT implement the option itself).
3. **True-black sphere** — the darkest thing on screen (keep the existing
   black disc; the field clear is pack_px(8,10,18), so the sphere reads only
   because of its glow and ring — that is the point).
4. **Rim ring** — bright, constant screen-space width (min 2 px; the current
   4 px via `insetWu = 4.0 / ppu` is approved and stays). Either the merged
   two-disc layering (rim disc behind + smaller black disc over — it already
   produces exactly this look) or a generated ring texture in front; pick
   whichever keeps the API surface to already-settled names and RECORD which
   you chose.
5. **Bodies** — nearest layer. Replace the flat circle with a generated
   "glowing star" texture: bright core + one soft halo fading to transparent
   (design: halo 2-3x the core radius, capped at 1/8 of the horizon radius so
   glows never merge into a field that hides the ring). Colour stays from
   SpriteRenderer.color via the existing `bodyColorInto` palette (tier bits
   `(kind >> 2) & 3` stay the identity channel — never encode identity in
   colour alone). Hazards keep the same palette family; the glow may sit
   behind a hazard core.

Glow rule (hard): layered soft-alpha sprites ONLY. No additive blending, no
custom shaders, no materials — none of those are proven on this package (the
built-in sprite archive lacks even Default-Circle). Alpha lives in the
generated texture; colour lives in SpriteRenderer.color.

## Measured facts you must honour (PLAN.md §4 A-023, ninth extension — merged)

- Built-in sprite lookup returns NULL on this package; boot-generated
  textures work: `Texture2D.SetPixels(Color[])` + `Sprite.Create(Texture2D,
  Rect, Vector2)` are settled names (the ninth extension compiled them).
- The camera at z = -10 with sprites at z in [-2, 0] presents correctly
  (near/far are -1000/1000).
- `EH-SPRITE source=` observability line exists; keep it and its meaning.
- The capture tool now writes `/data/local/tmp` (Android 16: /sd is
  read-only) — do not touch capture.sh.

## Hard rules (unchanged from the merged artifact)

- RenderLayer is STRICTLY READ-ONLY against SimState; no field of `s` is
  assigned.
- The §5.3 camera rule stays byte-identical in behaviour: shortEdge from the
  active window, ppuWorld/ppuContain/min/clamp, tierPPU upper bound. Do not
  re-decide framing.
- Pools allocated at BOOT only; steady-state frames allocate ZERO (P-05
  measured 0 and must stay 0: every new texture, sprite, and material-free
  object is boot work; per-frame code reuses the tmpPos/tmpScale/tmpColor
  temporaries and boot-cached Transforms; a reused UnityEngine.Quaternion
  variable is a value type — no per-frame allocation for the disc rotation).
- Draw order stays ascending body index; inactive records stay inactive.
- Culling against the active window stays (P-08 draw-call discipline): the
  starfield is ONE sprite, not a per-star draw.
- No float/double reaches SimCore (the doubles here are render-side only).
- The headless surface must not move: RenderLayer.cs is excluded from
  harness/player headless builds by their Exclude clauses — touch NOTHING
  else.

## API rule (the compiler is the oracle)

Author against Unity 6 LTS 6000.0.84f1. If a name is uncertain, check the
engine's own stubs (`Assets/..../Data/Managed/UnityEngine/UnityEngine.*.xml`
under the editor at /home/dfcastro/Unity/Hub/Editor/6000.0.84f1) or a
temporary probe file under Assets/Runtime/Bridge/Unity (deleted after), with a
validation line that MUST appear in the error list for the evidence to count.
Record any rejected name and the settled replacement in the file's comments —
nothing may pass quietly.

## Verification you must run (the license gate is ACTIVE now)

1. `tools/player-package/build.sh` → must print "Build Finished, Result:
   Success." and write Build/EventHorizon-Android.apk; report the APK size and
   sha256.
2. `git diff --stat` against develop: ONLY RenderLayer.cs moves.
3. Do NOT run the device capture (the phone is not USB-connected) — the visual
   proof and P-01 re-measure stay open for the coordinator; never claim them.

Return: the edited file, the decisions taken (ring layering choice, starfield
choice, texture recipes, rotation constant), any rejected API names, the build
outcome, and the open questions you deferred (device capture, A-012 reduced
motion, A-010 UI). Do not commit, do not merge.
