# Brief: A-023 twelfth extension — the live-path input fix and the view-covering starfield

Artifact: A-023 Unity player package, TWELFTH extension. Agent: programmer.
Branch: `feature/programmer-player-live-input` (from develop; registered in
PLAN.md §1.3). Commit prefix: `code(player-package):`. You draft; the
coordinator reviews and merges. PLAN.md wins on disagreement — it was updated
FIRST (commit 6b336d7 docs(plan):).

The eleventh build's device capture (2026-10-17) proved: the frame loop RUNS
(EH-PERF 3,902 frames, p50=16.625 ms = 60 FPS, P-05 allocs=0) but EH-DRAW
froze at frame=536 (t≈9 s — the first touch, whose first GetTouch frame is
also the run's worst_hitch=29.28 ms). Root cause (settled, recorded in PLAN.md
§4 A-023): a 29.3 ms frame with accumulator debt is a TWO-STEP catch-up frame;
`FixedStepDriver.advance` moves `sim.stepIndex` by 2, and the NEXT frame's
`InputAdapter.sampleIntent` hard-rejects "step must advance exactly one per
sample" — the frame aborts BEFORE frameLive, stepIndex never advances again,
the rejection repeats FOREVER. The same latent break happened at the 2026-10-16
DOZE suspend (EH-DRAW stayed at frame=2495). The user's report: "same problems"
— the game freezes on the first touch.

## Fix 1 (the freeze) — GameLoop.cs only, per A-022's boundary

A-022's own words (FrameDriver.cs header, docs): "frameLive is the LIVE path:
the platform layer hands over ONE already-resolved intent for the frame; at 30
fps a frame delivers two steps and both share it". The per-step CODE CHANNEL
(InputAdapter + InputDigest.Codes) belongs to the DETERMINISTIC replay path
(H-02), not to the live path. So in GameLoop:

- REMOVE the live path's `pushLiveIntent → InputAdapter.sampleIntent(sim.stepIndex)`
  round-trip. Resolve the drag each frame (the existing anchor/drag code stays)
  into ONE Intent INSTANCE held in a GameLoop field (allocated once at Start —
  never `new` per frame; P-05 must stay 0), and pass it straight to
  `FrameDriver.frameLive(clock, sim, i)`.
- Quantize through the SAME InputDigest tables so the identity channels are
  identical, no second invention: `angle = InputDigest.quantizeAngle(thrustX,
  thrustY)`; if `angle == InputDigest.ANGLE_COAST` set thrustX=thrustY=0,
  else thrustX = InputDigest.ANGLE_X[angle], thrustY = InputDigest.ANGLE_Y[angle];
  rate: `rate = InputDigest.rateFromCode(InputDigest.quantizeRate(rateRaw))`.
  No-contact frames: coast/zero (the same neutral intent — A-011's coast).
- Keep `InputAdapter.boot(...)` at Start (the transport slot stays documented
  for the deterministic path) with a comment saying the live path no longer
  samples the code channel and WHY (the invariant cannot hold when one frame
  covers many steps — A-022's boundary).
- Warm the touch subsystem at boot (one `UnityEngine.Input.touchCount` read at
  Start, result discarded): the first-touch 29 ms hitch moves to boot cost
  (counted in the warm-up max, never in a steady frame). Record it as an
  attempt, not a claim — the next capture decides.
- No try/catch, no softening anywhere. Read-only against SimState. No float
  into SimCore (the drag floats are platform-side, converted before the
  tables).

## Fix 2 (the bands) — RenderLayer.cs: the starfield covers the PRESENTED VIEW

The §5.3 both-axis fit leaves bare bands on a portrait window (a landscape
world in a tall screen) — the user reads "content from the first third down".
The field layer is visual (A-006 §3.3): make the starfield cover the
PRESENTED VIEW rectangle, not the world bounds: each frame, scale =
(windowW/ppu, windowH/ppu) centered on the camera position (the hole), farthest
z as now. It stays screen-static (it moves exactly with the camera, never on
its own — §3.6's "never drifts"), still the weakest element (same texture,
same alpha rule), one draw call. Remove the one-time world-bounds placement;
the world bounds stay the play area and the culling rule unchanged. Record in
the comment: the bands are inherent to a landscape world on a portrait window
under the approved fit; the field layer now reads as space everywhere.

## Hard rules (unchanged)

Touch ONLY Assets/Runtime/Bridge/Unity/GameLoop.cs and RenderLayer.cs. The
§5.3 rule, pools, zero steady allocations, ascending draw order, culling,
EH-* observability (keep EH-SPRITE/EH-PACE/EH-CAMERA/EH-DRAW as-is; EH-DRAW
now also proves the fix: lastDrawFrame must track frameCount again).

## Verification you must run

1. `tools/player-package/build.sh` → success + APK size/sha256 (the license
   gate is active).
2. `git diff --stat` against develop: ONLY the two files.
3. Do NOT run the device capture (the coordinator runs it with the user
   playing). Never claim the visual proof or P-01 here.

Return: the decisions (intent reuse site, quantization path, starfield
sizing), the boot-warm outcome as an attempt, the build result, and what the
next capture must show to call defect D closed (EH-DRAW frame tracking
EH-PERF frame across the whole window, including past the first touch).
