# Device result rows — modern phone class (A-014 §5 format)

Rows:

```text
modern-phone, Android 16 (API 36) BP4A.251205.006 SM-S721B s5e9945, P-01, p95_frame_time_ms, 34.375, <=16.6, fail, 2026-10-11T19:28:20 local, sha256 279b48003bd5bddd1dc132ae14d7d262aac05ee8d6f68bd1328910f9b9229328
modern-phone, Android 16 (API 36) BP4A.251205.006 SM-S721B s5e9945, P-01, worst_hitch_ms (display-ON segments), 38.301, <=50, pass, 2026-10-11T19:22:25 local, sha256 279b48003bd5bddd1dc132ae14d7d262aac05ee8d6f68bd1328910f9b9229328
modern-phone, Android 16 (API 36) BP4A.251205.006 SM-S721B s5e9945, P-05, allocs_per_frame_max, 0, 0, pass, 2026-10-11T19:28:20 local, sha256 279b48003bd5bddd1dc132ae14d7d262aac05ee8d6f68bd1328910f9b9229328
```

## Context

- Device: S24 FE, serial `RZCXB2BSF8P`, model `SM-S721B`, Android 16 (API 36),
  `ro.hardware s5e9945`, display 1080x2340 @ 120 Hz (meets the 60 Hz+ class bound),
  Dynamic AMOLED 2X, 16M colour, 6.7". `MemTotal: 7397720 kB`.
- Build: player package `Build/EventHorizon-Android.apk`, 21,273,037 B,
  sha256 `279b48003bd5bddd1dc132ae14d7d262aac05ee8d6f68bd1328910f9b9229328`.
- Method: `realtime` — in-engine clock (`UnityEngine.Time.realtimeSinceStartup`),
  histogram 1024 buckets x 250 us, bucket-centre percentiles (±0.125 ms resolution).
- Boot evidence: `EH-BOOT level=p1-level-01 targetFps=60 tier=0` and
  `EH-FRAME first frame seen by the measurement surface` (pid 3989, 19:21:25).
- Windows: two continuous display-ON segments after the 10 s warm-up:
  19:21:35→19:22:25 (50 s, 1503 steady frames) and
  19:26:33→19:28:20 (107 s, ~2400 frames). ~157 s of presented steady state total.
- Raw EH-PERF lines (last of each segment):

```text
19:22:25.830 EH-PERF method=realtime frame=1805 t=60042713 p50_ms=33.375 p95_ms=34.625 p999_ms=37.875 worst_hitch_ms=38.301 frames=1503 allocs_warmup_max=917 allocs_per_frame_max=0
19:28:20.840 EH-PERF method=realtime frame=4686 t=415052826 p50_ms=33.375 p95_ms=34.375 p999_ms=54.125 worst_hitch_ms=244013.222 frames=4384 allocs_warmup_max=917 allocs_per_frame_max=0
```

## Caveats (stated, not hidden)

- The 244,013 ms "hitch" in the second line is the display-off/doze period
  (19:22:25→19:26:33) where the compositor presented no frames; it is not a
  game hitch. Within display-ON segments the worst hitch is 38.3 ms.
- The 120 s continuous window of A-014 §6 could not be held in one segment
  because the S24 FE display auto-dozes ~75 s after the last real touch and
  synthetic taps do not reset the doze timer; the two segments sum to ~157 s.
- P-01 FAIL is a real finding: the game targets 60 FPS (`targetFps=60 tier=0`)
  but presents at the 30 FPS vsync step (p50 = 33.375 ms = exactly the 120 Hz
  quarter step). Cause (vsync policy vs render cost) is open; follow-up required.
- P-05 PASS: `allocs_per_frame_max=0` across all 4,384 steady frames;
  `allocs_warmup_max=917` is warm-up-only by construction.
