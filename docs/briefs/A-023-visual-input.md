# Brief: A-023 eleventh extension — the four defects the user SAW on the phone

Artifact: A-023 Unity player package, ELEVENTH extension. Agent: programmer.
Branch: `feature/programmer-player-visual-input` (created from develop;
registered in PLAN.md §1.3). Commit prefix: `code(player-package):`. You draft
on the branch; the coordinator reviews and merges. PLAN.md wins on
disagreement — it was updated FIRST (commit 283af76 docs(plan):).

The user played the tenth-extension build on the S24 FE and reported
(verbatim intent): "everything square, slow, I don't see the Interstellar
black hole, I touch the screen and nothing moves, everything is drawn from
the first third of the screen down". The captured frame and the game's own
log name the causes. Four defects, three to FIX now, one to INVESTIGATE:

## A. SQUARE SPRITES — fix: generated textures must carry alpha

MEASURED: bodies render as hard-edged squares; the circle mask and every
soft-alpha glow is ignored. CAUSE: Unity 6's two-argument
`new UnityEngine.Texture2D(w, h)` defaults to a NO-ALPHA graphics format —
the engine's own stubs (`UnityEngine.CoreModule.xml`, member names
`M:UnityEngine.Texture2D.#ctor(...)`) list `(Int32,Int32)` and
`(Int32,Int32,UnityEngine.TextureFormat,Boolean)` among the overloads.
FIX: every boot-generated texture (the ninth-extension fallback circle,
the body glow, the accretion band, the starfield) must use the FOUR-argument
constructor with alpha=true. The stubs do NOT list the TextureFormat enum
members, so the compiler is the oracle: probe candidate members
(`UnityEngine.TextureFormat.RGBA`, `.RGBA16`, `.RGBA32`) with the
established ApiProbe pattern — a temporary file under
Assets/Runtime/Bridge/Unity with a validation line that MUST appear in the
error list for the evidence to count — build, read the error list, pick the
accepted name, DELETE the probe file. Record the settled name and every
rejected candidate in the RenderLayer comment. Extend the EH-SPRITE line to
print the format used (a printed fact, per the observability directive).

## B. PORTRAIT LETTERBOX — fix: the §5.3 world-fit term, as updated in PLAN.md

MEASURED: on the portrait window the world rendered only in a band (the
user's "first third down" report; the captured frame shows content in a
middle band). PLAN.md §5.3 was updated FIRST to the BOTH-AXIS fit:

```text
ppuWorld = min(windowWidthPx / boundsW, windowHeightPx / boundsH)
```

In RenderLayer.draw(): read boundsW (already read for the starfield
placement) and boundsH, compute the both-axis fit exactly as §5.3 now
states, keep the rest of the rule byte-for-byte in behavior (ppuContain,
min, clamp to [MIN_PPU, tierPPU], orthoSize = windowHeightPx / ppu / 2).
The Q32.32 reads stay full quotients. For landscape windows this reduces to
the approved value (recorded in §5.3), so A-026's proof numbers are
unchanged by construction.

## C. TOUCH DOES NOTHING — fix: wire A-011's drag into the transport

MEASURED: GameLoop.pushLiveIntent is a `pause()` stub — the candidate build
NEVER read a finger. A-011 (docs/input.md §2, the approved default scheme):
touch + drag TRANSLATES the hole; the intent channel is direction + rate.
Wire it honestly and minimally in GameLoop (the platform side):

- Read Unity's live touch each frame (probe the names the same way as A:
  candidates `UnityEngine.Input.touchCount`, `UnityEngine.Input.GetTouch(int)`
  — the built-in mobile input module; the Input System package is NOT
  installed, do not reach for it).
- Maintain a drag anchor: on first contact with no anchor, anchor = current
  finger position (screen px); while a contact is held, the drag vector =
  current − anchor; when no contact, clear the anchor and call
  InputAdapter.pause() (coast/neutral — the existing rule).
- Feed the transport contract ONLY:
  `InputAdapter.pushIntent(thrustX, thrustY, rate)` with Q32.32 int64
  values (FixedQ.ONE = 1.0): direction = normalized drag vector (floats
  allowed HERE — platform side, never into SimCore), rate =
  min(dragLength / DRAG_FULL_PX, 1) as Q32.32, DRAG_FULL_PX a named
  constant (e.g. 120 px to full rate — record it as a placeholder for
  A-011's tuning, not a re-decided scheme).
- first-contact-wins and pause-cancels stay in InputAdapter (do not touch
  it); one pushIntent per frame is fine — the adapter locks the first per
  step and discards the rest.
- Record in the comment: this is the MINIMAL drag wiring; A-011's full
  scheme (offset-preserving pickup, dead zone, edge band, re-anchor) is a
  later refinement; the transport contract and the sim are untouched.

## D. THE FREEZE — investigate, do not paper over

MEASURED: the log shows EH-BOOT/EH-PACE/EH-SPRITE/EH-CAMERA/EH-FRAME and
then ZERO EH-PERF lines — Update ran once and stopped; an earlier run left
an il2cpp dropbox report (native crash) while the user's process was alive
but suspended. The cause is NOT proven. Do NOT add try/catch or any
softening. The fixes in A/B/C touch exactly the code that runs in the first
draw() — after them, the NEXT device capture either shows EH-PERF (the
freeze was in the code being fixed) or the Unity log will name the fault.
Leave a comment recording the open investigation and what would decide it.

## Hard rules (unchanged)

- RenderLayer strictly read-only against SimState; GameLoop feeds the same
  FrameDriver/InputAdapter contracts; no float/double into SimCore (the
  floats for touch/camera are platform/render side only).
- §5.3 as updated in PLAN.md; pools boot-only; steady-state frames allocate
  ZERO (the touch read returns value-type structs; the drag anchor is two
  longs/floats in GameLoop instance fields — no per-frame `new`).
- Draw order ascending body index; culling keeps deactivating glow with
  core; EH-* observability lines stay and extend honestly.
- Touch NOTHING outside Assets/Runtime/Bridge/Unity (RenderLayer.cs,
  GameLoop.cs, plus the temporary probe, deleted after). No ProjectSettings,
  no docs, no tools.

## Verification you must run

1. The API probe build(s) — record accepted/rejected names.
2. `tools/player-package/build.sh` → must print success with the package on
   disk; report the APK size + sha256.
3. `git diff --stat` against develop: ONLY RenderLayer.cs and GameLoop.cs
   move (the headless surface is unchanged by construction — both files are
   excluded from the headless builds).
4. Do NOT run the device capture (the phone is disconnected; the
   coordinator runs it after the user reconnects). Never claim the visual
   proof or P-01 here.

Return: decisions (the settled TextureFormat name, the settled touch names,
the drag constants), rejected names with evidence, the build outcome, and
what the next capture must decide for D. Do not commit, do not merge.
