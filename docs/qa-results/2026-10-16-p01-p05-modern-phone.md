# P-01 / P-05 device capture — modern phone (S24 FE), 2026-10-16

Row format per A-014 §5: `device-class, os-build, test-ID, metric, measured, bound, pass/fail, timestamp, build-hash`.

- device-class: modern phone (A-014 §6 reference class; real device SM-S721B, serial RZCXB2BSF8P)
- os-build: Android 16, `ro.build.id BP4A.251205.006` (recorded 2026-10-11)
- build-hash: sha256 6288a388432cb3264e5c51741ad2704b124fe2057baf4f2022c34c944fbd4bbf, 21,273,946 B (eighth-extension package: Application.targetFrameRate + EH-PACE + EH-CAMERA/EH-DRAW)
- capture tool: `tools/player-package/capture.sh` (single-shot pipeline; this file is its output record)
- timestamp: 2026-10-16 (logcat run; EH-PERF t= values span 15.0 s .. 100.0 s of measured time)

## Measured rows (printed by the game itself, not inferred)

- EH-BOOT `level=p1-level-01 targetFps=60 tier=0` — Start()/Update() run; our marker present.
- EH-PACE `target=60 vsyncsPerSecond=1` — the engine's limiter is OURS (Application.targetFrameRate=60 read back); FrameTimingManager reports vsyncsPerSecond=1 at boot (feature enabled; value semantics NOT assumed — recorded as printed).
- EH-CAMERA `created=1 clearFlags=2 orthographic=False near=-1000 far=1000` — clearFlags=2 is the SolidColor ordinal: the camera fix is in the running instance. orthographic=False at boot is expected (draw() sets it per frame; EH-DRAW shows orthoSize=104).
- EH-DRAW (last rows): `frame=2495 cam=64,48 ppu=11.25 horizonPx=429 visible=5 clearFlags=2 orthoSize=104` — draw() presents content: horizon growing 410→429 px, 5 visible bodies, contained scale.
- P-01, metric=p95_ms, measured=17.625, bound=16.6, FAIL, 2026-10-16, 6288a38…
- P-01, metric=p50_ms, measured=16.625, bound=(informational), —, 2026-10-16, 6288a38…
- P-05, metric=allocs_per_frame_max, measured=0, bound=0, PASS, 2026-10-16, 6288a38… (steady across 1,934+ steady-state frames)

## Findings (measured, not assumed)

1. The 30 FPS defect is GONE: p50=16.625 ms = the exact 60 Hz vsync period; the previous
   p95=34.4 ms (30 FPS quarter step) no longer occurs. The cause was the engine's frame
   limiter never being set; the eighth extension sets it (EH-PACE target=60 read back).
2. BOUND ARITHMETIC: the exact 1/60 period is 16.667 ms; the measured best-achievable
   p50 is 16.625 ms; A-014 §6's P-01 bound of ≤16.6 ms is BELOW the physical vsync
   period, so a perfectly-paced 60 FPS app can never satisfy it. p95=17.625 = one
   vsync step + ~1 ms. The bound must be re-derived against the vsync period
   (PLAN.md §4 A-014 updated with this finding; the bound itself is A-014/A-007's
   to re-derive, not silently softened here).
3. worst_hitch_ms=57822.659 with a t-jump 40.0 s → 99.0 s: the environment suspended
   the app (display entered DOZE_SUSPEND; the phone was untouched). Recorded as an
   environment artifact, not a game hitch.
4. Visual proof STILL OPEN: display-state DOZE_SUSPEND at capture time and the
   screencap failed (USB flake); synthetic taps do not reset the doze timer (recorded
   2026-10-11). The visual needs one human action: physically wake/open the phone,
   then re-run capture.sh. The EH log decides the sim/pacing gates independent of
   display state — that is what the observability directive bought.
