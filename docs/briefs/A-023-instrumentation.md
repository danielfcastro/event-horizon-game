# Brief: A-023 extension — player measurement surface (P-01, P-05)

Branch: `feature/programmer-player-instrumentation` (created from develop). Agent: programmer. This is an extension of the done artifact A-023 (Unity player package); PLAN.md §4 A-023 already records the decision and the API facts.

## Contract

Add the measurement surface A-014 §6's "needs, not design" item 3 asks for, so P-01 (frame time) and P-05 (per-frame allocations) become measurable on a real device:

1. A frame-time counter: per-frame real-time deltas accumulated into a fixed-bucket histogram (no per-frame allocation), plus worst hitch.
2. A per-frame allocation counter for the player's own per-frame code (P-05: "per-frame allocation counters must read 0 after warm-up").
3. `RenderLayer.draw()` made allocation-free per frame: reuse `Vector3`/`Color` temporaries — the current code allocates per frame, which is what P-05 targets.
4. Summary lines emitted through `UnityEngine.Debug.Log` so they are readable on the device via `logcat`.

## Inherited rules (PLAN.md wins on any disagreement)

- Same SimCore, `DT = 1/60` unchanged; the §5.3 camera rule unchanged; the render side never writes simulation state; no-softening: the measurement surface changes nothing the player can perceive except log lines.
- A-019 §12 ship-build exclusions must stay green (no `dev.*` knobs reachable, no harness in the package). The measurement surface is a QA instrument, not a gameplay feature; it may log, it may not gate.
- Never green a gate without a measurement; never claim a number you did not print.

## A-014 excerpts (the only upstream you need)

Result row format (one row per test per device, committed to `docs/qa-results/`):

```text
device-class, os-build, test-ID, metric, measured, bound, pass/fail, timestamp, build-hash
```

- P-01: "Modern phone, level 40 peak load, 120 s; p95 frame ≤ 16.6 ms, no hitch > 50 ms." Windows: 120 s steady-state after a 10 s warm-up; report p50/p95/p99.9 frame time and worst hitch.
- P-05: "Steady state allocates from pools only; per-frame allocation counters must read 0 after warm-up (nonzero = leak)."
- P-04 RSS stays OS-side via `dumpsys meminfo` (already measured; do not re-invent an RSS counter in C#).
- A device class with no recorded row counts as a fail.

## Unity 6 API facts — settled by the compiler on 6000.0.84f1, do not re-guess

Accepted (compile): `UnityEngine.Time.frameCount`, `UnityEngine.Debug.Log`, `Camera.backgroundColor`, `nearClipPlane`/`farClipPlane`.
Rejected (compile errors): `UnityEngine.Time.realTimeSinceStartup` (capital T — you may try the exact lowercase spellings `realtimeSinceStartup` and `realtimeDeltaTIme` once, the compiler decides), `UnityEngine.Time.memoryUsage`, all `GetGCEditable*`/`GetGCTotalMemory`/`GetGCPause`/`GetGCTotalStack`, `UnityEngine.Stdout`, `UnityEngine.Console`, `SystemInfo` memory members.

If no scriptable real-time clock exists, fall back to OS-side wall-clock: emit `Debug.Log` marker lines carrying `frameCount` at fixed intervals and derive frame time from `logcat` timestamps; record which method produced the numbers in the result row.

## Code layout (current, on develop)

- `Assets/Runtime/Bridge/Unity/GameLoop.cs` — the loop; logs "Starting Game Loop"; already excluded from the headless player build.
- `FrameDriver.cs`, `InputAdapter.cs`, `LevelLoader.cs`, `PlatformBridge.cs` — shared headless; `PlatformBridge.frameWallDelta()` returns a constant, `probeDeviceTier()` returns 0. Do not add UnityEngine references to these four or the headless build breaks.
- `RenderLayer.cs`, `UIRoot.cs` — Unity-only, excluded from headless.
- Hook the measurement surface from `GameLoop.cs` or `RenderLayer.cs` only (both already excluded). Any NEW file under `Assets/Runtime/Bridge/Unity` must be added to the `Exclude=` list in `player.csproj` (line 37) or the headless player build breaks.
- Build recipe: `tools/player-package/build.sh` (verified `-executeMethod BuildPlayerPackage.BuildAll` form; success only when `Build/EventHorizon-Android.apk` exists). Unity editor: `/home/dfcastro/Unity/Hub/Editor/6000.0.84f1/Editor/Unity`; CLI `unity build --target Android --execute-method BuildPlayerPackage.BuildAll` prints script errors.
- API probes must live under `Assets/Runtime/Bridge/Unity` (`Assets/Editor` is not compiled by the player build) and carry a validation line that must appear in the error list for the evidence to count. Delete probe files before finishing.

## Device facts (S24 FE — the A-014 "modern phone" reference class)

Serial `RZCXB2BSF8P`, model `SM-S721B`, Android 16 / API 36, `ro.build.id BP4A.251205.006`, `MemTotal: 7397720 kB`. Package `com.DefaultCompany.eventhorizongame`; launch via `am start-activity com.DefaultCompany.eventhorizongame/com.unity3d.player.UnityPlayerGameActivity`; fresh installs need `cmd package enable <pkg>`; `adb install` plain (not `-r`). The display enters DOZE quickly — poll `dumpsys display` for `Display State=ON`. `adb devices` is currently EMPTY: the device run stops at the reconnect gate; implement and build, and state honestly that the device capture awaits reconnection. `dumpsys gfxinfo` reports 0 frames for this player — P-01 is NOT measurable through SurfaceFlinger.

## Scope boundaries

No design decisions; no changes to SimCore, harness, replays, golden files, or the §5.3 rule; no new dev knobs; do not touch A-025. The palette, camera, and scene binding are merged and must not regress.

## Acceptance criteria

1. Headless player build stays green: `player.csproj` 0/0 after adding any new file to `Exclude=`.
2. The Unity package builds via the recipe with `editorErrors: []`.
3. `RenderLayer.draw()` allocates nothing per frame (reused temporaries; the counter proves it).
4. The surface emits parseable summary lines (p50/p95/p99.9, worst hitch, per-frame allocation count) through `Debug.Log`.
5. Post-merge regression stays green: fork guard silent (the `forkExpected` hash unchanged), H-01 `digest=6d25ff0add639448 steps=2828 droppedSteps=0 result=goal`, sim byte-identical to `replays/p1-level-01.json` (138,865 B), H-02 PASS, exclusions STEP-11a..11e PASS.
6. No P-01/P-05 row is claimed green without a device measurement.

## Output procedure

Write-first: implement the code, verify by running the builds, commit on the branch with prefix `code(player-package):` (docs with `docs(briefs):`), open nothing — the coordinator reviews and merges. Report the API names you settled, the clock method chosen, and the exact log-line format the device capture will read.
