# P-01 / P-05 device capture — modern phone (S24 FE), 2026-10-17

Row format per A-014 §5: `device-class, os-build, test-ID, metric, measured, bound, pass/fail, timestamp, build-hash`.

- device-class: modern phone (A-014 §6 reference class; real device SM-S721B, serial RZCXB2BSF8P)
- os-build: Android 16, `ro.build.id BP4A.251205.006` (recorded 2026-10-11)
- build-hash: sha256 b7a3f3d5… (prefix as recorded), 21,284,066 B — the A-023 ELEVENTH-extension package (RGBA32 alpha textures, both-axis §5.3 world-fit, minimal A-011 drag wiring), merged at 88f20df and installed on the phone
- capture tool: `tools/player-package/capture.sh` (single-shot pipeline; WAIT=120, the user reconnected the cable AND played during the window — real touches)
- timestamp: 2026-10-17 (logcat run; EH-PERF t= values span the warm-up + 120 s window)

## Measured rows (printed by the game itself, not inferred)

- EH-BOOT `level=p1-level-01 targetFps=60 tier=0` — Start()/Update() run; our marker present.
- EH-PACE `target=60 vsyncsPerSecond=1` — the engine's limiter is OURS (Application.targetFrameRate=60 read back).
- EH-SPRITE `source=generated format=4` — format=4 is `TextureFormat.RGBA32`: the alpha-carrying textures are in the running instance (the no-alpha default was the square-sprite cause).
- EH-CAMERA / EH-FRAME present — the camera and the frame loop run.
- EH-DRAW (frozen for the whole window): `frame=536 cam=64,48 ppu=8.4375 horizonPx=307 visible=0` — the last completed draw() was frame 536 (t≈9 s). ppu=8.4375 = min(1080/128, 2340/96): the both-axis fit works (the world fits the window width; 128×8.4375 = 1080 px).
- EH-PERF lines PRESENT: 3,902 frames measured, frame counts advance to 4,499 while EH-DRAW stays at 536.
- P-01, metric=p50_ms, measured=16.625, bound=(informational), —, 2026-10-17, b7a3f3d5…
- P-01, metric=p95_ms, measured=18.125, bound=16.6, FAIL, 2026-10-17, b7a3f3d5…
- P-01, metric=worst_hitch_ms, measured=29.28, bound=(informational), —, 2026-10-17, b7a3f3d5… (this is the first-touch frame, frame 536)
- P-05, metric=allocs_per_frame_max, measured=0, bound=0, PASS, 2026-10-17, b7a3f3d5… (steady across the measured frames)
- visual: screencap read all (0,0,0) with Display State=DOZE_SUSPEND → INCONCLUSIVE (black + display dimmed = environment, never a game verdict).

## Findings (measured, not assumed)

1. DEFECT D (the first-draw freeze of the tenth build) IS GONE: EH-PERF appears and the
   frame loop RUNS at 60 FPS pacing (p50=16.625 ms = the exact 60 Hz vsync period).
2. A SECOND, DIFFERENT freeze is now proven: EH-DRAW froze at frame=536 (t≈9 s — the
   first touch; the run's worst_hitch=29.28 ms is that same frame) while EH-PERF kept
   counting to 4,499. Root cause settled from the code, not guessed: a 29.3 ms frame
   with accumulator debt is a TWO-STEP catch-up frame; `FixedStepDriver.advance` moves
   `sim.stepIndex` by 2, and the next frame's `InputAdapter.sampleIntent` hard-rejects
   "step must advance exactly one per sample" — the frame aborts BEFORE frameLive,
   stepIndex never advances again, and the rejection repeats every frame FOREVER. The
   live path permanently breaks its own one-sample-per-step invariant on ANY multi-step
   frame. The same latent break already happened in the 2026-10-16 capture at the 57.8 s
   DOZE suspend (its EH-DRAW stayed at frame=2495 while EH-PERF continued — unnoticed
   then, same signature). The user's report "same problems" = the game freezes on the
   first touch.
3. BOUND ARITHMETIC (unchanged finding, now a second data point): the exact 1/60 period
   is 16.667 ms; the measured best-achievable p50 is 16.625 ms; A-014 §6's P-01 bound
   of ≤16.6 ms is BELOW the physical vsync period, so a perfectly-paced 60 FPS app can
   never satisfy it (p95=18.125 = one vsync step + ~1.5 ms of occasional hitch pairs).
   The bound must be re-derived against the vsync period — that is A-014's/A-007's
   decision, never softened here.
4. The starfield covered only the world bounds, so the §5.3 both-axis fit leaves bare
   bands top/bottom on a portrait window (the user's "content from the first third
   down"); the twelfth extension makes the field layer cover the presented view
   (PLAN.md §4 A-023 records the decision).
5. The visual proof of the Interstellar look is STILL OPEN: the display entered
   DOZE_SUSPEND during the capture (≈75 s after the last real touch). The user playing
   actively during the window keeps it ON; the next capture needs that plus the
   twelfth-extension build (7240678d… 21,284,329 B).

## What the NEXT capture (twelfth build) must show

- EH-DRAW `frame=` must TRACK EH-PERF `frame=` across the whole window, including past
  the first touch — no more EH-DRAW stuck at 536 while EH-PERF counts on. That closes
  defect D.
- The boot-warm attempt (one discarded `touchCount` read at Start) is decided by whether
  worst_hitch_ms drops below the 29.28 ms first-touch value; an attempt, not a claim.
- The screencap with the display ON must show the approved look: true-black sphere +
  accretion glow + lensing halo, glowing stars not flat discs, the field reading as
  space across the whole portrait frame.
- P-01 stays FAIL against the 16.6 bound until A-014/A-007 re-derive it; P-05 must stay
  allocs_per_frame_max=0.
