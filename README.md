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

Work proceeds through 21 artifacts, `A-001` through `A-021`, in the dependency order given in `PLAN.md`. Each artifact is one file or module, owned by one agent role, and moves through four statuses:

```text
proposed -> ready -> doing -> done
```

An artifact must not be started before its dependencies are `done`. The current status of every artifact is in the sequence table in `PLAN.md`; the current position of the run is in `PROGRESS.md`.

## Agent roles

`coordinator`, `planner`, `architect`, `designer`, `balance`, `level-designer`, `programmer`, `qa`, `release`. The coordinator owns repo workflow, dependency order, pull requests, merge decisions, and status updates; the others produce their assigned artifacts. Full responsibilities are defined in `PLAN.md` and `AGENTS.md`.

## Build and run the prototype

The prototype is **headless**: it runs the deterministic simulation and prints results. There is no window, no player input, and no rendering — the frame driver and render layer are phase 2. You need a .NET 8 toolchain (`dotnet`); the build is the plain `harness.csproj` at the repo root.

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

Known deviation from the exit-code contract: a malformed hex seed (`--seed 0xZZZ`) aborts with exit 134 instead of exiting 2, because argument parsing throws `SystemicFailure` and `Main` does not catch it.

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
