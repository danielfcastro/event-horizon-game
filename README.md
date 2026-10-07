# Event Horizon

A mobile game (iOS, Android) where the player controls a growing black hole. It starts small, drifts across the screen, pulls nearby bodies into its gravitational field, absorbs them, and grows in mass, event horizon, and pull strength.

This repository holds the **design and coordination plan** for that game, and since the phase-1 prototype merged it also holds the **headless simulation code** that plan produced: the deterministic core (`Assets/Runtime/SimCore/`, `Assets/Runtime/Fixed/`), a command-line harness (`tools/harness/`), and blessed golden fixtures (`replays/`). What it does **not** hold is a player-facing build: there is no app package, no render layer running, and no store submission. Everything is written so that AI agents can pick up work in a predictable order without duplicating effort or making incompatible decisions.

## What is in this repository

| File | Role |
| --- | --- |
| `PLAN.md` | Canonical source of truth: game concept, agent roles, artifact order, workflow rules, git-flow requirement. |
| `PROGRESS.md` | Run-state control file. Records the current artifact, what is done, what is next, and a dated log so an interrupted run can resume. |
| `README.md` | This overview. |
| `docs/` | Design and specification artifacts (`spec.md`, `architecture.md`, `design.md`, …), produced in the order defined in `PLAN.md`. |
| `docs/briefs/` | Per-artifact context briefs. A role agent reads only its brief, never `PLAN.md` or an approved upstream document in full. |
| `docs/policy-page/` | The filled privacy policy page source, at the current policy text version. |
| `Assets/Runtime/Fixed/` | Fixed-point arithmetic and the PRNG. Part of the hashed fork set. |
| `Assets/Runtime/SimCore/` | The deterministic simulation: level loader, fixed-step driver, spawn director, snapshot codec. Part of the hashed fork set. |
| `Assets/Runtime/Bridge/Unity/` | Contract-only Unity-facing files. Three of them (`GameLoop.cs`, `RenderLayer.cs`, `UIRoot.cs`) are excluded from the headless build by `harness.csproj` and do not compile without the Unity toolchain; the other three (`InputAdapter.cs`, `LevelLoader.cs`, `PlatformBridge.cs`) are compiled headless. |
| `tools/harness/` | The command-line harness: the H-01..H-09 command surface, golden registry, snapshot codecs. Not hashed by the fork guard. |
| `replays/` | Blessed golden fixtures: the committed run digest and the golden replay. These are the oracle for regression checks. |
| `harness.csproj` | Headless build manifest at the repo root, with root-relative source globs and the three Unity files excluded. |

`PLAN.md` wins on any disagreement. `PROGRESS.md` is run state only and carries no design decisions.

## Artifact sequence

Work proceeds through 25 artifacts, `A-001` through `A-025`, in the dependency order given in `PLAN.md`. Each artifact is one file or module, owned by one agent role, and moves through four statuses:

```text
proposed -> ready -> doing -> done
```

An artifact must not be started before its dependencies are `done`. The current status of every artifact is in the sequence table in `PLAN.md`; the current position of the run is in `PROGRESS.md`.

## Agent roles

`coordinator`, `planner`, `architect`, `designer`, `balance`, `level-designer`, `programmer`, `qa`, `release`. The coordinator owns repo workflow, dependency order, pull requests, merge decisions, and status updates; the others produce their assigned artifacts. Full responsibilities are defined in `PLAN.md` and `AGENTS.md`.

## Build and run the prototype

The prototype is **headless**: it runs the deterministic simulation and prints results. There is no window, no player input, and no rendering — the frame driver and render layer are phase 2. You need a .NET 8 toolchain (`dotnet`); the build is the plain `harness.csproj` at the repo root.

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

Unity is not needed for anything in this repository and is not installable in this environment: the headless prototype builds and runs without it. The artifact that needs the Unity toolchain is A-023, the player package, which carries that gate.

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

Exit codes are the contract in `PLAN.md` / A-019 §9: **0 pass, 1 fail, 2 harness error**. `hitch`, `fps`, `pointer`, and `tier` (H-06..H-09) are phase-2 stubs: they print why they cannot run and exit **2**, never a fake success.

Three gotchas that will bite if you improvise:

- Use `dotnet exec obj/harness.dll`. `dotnet run` masks the app's exit code when piped, and the native launcher `./obj/harness` fails with exit 131 on this toolchain.
- Filter build output with `error|warning|Build`, not `error CS` — the narrower filter hides project-level failures *and warnings*, which is how a warning once hid itself for a whole artifact.
- `rm -rf obj` before re-probing. A stale DLL in `obj` produced a false diff once.

A malformed argument exits **2** with the reason it rejected, for example `--seed 0xZZZ` prints `harness: parseHexU64: bad digit in 0xZZZ (command did not complete)` and exits 2. A simulation failure is distinct: it exits **1**.

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
