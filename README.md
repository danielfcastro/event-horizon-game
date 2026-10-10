# Event Horizon

A mobile game (iOS, Android) where the player controls a growing black hole. It starts small, drifts across the screen, pulls nearby bodies into its gravitational field, absorbs them, and grows in mass, event horizon, and pull strength.

This repository holds the **design and coordination plan** for that game, and since the phase-1 prototype merged it also holds the **deterministic simulation code** that plan produced: the fixed-point core (`Assets/Runtime/Fixed/`), the simulation (`Assets/Runtime/SimCore/`), a command-line harness (`tools/harness/`), blessed golden fixtures (`replays/`), and a **non-Unity windowed client** (`tools/player/`, built by A-026) that draws the same simulation in an SDL2 window. Since A-023 merged it also holds a **Unity player package**: the project (`Assets/EventHorizon.unity`, `ProjectSettings/`), the render layer, the build recipe (`tools/player-package/`), and a recipe that produces a real app package (`Build/EventHorizon-Android.apk`, `Build/EventHorizon-iOS.app` — build outputs, gitignored like the harness outputs). What it does **not** hold is a store submission: A-025 is gated on store consoles and on the named device classes. Everything is written so that AI agents can pick up work in a predictable order without duplicating effort or making incompatible decisions.

## What is in this repository

| File | Role |
| --- | --- |
| `PLAN.md` | Canonical source of truth: game concept, agent roles, artifact order, workflow rules, git-flow requirement. |
| `PROGRESS.md` | Run-state control file. Records the current artifact, what is done, what is next, and a dated log so an interrupted run can resume. |
| `README.md` | This overview. |
| `docs/` | Design and specification artifacts (`spec.md`, `architecture.md`, `design.md`, …), produced in the order defined in `PLAN.md`. |
| `docs/briefs/` | Per-artifact context briefs. A role agent reads only its brief, never `PLAN.md` or an approved upstream document in full. |
| `docs/policy-page/` | The filled privacy policy page source, at the current policy text version. |
| `docs/policy-hosting.md` | A-024: the published policy page, the single `POLICY_URL`, and the live-page verification. Every consumer reads the value recorded there. |
| `Assets/Runtime/Fixed/` | Fixed-point arithmetic and the PRNG. Part of the hashed fork set. |
| `Assets/Runtime/SimCore/` | The deterministic simulation: level loader, fixed-step driver, spawn director, snapshot codec. Part of the hashed fork set. |
| `Assets/Runtime/Bridge/Unity/` | Unity-facing files. Three of them (`GameLoop.cs`, `RenderLayer.cs`, `UIRoot.cs`) are excluded from the headless builds by `harness.csproj` and `player.csproj` and only compile inside the Unity editor; the other four (`FrameDriver.cs`, `InputAdapter.cs`, `LevelLoader.cs`, `PlatformBridge.cs`) are compiled headless and are shared by both surfaces. |
| `Assets/EventHorizon.unity` | The one scene the player package builds, registered in `ProjectSettings/EditorBuildSettings.asset`. A build with an empty scene list is a vacuous success: it exits 0 and writes nothing. |
| `Assets/Editor/BuildPlayerPackage.cs` | The scripted build entry (`BuildPlayerPackage.BuildAll`) for both shipping targets. Editor-side only; never compiled headless. |
| `ProjectSettings/` | The Unity project's own settings, committed because the repo root **is** the Unity project root. `submitAnalytics: 0`, `enableCrashReportAPI: 0`, `cloudEnabled: 0` — the shipped player's only collector is the first-party endpoint A-016 owns, so nothing emits outside A-017 §2's inventory. |
| `tools/player-package/` | A-023's recipes: `build.sh` runs the verified editor invocation (`-executeMethod`, not `-executeCommand`, which the editor ignores and exits 0 — a false green) and reports success only when a package exists on disk; `exclusions.sh` runs A-019 §12's ship-build exclusion checks (STEP-11a..11e) against the built package. |
| `tools/harness/` | The command-line harness: the H-01..H-09 command surface, golden registry, snapshot codecs. Not hashed by the fork guard. |
| `tools/player/` | The non-Unity windowed client: `SimHost.cs` runs the same simulation over `FrameDriver.frameLive` and writes a frame payload; `view.c` is a C SDL2 renderer that READS that payload and never writes to `SimState`. |
| `player.csproj` | Build manifest for the windowed client's C# side. The C side is built with `gcc` and SDL2. |
| `replays/` | Blessed golden fixtures: the committed run digest and the golden replay. These are the oracle for regression checks. |
| `harness.csproj` | Headless build manifest at the repo root, with root-relative source globs and the three Unity files excluded. |

`PLAN.md` wins on any disagreement. `PROGRESS.md` is run state only and carries no design decisions.

## Artifact sequence

Work proceeds through 26 artifacts, `A-001` through `A-026`, in the dependency order given in `PLAN.md`. Each artifact is one file or module, owned by one agent role, and moves through four statuses:

```text
proposed -> ready -> doing -> done
```

An artifact must not be started before its dependencies are `done`. The current status of every artifact is in the sequence table in `PLAN.md`; the current position of the run is in `PROGRESS.md`.

## Agent roles

`coordinator`, `planner`, `architect`, `designer`, `balance`, `level-designer`, `programmer`, `qa`, `release`. The coordinator owns repo workflow, dependency order, pull requests, merge decisions, and status updates; the others produce their assigned artifacts. Full responsibilities are defined in `PLAN.md` and `AGENTS.md`.

## Build and run the prototype

There are two runnable surfaces, and they share one simulation. The **harness** is headless: it runs the deterministic simulation and prints results, which is what makes the golden fixtures a regression oracle. The **windowed client** (`tools/player/`) draws the same `SimState` in an SDL2 window and takes keyboard thrust; it adds a renderer, never a second simulation, and `DT` stays `71582788L` in both. You need a .NET 8 toolchain (`dotnet`) for either; the harness build is the plain `harness.csproj` at the repo root, the client's C# side is `player.csproj`, and its C renderer needs SDL2.

### Getting `dotnet` on PATH (EndeavourOS / Arch)

The commands below use `$D` for the toolchain binary; with the two exports below you can call them as plain `dotnet` instead. Verified on EndeavourOS (Arch, `pacman`), from a fresh shell:

```sh
# A .NET 8 install whose layout is dotnet + sdk + packs + shared + host.
export DOTNET_ROOT="$HOME/.local/dotnet"
export PATH="$PATH:$HOME/.local/dotnet"

dotnet --version                                  # 8.0.425
dotnet build harness.csproj -o obj                # Build succeeded.
dotnet exec obj/harness.dll sim --seed 0x1F4A \
    --digest @replays/p1-level-01.digest.bin --level p1-level-01 --out /tmp/run.json
```

Two ways to get that toolchain, and the honest state of each:

- **Verified working here — the .NET 8 SDK tarball** (RID `linux-x64`, version 8.0.425). Get it from <https://dotnet.microsoft.com/en-us/download/dotnet/8.0>, the `sdk-8.0.425-linux-x64-binaries` link. The download was **not** run in this session: the page resolves the file with JavaScript, so no stable `curl` URL could be verified. The extracted layout is the one this repo's goldens were produced with, and the `--version` / `build` / `exec` sequence above was run verbatim.
- **Native Arch route — not verified here.** `extra` ships `dotnet-sdk-8.0` (`8.0.31.sdk131-1`), `dotnet-runtime-8.0`, `dotnet-host`, `dotnet-source-built-artifacts-8.0`, and `dotnet-targeting-pack-8.0`. Installing them needs `sudo`, which this session cannot supply. Do **not** hand-assemble that set: an extracted equivalent of the first four fails at `build` with `NU1101: Unable to find package Microsoft.NETCore.App.Host.arch-x64`, because that RID-specific host pack is in none of them — let `pacman` resolve the set. What *is* verified about that route: Arch's runtime (driver 8.0.131, RID `arch-x64`) **runs** the harness byte-identically — H-01 `digest=6d25ff0add639448 steps=2828 exit=0`, H-02 `PASS snapshotsCompared=6 exit=0`.

Neither headless surface needs Unity, and neither is affected by it: the harness and the windowed client build and run without any editor. A-023's player package does need Unity, and that toolchain is present and verified in this environment — Unity 6 LTS `6000.0.84f1` with iOS and Android build support, and an active Unity Personal license. What Unity still cannot do here is run on a phone: P-01/P-02/P-04/P-05 name real device classes, and no build on a desktop can stand in for them.

```sh
cd <repo>
D=<path-to>/dotnet        # e.g. /home/you/.local/dotnet/dotnet

# Build. Must print "Build succeeded." and nothing else: any error or
# warning line means the tree regressed.
$D build harness.csproj -o obj

# H-01: run level p1-level-01 and check it against the committed digest.
$D exec obj/harness.dll sim --seed 0x1F4A \
    --digest @replays/p1-level-01.digest.bin --level p1-level-01 --out /tmp/run1.json
# expect: digest=6d25ff0add639448  score=0  steps=2828  droppedSteps=0  result=goal

# H-02: replay the golden and assert the steps match.
$D exec obj/harness.dll replay @replays/p1-level-01.json --assert-steps 600
# expect: PASS snapshotsCompared=6

# Determinism: two runs must be byte-identical.
$D exec obj/harness.dll sim --seed 0x1F4A \
    --digest @replays/p1-level-01.digest.bin --level p1-level-01 --out /tmp/run2.json
cmp /tmp/run1.json /tmp/run2.json && echo IDENTICAL

# H-03: snapshot at a step. H-05: counters (timing histogram, highmarks, rssKB).
$D exec obj/harness.dll snap --seed 0x1F4A --digest @replays/p1-level-01.digest.bin \
    --level p1-level-01 --step 600
$D exec obj/harness.dll counters --seed 0x1F4A --digest @replays/p1-level-01.digest.bin \
    --level p1-level-01
```

Before any command runs, every invocation checks two guards silently: the fixed-point self-tests (`MulDiv`, `ISqrtQ`) and the **FIXED-FORK** hash over the enumerated `Assets/Runtime/Fixed/` and `Assets/Runtime/SimCore/` sources against a committed expected value. Silence means both passed. If a hashed file is missing or the digest moved, no command runs — that guard is what makes the golden fixtures a real regression oracle rather than a suggestion.

Exit codes are the contract in `PLAN.md` / A-019 §9: **0 pass, 1 fail, 2 harness error**. `hitch`, `fps`, `pointer`, and `tier` (H-06..H-09) are implemented, not stubs: A-022 merged the frame driver they test, and each one needs its scenario argument (`--at`/`--ms`, `--target`, `--script`, `--probe`) — called without it the command reports that it has no observable effect and exits 2, never a fake success.

Three gotchas that will bite if you improvise:

- Use `dotnet exec obj/harness.dll`. `dotnet run` masks the app's exit code when piped, and the native launcher `./obj/harness` fails with exit 131 on this toolchain.
- Filter build output with `error|warning|Build`, not `error CS` — the narrower filter hides project-level failures *and warnings*, which is how a warning once hid itself for a whole artifact.
- `rm -rf obj` before re-probing. A stale DLL in `obj` produced a false diff once.

A malformed argument exits **2** with the reason it rejected, for example `--seed 0xZZZ` prints `harness: parseHexU64: bad digit in 0xZZZ (command did not complete)` and exits 2. A simulation failure is distinct: it exits **1**.

### Phase 2: frame accounting (H-06..H-09)

Each command below was run in this session after the A-024 merge; the `expect` lines are what it printed. None of them changes `DT`.

```sh
# H-06: force a 100 ms hitch at two frames and check the accounting absorbs it.
$D exec obj/harness.dll hitch --at 1200,1800 --ms 100 \
    --seed 0x1F4A --digest @replays/p1-level-01.digest.bin --level p1-level-01
# expect: framesHitched=2818 droppedStepsHitched=2 maxStepsPerFrame=4 ... PASS result=goal

# H-07: run at a 30 fps schedule and prove the trajectory still reaches the goal.
$D exec obj/harness.dll fps --target 30 --seed 0x1F4A \
    --digest @replays/p1-level-01.digest.bin --level p1-level-01
# expect: stepsPerFrame=2 (expected 2) framesToGoal=1414 droppedSteps=0 PASS result=goal hardwareOnly=true
# note: --time-scale 2.0 is rejected with a named reason at exit 2; the schedule is not a speed dial.

# H-08: replay a scripted input fixture (replays/h-08-pointer.txt is committed).
$D exec obj/harness.dll pointer --script @replays/h-08-pointer.txt --seed 0x1F4A --level p1-level-01
# expect: scriptDirectives=10 scriptedSteps=7 dupSteps=2 pauseSteps=1 ... PASS snapshotsCompared=17 result=timeout

# H-09: probe the render tiers and prove the simulation is identical across them.
$D exec obj/harness.dll tier --probe --seed 0x1F4A \
    --digest @replays/p1-level-01.digest.bin --level p1-level-01
# expect: tiersProbed=3 goalSteps=2828,2828,2828 renderPixelsPerUnit distinct PASS
```

### Run the windowed client (no Unity)

```sh
# C# side. Must print "Build succeeded." with 0 warnings.
$D build player.csproj -o obj-player

# C renderer. The bare `-lSDL2` form does NOT link (undefined sqrt/sin/cos);
# these are the flags that were probed to work.
gcc tools/player/view.c -o obj-player/view \
    -I/usr/include/SDL2 -D_GNU_SOURCE=1 -D_REENTRANT -lm -lSDL2

# Replay mode reproduces H-01 through the same frame driver.
$D exec obj-player/player.dll --replay --level p1-level-01 --seed 0x1F4A \
    --digest @replays/p1-level-01.digest.bin
# expect: mode=replay frames=2828 steps=2828 goalStep=2828 goalReached=True droppedSteps=0 result=goal

# The headless proof that the renderer actually draws: dump ONE frame's payload
# at a step, then render that payload to a PPM. No window, no display needed.
$D exec obj-player/player.dll --replay --level p1-level-01 --seed 0x1F4A \
    --digest @replays/p1-level-01.digest.bin --dump-step 1200 --dump-file /tmp/f-1200.bin
obj-player/view --frame /tmp/f-1200.bin --ppm /tmp/f-1200.ppm --once
# expect: view: ppm=/tmp/f-1200.ppm size=960x540 step=1200 phase=1 boundsWU=128x96
#         eventRadiusPx=218 ppuUsed=5 bodies=15 nonbg=152359
```

The `phase` and `ppuUsed` fields are the §5.3 hybrid camera reporting what it
actually did, not what it intends: while the growing horizon fits, the camera is
pinned to the world (`phase=1`, `ppuUsed=5` fixed), so the disc visibly expands —
314 px at step 1, 436 px at step 1200. Once it would clip, the camera hands over
to a smooth zoom-out (`phase=2`, `ppuUsed` 4 then 3) and the horizon is held at
484 px, inside the frame. `boundsWU` is read from the payload, so the same binary
is correct for a level with different bounds and a window with different size.

The window itself needs a desktop (`DISPLAY` set, SDL2 installed). What a headless
environment can prove is more than a clean exit, and the proof above is why: a
dumped payload rendered to a PPM is a real picture, and it can be converted and
looked at. That path found a defect that arithmetic could not — the renderer drew
the collision core, so the hole was the same 32 px at every step while its mass
went 100 -> 567 — and it is the path that confirms the two-phase camera now. What
stays unprovable here is the **live** window on a desktop: no headless run can
observe the screen, so that one claim is recorded as not-executable until a human
looks at it.

### Build the Unity player package

Both recipes are in the repo and both were run in this session; the outputs under
`Build/` are gitignored, like the harness outputs.

```sh
# The recipe. It removes any stale package first, so a failed build cannot look
# green because an old APK is still on disk.
sh tools/player-package/build.sh
# expect: build: Build/EventHorizon-Android.apk and Build/EventHorizon-iOS.app written;
#         exclusions.sh can now run against them
# with no package it prints NOT EXECUTABLE and exits 2 — never reported green.

# A-019 §12's ship-build exclusion checks, run against the built package.
sh tools/player-package/exclusions.sh
# expect: STEP-11a PASS: no harness/golden paths
#         STEP-11b PASS: no harness entrypoint/flags
#         STEP-11c PASS: no replays/ or golden digest
#         STEP-11d PASS (static): no dev.* key in the package; device no-softening
#             re-run NOT EXECUTABLE (hardware)
#         STEP-11e PASS: no debug power state

# The equivalent Unity CLI form, which also reports the compiler's verdict:
unity build --target Android --execute-method BuildPlayerPackage.BuildAll --json
# expect: "success": true, "editorErrors": []
```

Two things the toolchain forced, and why they are not papered over:

- **The editor flag is `-executeMethod`.** `-executeCommand` is not a Unity flag:
  the editor ignores an unknown flag, exits 0, and writes nothing. A recipe that
  trusts the exit code alone reports a false green, so `build.sh` checks the
  package exists on disk before it says success.
- **Unity 6's C# API is not what the draft assumed.** The names in the merged
  package are the ones the compiler on `6000.0.84f1` accepts —
  `UnityEditor.BuildPipeline.BuildPlayer(EditorBuildSettingsScene[], string,
  BuildTarget, UnityEditor.BuildOptions)`, `BuildTarget.iOS`, and
  `UnityEngine.Screen.currentResolution` (a `Resolution` with `width`/`height`).
  `UnityEditor.BuildPlayer`, `BuildTarget.Standalone_ARM64`, and
  `Screen.GetResolution()` are all rejected. The rejected names are recorded in
  `PLAN.md` §4 so a later session does not re-guess them.
- **A-014's RAM-class thresholds are not scriptable on Unity 6.** Every
  `SystemInfo` memory member candidate is rejected by the compiler, so the player
  does not invent a CPU-count rule: the device tier stays unknown (0), and the
  RAM-based class detection remains a platform concern recorded for A-025.

The player package is **not** linked to Unity's cloud services. A-020 §8 chooses a
first-party-only crash reporter posting to A-016's own endpoint, and A-017 §2
states that its 8-event table is the complete inventory, so enabling Unity's
analytics or cloud project would add a collector outside it. That is why the
project settings carry `submitAnalytics: 0`, `enableCrashReportAPI: 0`,
`cloudEnabled: 0`.

## How to contribute

All contributors, human or AI, must use git-flow:

- `main` is protected. It only receives finished work merged from `release/*` and `hotfix/*`. Never commit or push directly to `main`.
- `develop` is the integration branch. All development work lives there.
- Every artifact is developed on a feature branch created from `develop`, named `feature/<agent>-<artifact-id>`.
- Approved feature branches are merged into `develop` and deleted after merge.

```sh
git checkout -b feature/coordinator-readme develop
# ... draft the artifact, update PLAN.md status ...
git commit -m "docs(readme): ..."
git push -u origin feature/coordinator-readme
```

Then open a pull request using the template in `PLAN.md` and wait for coordinator review. Merge only when approved.

## Pushing to a private remote

The repository is intended to live on a private remote. Create one and point `origin` at it:

```sh
# Create a private repository with the GitHub CLI
gh repo create event-horizon-game --private --clone

# Or attach an existing private repository
git remote add origin git@github.com:<owner>/event-horizon-game.git
git push -u origin HEAD
```

Keep the remote private for the whole run: the plan, balance values, and monetization design are unpublished design decisions. If a public mirror is ever needed, push only finished `release/*` output to it, never the working branches.

## Next steps

1. Read `PLAN.md`, then `PROGRESS.md`, and resume from the recorded state.
2. Take the first artifact whose status is `ready`.
3. Create its feature branch, draft the artifact, update its status in `PLAN.md`, and open a pull request.
4. Keep `PROGRESS.md` updated after every meaningful step, including partial ones.
