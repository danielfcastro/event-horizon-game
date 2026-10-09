---
status: draft
next_artifact: none executable (A-024 done and merged via PR #32 at 866fa08; A-023 waits on the Unity toolchain and on real device classes, A-025 waits on store consoles and on A-023; contact verification needs a real inbox and a human reply. The run is at a gate.)
owner: coordinator
project: Event Horizon
platform: iOS, Android
---

# Event Horizon Game Plan

## 0. Purpose

This plan defines a mobile game where the player controls a growing black hole. The black hole starts small, moves across the screen, attracts bodies with its gravitational field, absorbs them, and grows in mass, event horizon, and gravitational pull.

The repository is intended to be coordinated by AI agents. Each artifact is developed in a specific order so agents do not duplicate work or make incompatible design decisions.

## 1. Agent coordination protocol

### 1.1 Agent roles

| Agent | Main responsibilities |
| --- | --- |
| coordinator | Owns repo workflow, dependency order, PRs, status updates, merge decisions. |
| planner | Writes specifications, design plans, and game systems. |
| architect | Defines technical architecture, data structures, modules, and platform plan. |
| designer | Defines game feel, visuals, UI, camera, feedback, and accessibility. |
| balance | Defines progression curves, economy, difficulty, upgrade values. |
| level-designer | Defines objects, hazards, levels, worlds, and content. |
| programmer | Creates code scaffolding, prototypes, and implementation notes. |
| qa | Defines tests, QA checklist, performance checks, and regression risks. |
| release | Handles store compliance, privacy, analytics, monetization, submission. |

### 1.2 Artifact workflow

Every artifact must follow this process:

1. Read PLAN.md.
2. Select the first artifact whose status is ready.
3. Create a git-flow feature branch from develop named:

   ```text
   feature/<agent>-<artifact-id>
   ```

4. Draft the artifact.
5. Update the artifact status in PLAN.md:

   ```text
   proposed -> ready -> doing -> done
   ```

   Artifacts whose dependencies are not yet done are marked blocked in the sequence table until they become ready.

6. Open a pull request.
7. Wait for coordinator review.
8. Merge to main only when approved.

### 1.3 Branch naming

Use:

```text
feature/coordinator-plan
feature/coordinator-readme
feature/coordinator-agents
feature/planner-spec
feature/coordinator-briefs
feature/architect-architecture
feature/designer-design
feature/balance-balance
feature/level-designer-content
feature/level-designer-levels
feature/designer-ui
feature/designer-input
feature/designer-accessibility
feature/qa-test-plan
feature/qa-qa
feature/release-store
feature/release-monetization
feature/release-privacy-policy
feature/planner-agent-rules
feature/architect-prototype-scaffold
feature/release-ship
feature/programmer-prototype
feature/programmer-harness-warning
feature/programmer-harness-exitcode
feature/programmer-frame-driver
feature/programmer-player-package
feature/programmer-player-sdl
feature/release-policy-hosting
feature/release-submission
release/0.20
```

### 1.4 Commit prefixes

Use:

```text
docs(plan): ...
docs(agents): ...
docs(spec): ...
docs(architecture): ...
docs(design): ...
docs(balance): ...
docs(content): ...
docs(levels): ...
docs(ui): ...
docs(input): ...
docs(accessibility): ...
docs(test-plan): ...
docs(qa): ...
docs(store): ...
docs(monetization): ...
docs(privacy-policy): ...
docs(agent-rules): ...
docs(prototype-scaffold): ...
docs(ship): ...
docs(prototype): ...
code(prototype): ...
docs(briefs): ...
code(player): ...
chore(repo): ...
```

### 1.5 Pull request template

Each PR should include:

```markdown
## Artifact ID
A-xxx

## Artifact
file or module name

## Agent
agent role

## Depends on
- A-xxx
- A-xxx

## Summary
Short summary of what was produced.

## Open questions
- ...

## Acceptance criteria met
- [ ] ...

## Verification
- [ ] Artifact follows PLAN.md.
- [ ] Dependencies are complete.
- [ ] No conflicting design decisions.
```

### 1.6 Rules

- Any agent or person contributing to development MUST use Git-flow.
- Do not start an artifact if its dependencies are not done.
- Do not push directly to main unless the coordinator approves it.
- Do not change an artifact without updating PLAN.md.
- If a design decision conflicts with PLAN.md, update PLAN.md first.
- Keep one canonical source of truth: PLAN.md.
- The coordinator MUST keep PROGRESS.md updated after every meaningful step so an interrupted run can resume.
- Agent definitions in section 9.2 MUST name the same artifacts, dependencies, and consumers as the sequence table in section 2 and the dependency graph in section 3.
- Code must follow approved architecture and balance documents.
- Monetization must not block core game completion.

### 1.7 Git-flow requirement

Any agent or person contributing to development MUST follow Git-flow:

- `main` is protected. It only receives finished work merged from `release/*` and `hotfix/*` branches. Never commit or push directly to `main`.
- `develop` is the integration branch. All development work lives there.
- Every artifact is developed on a feature branch created from `develop`.
- Approved feature branches are merged into `develop` and deleted after merge.
- Release preparation uses `release/<version>` branches merged into `main` and back into `develop`.
- Urgent fixes against `main` use `hotfix/<version>` branches merged back into `develop`.

Preferred commands:

```text
git flow init
git flow feature start <name>
git flow feature finish <name>
git flow release start <version>
git flow release finish <version>
git flow hotfix start <version>
git flow hotfix finish <version>
```

If the git-flow command line tool is unavailable, use equivalent manual git-flow branch and merge operations with the same branch prefixes and merge targets.

### 1.8 Progress control file

- `PROGRESS.md` at the repo root is the run-state control file for the coordinator.
- It records: current artifact, last completed artifact, next ready artifact, branch, PR, status (idle, running, interrupted, blocked), and a dated log.
- The coordinator reads PLAN.md then PROGRESS.md at the start of every session and resumes exactly from the recorded state.
- The coordinator updates PROGRESS.md after every meaningful step, including partial ones, and records the exact stopping point and reason when interrupted or blocked.
- PROGRESS.md is run state only; design decisions belong in PLAN.md.

## 2. Artifact sequence

The sequence below is ordered so that AI agents can coordinate work safely.

| ID | Artifact | Agent | Depends on | Purpose | Status | Acceptance criteria |
| --- | --- | --- | --- | --- | --- | --- |
| A-001 | PLAN.md | coordinator | none | Master plan and coordination protocol | done | Contains game concept, artifact order, agent rules, git-flow requirement. |
| A-002 | README.md | coordinator | A-001 | Repo overview | done | Explains repo and next steps. |
| A-003 | AGENTS.md | coordinator | A-002 | Agent roles and workflow | done | Defines agent responsibilities and branch rules. |
| A-004 | docs/spec.md | planner | A-001 | High-level game specification | done | Defines core loop, controls, goals, failure conditions. |
| A-005 | docs/architecture.md | architect | A-004 | Technical architecture | done | Defines modules, simulation loop, data structures, platform plan. |
| A-006 | docs/design.md | designer | A-004 | Game feel, visuals, UI, camera | done | Defines visual language, feedback, accessibility. |
| A-007 | docs/balance.md | balance | A-006 | Progression, economy, difficulty | done | Defines mass growth, upgrade values, target masses. |
| A-008 | docs/content.md | level-designer | A-006, A-007 | Object types, hazards, upgrades | done | Lists objects, hazards, rewards, modifiers. |
| A-009 | docs/levels.md | level-designer | A-008 | Level structure and campaign | done | Defines worlds, objectives, difficulty curve. |
| A-010 | docs/ui.md | designer | A-006 | HUD, menus, settings | done | Defines UI layout and mobile scaling. |
| A-011 | docs/input.md | designer | A-010 | Mobile controls | done | Defines drag, joystick, tilt, accessibility. |
| A-012 | docs/accessibility.md | designer | A-010 | Accessibility options | done | Defines contrast, reduced motion, larger controls. |
| A-013 | docs/test-plan.md | qa | A-005 | Testing strategy | done | Defines simulation tests, balance tests, UI tests. |
| A-014 | docs/qa.md | qa | A-013 | QA checklist | done | Defines manual QA and performance checklist. |
| A-015 | docs/store.md | release | A-014 | Store submission plan | done | Defines listing, screenshots, trailer, compliance. |
| A-016 | docs/monetization.md | release | A-015 | Monetization and economy | done | Defines ads, IAP, analytics, fairness. |
| A-017 | docs/privacy-policy.md | release | A-015 | Privacy policy | done | Defines data collected, contact info, store compliance. |
| A-018 | docs/agent-rules.md | planner | A-003 | Expanded agent rules | done | Detailed rules for AI agents. |
| A-019 | docs/prototype-scaffold.md | architect | A-005, A-013 | First code scaffold | done | Defines first playable prototype modules. |
| A-020 | docs/ship.md | release | A-015, A-016, A-017 | Shipping checklist | done | Defines final release steps. |
| A-021 | Assets/ + tools/harness/ (Phase 1 prototype code) | programmer | A-005, A-013, A-019 | Phase 1 prototype: one black hole, one level, move/attract/absorb/grow | done | Headless build compiles with dotnet; harness H-01 runs p1-level-01 to completion exercising move/attract/absorb/grow per A-007 formulas; H-02 golden replay byte-equal (determinism); module status table per A-019 §4; EHSNAP1/EIDIG1 formats per A-019 §7/§8; no-softening holds (no assists, stubs return identity). |
| A-022 | Phase 2 frame driver and render state (code) | programmer | A-005, A-019, A-021 | Frame driver, render state, and the weak-device and quality-tier knobs that make H-06..H-09 real instead of exit-2 stubs. | done | Headless build compiles with 0 errors and 0 warnings; the frame driver reaches the exactly-one-loop of A-019 §11 and never re-implements stepping; H-06..H-09 exit 0 and assert SimState byte-identical across quality tiers for the same (seed, inputDigest) per A-019 §10; DT stays the constant 1/60 so a quality or fps setting cannot change the simulation; FIXED-FORK digest stays 0xb6b01e1cff3f7710UL unless a hashed Fixed/ or SimCore/ file changed, in which case forkExpected is re-blessed and both goldens re-verified. |
| A-023 | Unity player package (ship build) | programmer | A-005, A-022 | The player-facing build for the device classes named in A-014: render layer, UI root, input adapter, platform bridge over the same SimCore. | proposed | Environment gate: requires the Unity toolchain, which is not present in this environment, so this artifact stays blocked until it is. Builds for the device classes named in A-014; runs the same SimCore with DT = 1/60 unchanged; the two ship-build exclusion steps of A-019 §7 run green; the version string follows A-020 §2, so 1.0.0 is used only once A-014's release gate and A-015's SG gates are green. |
| A-024 | docs/policy-hosting.md | release | A-017, A-020 | Publishes the policy page at the single POLICY_URL and records it (A-020 STEP-10, PR-2/PR-3). | done | Host gate closed 2026-10-09: the segment pre-named in A-020 §6 is squatted by an unrelated GitHub org (probed), so the canonical POLICY_URL is `https://danielfcastro.github.io/privacy-policy/`. Merged via PR #32 into develop at 866fa08. Evidence in docs/policy-hosting.md §5: GET 200 with zero redirects; served text equals the page source verbatim after documented host rendering (the default theme emits an h1 "privacy-policy" heading, recorded not removed); no placeholder tokens; live page carries policy text version 1.1 and the effective date. Records the host, the published POLICY_URL, and the policy text version of the page published; confirms one single URL reused in every listing field and the in-game privacy screen; the page body is the fenced block of A-017 §3 verbatim at that version. |
| A-025 | docs/submission.md and docs/ship-notes/<version>.md | release | A-020, A-023, A-024 | Executes A-020 §4.1 submission mechanics against the real consoles and files the ship notes. | proposed | Environment gate: requires store consoles and the A-023 ship build, neither present in this environment. Records per-store submission date and public listing URLs, the version string matching A-020 §2, and one line of gate evidence per row of A-020 §4; never reports a gate green without evidence. |
| A-026 | Non-Unity player package (SDL2 windowed client) | programmer | A-005, A-019, A-021, A-022 | A windowed, playable client that draws the black hole and the bodies it absorbs, so the repo produces something a human can see and steer without the Unity toolchain. | done | A window opens on the desktop and shows the hole and the bodies; keyboard thrust moves it through the same 16-way `InputDigest` angle table the replay channel uses; `holeMass` and `holeRadius` grow as bodies are absorbed; the run reaches the `p1-level-01` goal at the same step index as H-01 (2828). The renderer READS `SimState` and never writes to it, and the simulation is the existing `SimCore`/`FixedStepDriver` reached through `FrameDriver.frameLive`, not a reimplementation. A frame payload dumped from the client matches the harness snapshot at the same step index. The C renderer compiles with `gcc -lSDL2` and the C# side builds with 0 errors and 0 warnings; `DT` stays `71582788L`. |

## 3. Dependency graph

```text
A-001 PLAN.md
  |
  +--> A-002 README.md
  |       |
  |       +--> A-003 AGENTS.md
  |               |
  |               +--> A-018 agent-rules.md
  |
  +--> A-004 spec.md
          |
          +--> A-005 architecture.md
          |       |
          |       +--> A-013 test-plan.md
          |               |
          |               +--> A-014 qa.md
          |                       |
          |                       +--> A-015 store.md
          |                               |
          |                               +--> A-016 monetization.md
          |                               |
          |                               +--> A-017 privacy-policy.md
          |
          +--> A-006 design.md
                  |
                  +--> A-007 balance.md
                  |
                  +--> A-010 ui.md
                  |       |
                  |       +--> A-011 input.md
                  |       |
                  |       +--> A-012 accessibility.md
                  |
                  +--> A-008 content.md
                          |
                          +--> A-009 levels.md

A-019 prototype-scaffold.md depends on A-005 and A-013.
A-020 ship.md depends on A-015, A-016, A-017.
A-021 Phase 1 prototype code depends on A-005, A-013, A-019.

Post-plan artifacts (registered 2026-10-07 so the rows of A-020 §4 stop being unreachable):
A-022 phase 2 frame driver depends on A-005, A-019, A-021.
A-023 Unity player package depends on A-005, A-022.
A-024 policy-hosting.md depends on A-017, A-020.
A-025 submission.md and ship notes depend on A-020, A-023, A-024.
A-026 non-Unity player package depends on A-005, A-019, A-021, A-022.

A-023 and A-025 additionally carry an environment gate (Unity toolchain, store consoles):
they are not startable in an environment that lacks the tool, and they are never
reported as done without the evidence their acceptance criteria name.

A-026 exists precisely because that gate leaves the repo with nothing a human can
see: A-023 is the Unity player package and this environment has no Unity toolchain,
so before A-026 the only runnable artifact was a text harness. A-026 is not a
second simulation. It is a renderer over the SAME SimState, and it is the artifact
that makes the black hole of A-005 §5.1 visible and steerable without Unity.
```

## 4. Artifact contracts

### A-001 PLAN.md

**Purpose:**

- Master source of truth.
- Defines game concept.
- Defines agent workflow.
- Defines artifact order.

**Status:**

- done

### A-002 README.md

**Purpose:**

- Human-readable repo overview.
- Explains what the repo contains.
- Explains how to push to a private remote.

**Status:**

- done (extended 2026-10-07 on `feature/coordinator-readme`: a "Build and run the prototype" section was added, because after A-021 merged there was no documented way for a human to build or run anything. Two stale claims were corrected in the same pass: "20 artifacts, A-001 through A-020" -> 21 artifacts through A-021, and "not the game code" -> the repo now also carries the phase-1 headless prototype (`Assets/`, `tools/harness/`, `replays/`, `harness.csproj`). The run guide documents only commands verified in this session, the A-019 §9 exit-code contract (0 pass / 1 fail / 2 harness error), and the toolchain gotchas (`dotnet run` masks the app exit code when piped; the native launcher `./obj/harness` fails with exit 131; use `dotnet exec obj/harness.dll`).)
- extended again 2026-10-07 on the same branch: a "Getting `dotnet` on PATH (EndeavourOS / Arch)" subsection was added, because the user asked for toolchain steps on EndeavourOS and the repo documented none. The machine was confirmed EndeavourOS (`/etc/os-release` `ID=endeavouros`, `ID_LIKE=arch`, pacman present). Verified by running: `dotnet` on PATH from a fresh shell gives `dotnet --version` 8.0.425, `dotnet build harness.csproj -o obj` prints Build succeeded., and H-01/H-02 run byte-identical to the blessed golden. The artifact count claim was corrected again in this pass, 21 -> 25, because registering A-022..A-025 invalidated it. Two routes are documented with their honest state: the .NET 8 SDK tarball (RID `linux-x64`, 8.0.425) is the verified-working one whose download step is NOT executed because the official page resolves the file with JavaScript and no stable curl URL could be verified; the native Arch route (extra ships `dotnet-sdk-8.0` 8.0.31.sdk131-1 plus runtime/host/source-built-artifacts/targeting-pack) is NOT verified here because installing needs sudo and this session cannot supply a password, and a hand-assembled equivalent of those packages failed at build with NU1101 (no `Microsoft.NETCore.App.Host.arch-x64` pack), while Arch's runtime was verified to run the harness byte-identically.)

- extended a third time 2026-10-09 on `feature/coordinator-readme`: the "headless" claims were corrected, because A-022 (frame driver, H-06..H-09) and A-026 (SDL2 windowed client, `tools/player/`, `player.csproj`) are merged and those sentences are now false — line 5 still said "no render layer running" and line 42 still said "no window, no player input, and no rendering". The artifact count claim was corrected again, 25 -> 26, because A-026 is registered in PLAN.md §2. A "Run the windowed client" subsection documents the player build and replay commands, each one run in this session after the A-024 merge: `dotnet build player.csproj -o obj-player` prints `Build succeeded. 0 Warning(s) 0 Error(s)`, `gcc tools/player/view.c -o obj-player/view -I/usr/include/SDL2 -D_GNU_SOURCE=1 -D_REENTRANT -lm -lSDL2` links, and `dotnet exec obj-player/player.dll --replay --level p1-level-01 --seed 0x1F4A --digest @replays/p1-level-01.digest.bin` prints `mode=replay frames=2828 steps=2828 goalStep=2828 goalReached=True droppedSteps=0 result=goal exit=0`. The bare `gcc ... -lSDL2` form from the A-026 brief does NOT link (undefined sqrt/sin/cos) and is documented with the full flags instead. What stays true and is left alone: there is still no app package, no store submission, and no Unity player build — A-023 remains gated. Merged via PR #33 into develop at 7e952f4, branch deleted locally and remotely; post-merge sweep on merged develop green (harness 0/0, fork guard silent, H-01 digest=6d25ff0add639448 steps=2828, sim byte-identical to the golden, H-02 PASS, player goalStep=2828 droppedSteps=0) and the stale-phrase grep is empty.)

### A-003 AGENTS.md

**Purpose:**

- Defines AI agent roles.
- Defines branch naming.
- Defines PR workflow.
- Defines status updates.

**Status:**

- done

### A-004 docs/spec.md

**Purpose:**

- High-level game specification.
- Defines core loop, controls, objectives, failure conditions, and player experience.

**Status:**

- done

### A-005 docs/architecture.md

**Purpose:**

- Technical architecture.
- Defines simulation model, modules, data structures, rendering, performance, and platform choices.

**Status:**

- done

### A-006 docs/design.md

**Purpose:**

- Game feel and presentation.
- Defines camera, visuals, UI, particles, sound, feedback, and accessibility.

**Status:**

- done

### A-007 docs/balance.md

**Purpose:**

- Progression and difficulty.
- Defines mass growth, event horizon growth, pull strength, upgrade values, level targets, and economy.

**Status:**

- done

### A-008 docs/content.md

**Purpose:**

- Content design.
- Defines object types, hazards, upgrades, modifiers, rewards, and special black hole types.

**Status:**

- done

### A-009 docs/levels.md

**Purpose:**

- Level design.
- Defines campaign worlds, level goals, difficulty curve, endless mode, and daily challenges.

**Status:**

- done

### A-010 docs/ui.md

**Purpose:**

- UI design.
- Defines HUD, menus, settings, buttons, mobile scaling, and safe areas.

**Status:**

- done

### A-011 docs/input.md

**Purpose:**

- Input design.
- Defines drag control, joystick, tilt, touch handling, and accessibility.

**Status:**

- done

### A-012 docs/accessibility.md

**Purpose:**

- Accessibility design.
- Defines reduced motion, high contrast, larger buttons, colorblind-safe palette, and alternative controls.

**Status:**

- done

### A-013 docs/test-plan.md

**Purpose:**

- Testing plan.
- Defines simulation tests, balance tests, UI tests, performance tests, and regression risks.

**Status:**

- done

### A-014 docs/qa.md

**Purpose:**

- QA checklist.
- Defines manual QA, device testing, performance testing, and store readiness.

**Status:**

- done

### A-015 docs/store.md

**Purpose:**

- Store submission plan.
- Defines title, description, screenshots, trailer, age rating, localization, and compliance.

**Status:**

- done

### A-016 docs/monetization.md

**Purpose:**

- Monetization plan.
- Defines free-to-play structure, ads, IAP, analytics, and fairness rules.

**Status:**

- done

### A-017 docs/privacy-policy.md

**Purpose:**

- Privacy policy.
- Defines data collection, analytics disclosure, contact info, and store compliance.

**Status:**

- done (amended 2026-10-07 on `release/0.20`: policy text version bumped `1.0` -> `1.1`. Reason: filling the §3 third-party bracket left the sentence "anything extra the provider records is disclosed separately in the store forms and in the bracket above" referring to a bracket that no longer exists on the published page. §3 wording changed only where the reference was ship-time meta-language: "we name it here at release:" -> "we name it here:", and "in the bracket above" -> "in the sentence above". The three version assertions move together: §3 line 82, §6 line 226, §7 PR-1 line 297. The verbatim rule is unchanged: the published page is still A-017 §3 copied verbatim with only its two bracketed values filled.)

### A-018 docs/agent-rules.md

**Purpose:**

- Expanded agent rules.
- Defines how agents update status, create PRs, resolve conflicts, and avoid duplicate work.

**Status:**

- done

### A-019 docs/prototype-scaffold.md

**Purpose:**

- First code scaffold.
- Defines modules for black hole, bodies, gravity, absorption, camera, UI, and input.

**Status:**

- done

### A-020 docs/ship.md

**Purpose:**

- Shipping checklist.
- Defines final build, store submission, remote setup, analytics, ads/IAP, and launch steps.

**Status:**

- done

### A-021 Phase 1 prototype code

**Purpose:**

- First playable prototype code: PLAN 5.18 Phase 1 (one black hole, one level, move/attract/absorb/grow).
- Implements the module contract of A-019 docs/prototype-scaffold.md under the architecture of A-005 and the test plan of A-013.
- Consumed by every later code phase (game feel, balance, content, mobile polish).

**Status:**

- done (merged into develop via PR #24 at 8f52d25; commit 2770f53 code(prototype):)

### A-022 Phase 2 frame driver and render state

**Purpose:**

- Turn the phase-1 stubs H-06..H-09 into real commands: hitch detection, fps accounting, pointer mapping, quality-tier probe.
- Own the weak-device switch and the quality-tier probe of A-019 §10 without touching `DT`.
- Consumed by A-023, which renders through the same frame driver.

**Design decisions this artifact owns (recorded before the code, per the rule that
a design decision updates `PLAN.md` first):**

- **Per-step intent entry point in `FixedStepDriver`.** A-019 §4 requires input
  sampled once per step, and A-019 §10 says a 30 fps target runs two sim steps
  per rendered frame through the same accumulator. The phase-1 `advance(s, wall, i)`
  carries ONE `Intent` for the whole frame, so at 30 fps the second step would
  silently reuse the first step's intent, and H-07 could never show the 30 fps
  trajectory equal to the 60 fps one. A-022 adds `advanceDigest(s, wall, digest)`
  beside it: exactly one `InputDigest.decode` per delivered step (`InputDigest`
  already lives in `SimCore`, so this adds no Bridge dependency to the sim),
  sharing ONE accumulator/catch-up/dropped-step implementation with `advance`
  through a single private `deliver` — no duplicated budget logic. `advance`
  keeps its meaning (one intent for the whole frame: the live-device entry, where
  the per-frame to per-step mapping at 30 fps is A-011's decision, not this
  artifact's) and its comment states that boundary. `FixedStepDriver.cs` is a
  hashed `SimCore` file, so `forkExpected` is re-blessed and both goldens re-verified;
  the goldens step through `FixedStepDriver.step`, which is unchanged, so they
  must stay byte-identical.
- **The frame clock is per-run state, not globals.** H-06/H-07/H-09 compare two
  or three frame-driven runs at once, so `FrameDriver` owns a `FrameClock` object
  (frame index, hitch schedule, tier) handed to `frame(clock, state, digest)`;
  static globals would let one run's clock corrupt another's comparison.
- **H-06's precise assertion.** A-019 §9 words the hitch test as "changes
  `droppedSteps` and nothing else". A hitch smaller than the catch-up budget
  does NOT change `droppedSteps` — A-019 §10 already says `droppedSteps` "only
  grows through the `MAX_CATCHUP = 4` rule". The honest assertion implemented
  here is stronger and stated precisely: the hitch changes frame accounting
  (`framesToGoal`) and, when it exceeds the catch-up budget, `droppedSteps`,
  and nothing else — the goal step index and every observable snapshot are
  byte-identical. Both a within-budget (42 ms) and an over-budget (100 ms)
  schedule are run so the drop path is proven live, not dead code.
- **The snapshot's trailing seal moves with `droppedSteps`.** A-019 §11's
  `EHSNAP1` trailer is an FNV-1a seal over the snapshot bytes, so it changes
  whenever `droppedSteps` does. The H-06 comparison therefore excludes exactly
  those two adjacent trailing fields and nothing else, and verifies the window
  really is those two fields (16 bytes, adjacent) by asking `Counters.fieldName`
  rather than recomputing snapshot offsets — duplicated layout arithmetic is the
  drift that would let a real difference hide inside a wrong window. Comparing
  every other byte is strictly stronger than comparing the seal.
- **Two latent contract drifts found by running the phase-2 commands, both in
  hashed `SimCore` files, both fixed here rather than papered over.**
  (1) `SpawnDirector.apply` documents itself as "called by `SimStep` before
  `AttractionSystem`", but phase-1 `SimStep.run` never called it — `runLoop` did
  — so any other entry point (the frame driver) would run a level with no
  spawns. It moved into `SimStep.run` as step 0), before `PlayerController`,
  preserving exactly the order the goldens were produced with, and was removed
  from `runLoop`. (2) `InputDigest.encode` initialised its write cursor at 0, so
  `writeUvarint` overwrote the magic and version bytes; nothing in phase 1 ever
  called `encode` (the harness reads a committed `EIDIG1` fixture, it never
  writes one), so the bug was latent, and H-08's round-trip is the first caller
  — `decode`'s `BAD_MAGIC` rejection is what surfaced it. The committed golden
  fixture shows the intended layout (magic, ver u16, uvarint step count), so the
  cursor starts at 8. Both files are hashed, so `forkExpected` is re-blessed
  twice: `0xb6b01e1cff3f7710` -> `0x00200c7f119c888d` (frame-driver edits to
  `FixedStepDriver`/`SimStep`) -> `0x99dd49de20c48bc4` (the `encode` fix), with
  both goldens re-verified byte-identical at every step.
- **`fileRecord` gains an explicit `hardwareOnly` parameter.** A-019 §9 says
  H-07's thermal half keeps a hardware-only component "visibly split rather than
  silently skipped", but phase-1 `fileRecord` hardcoded `hardwareOnly: false`,
  so the record could not say it. It became `writeRecord(result, hardwareOnly)`
  with `fileRecord(result)` delegating `false`; a mutable global would let the
  flag leak into the next command's record.
- **H-08's fixture is committed, not generated at run time.** `replays/h-08-
  pointer.txt` is a checked-in `ehpointer1` telemetry script so the test is
  reproducible from the repo; its values are already-resolved intents, because
  `InputAdapter` is transport only and pointer-position -> intent resolution is
  A-011's scheme, out of this artifact's scope.

**Status:**

- doing (2026-10-07, on `feature/programmer-frame-driver` from `develop`)
- done (2026-10-07, same branch; acceptance criteria verified before the pull
  request, merge into `develop` completes it per the established pattern)

**Acceptance evidence (2026-10-07, headless build `dotnet build harness.csproj -o obj`):**

- Build: `Build succeeded. 0 Warning(s) 0 Error(s)`.
- `DT` unchanged: `public static readonly long DT = 71582788L` (Q32.32 raw 1/60),
  and `configureStartup` rejects `--time-scale 2.0` with a named reason and exit 2,
  so no fps or time-scale setting can move `DT`.
- FIXED-FORK: re-blessed to `0x99dd49de20c48bc4` over the same 19 hashed files;
  `replays/p1-level-01.json` regenerated by `sim --out` is byte-identical to the
  committed golden, and H-02 replays it with `PASS snapshotsCompared=6`.
- H-06 `hitch --at 120,341,902 --ms 42` exit 0: goal step 2828 identical,
  2828 frames unhitched vs 2821 hitched, `droppedSteps=0` within budget, and the
  forced 100 ms schedule gives `droppedSteps=3` with `maxStepsPerFrame=4`, so the
  drop path is live; 5639 snapshot comparisons byte-identical outside the
  `droppedSteps` + seal window.
- H-07 `fps --target 30` exit 0: `goalStep=2828 goalStepAtTarget=2828`,
  `stepsPerFrame=2`, `framesToGoal=1414`, `droppedSteps=0`, record carries
  `hardwareOnly: true`.
- H-08 `pointer --script @replays/h-08-pointer.txt` exit 0: 10 directives, 2 dup
  steps discarded first-contact-wins, digest round-tripped through
  `InputDigest.decode` every 100 steps, two runs of 9300 steps byte-identical
  (`snapshotsCompared=17`).
- H-09 `tier --probe` exit 0: three tiers, `goalSteps=2828,2828,2828`,
  `renderPixelsPerUnit` distinct per tier, 2828 snapshots byte-identical across
  tiers for the same `(seed, inputDigest)`; `deviceTier=unknown` stated honestly
  rather than a guessed default.

### A-023 Unity player package

**Purpose:**

- The player-facing build for the device classes named in A-014.
- Render layer, UI root, input adapter, platform bridge over the same `SimCore`.
- The candidate ship build whose version string is governed by A-020 §2.

**Status:**

- proposed (carries an environment gate: no Unity toolchain in this environment)

### A-024 docs/policy-hosting.md

**Purpose:**

- Publish the policy page and record the single `POLICY_URL` (A-020 STEP-10, PR-2/PR-3).
- Name the host, the policy text version published, and every place the URL is reused.

**Design decisions this artifact owns (recorded before the artifact):**

- **Host segment (probed 2026-10-09, not assumed).** A-020 §6 pre-names
  `POLICY_URL = https://event-horizon-game.github.io/privacy-policy/` and its
  own rule fixes the org/user segment at the moment of publishing. That
  segment is not ours: `api.github.com/orgs/Event-Horizon-Game` returns an
  organization created 2024-07-26 holding zero repositories, and `gh api
  user/orgs` returns no organization for the authenticated account
  `danielfcastro`. GitHub names are unique and case-insensitive, so no
  casing of that segment is available to us and the pre-named URL can never
  be published. A-020 §6 therefore resolves to the writable host: user
  segment `danielfcastro`, repository `privacy-policy`, GitHub Pages over
  HTTPS, giving the canonical recorded value
  `POLICY_URL = https://danielfcastro.github.io/privacy-policy/`. The
  pre-named string in A-020 §6 and in the page-source header is corrected to
  this value in the same branch, so no consumer re-derives a URL that
  cannot exist.
- **The gate is closed, not claimed.** A-024 is done only with the live URL
  fetched and its served body compared against the page source; a repo
  created and a build scheduled are not evidence that a page was published.

**Status:**

- done (merged via PR #32 into develop at 866fa08, branch deleted locally and
  remotely; the hosting gate was closed by probing the segment, not by declaring
  it, and the live page was fetched and diffed against the page source by the
  coordinator rather than trusting the author's report)

### A-025 docs/submission.md and docs/ship-notes/<version>.md

**Purpose:**

- Execute A-020 §4.1 submission mechanics against the real consoles.
- File the ship notes of A-020 §9, one line of gate evidence per row of A-020 §4.

**Status:**

- proposed (carries an environment gate: no store consoles and no A-023 ship build in this environment)

### A-026 Non-Unity player package (SDL2 windowed client)

**Purpose:**

- Put the black hole of A-005 §5.1 on a screen and let a person steer it, in an
  environment that cannot run A-023 because it has no Unity toolchain.
- Keep the rule that made A-021..A-022 worth having: one simulation, proven
  deterministic. A-026 adds a renderer, never a second simulation.

**Design decisions this artifact owns (recorded before the code):**

- **Two processes, one simulation.** The C# side owns the sim and the frame
  clock; the C side owns the window and the keyboard. This shape is forced, not
  chosen: this .NET 8 toolchain cannot call a C library from C#. Probed, not
  assumed — `#pragma DLI_Import` is rejected as `CS1633 Unrecognized #pragma
  directive` and `extern "C"` parses as a storage modifier (`CS1003 'alias'
  expected`), so dynamic-library linking into SDL2 is unavailable. The compiler
  is the oracle here, as everywhere in this repo.
- **The renderer reads `SimState`, it never writes to it.** This is the contract
  `RenderLayer.cs` was written and excluded for, made real and compiled. The
  sim side reaches the run through `FrameDriver.frameLive(clock, state, intent)`,
  the documented live-device entry, so the exactly-one-loop rule of A-019 §11
  still holds and no stepping code is duplicated.
- **Live input uses the same quantization as the replay channel.** A key selects
  one of the 16 direction codes of `InputDigest.ANGLE_X/ANGLE_Y` (16 = coast),
  and the `Intent` is built from that table, so a keyboard run and a recorded
  run of the same key sequence agree. Pointer-position -> intent resolution
  stays A-011's and out of scope.
- **Exchange is by file, atomically, never by pipe.** The sim writes the newest
  frame payload to a temp file and renames it into place; the renderer reads
  whatever complete payload is newest. A pipe would block the sim whenever the
  renderer has not consumed a frame, which is a stall that looks like a bug in
  the sim. The renderer writes the newest key state the same way; the sim reads
  it once per frame, which is exactly the sampling granularity A-019 §4 already
  defines.
- **The payload is the observable state, not a render product.** Step index,
  hole position/radius/mass, active bodies, `pixelsPerUnit`, result flag. That
  is what makes the acceptance criterion checkable: a payload dumped at step N
  must agree with the harness snapshot emitted at step N.
- **`pixelsPerUnit` stays render-only.** The camera follows the hole and the
  WU -> px mapping happens in the renderer, per A-019 §6. `DT` is untouched.

**Extension (2026-10-10), recorded before the code, after looking at the frames:**

- **Defect found by sight, not by arithmetic.** `view.c` drew
  `wu_to_px(p.holeRadius, p.ppu)` — the collision radius, which is constant at
  1.0 WU — so the rendered hole was the same 32 px disc at step 1 and at step
  1800 while `holeMass` went 100 -> 567. The earlier proof passed because it
  counted non-background pixels of a constant disc. PLAN.md §5.3 says to keep the
  event horizon visible and to zoom as the hole grows; the client did neither.
- **The payload must carry `holeEventRadius`.** This is a protocol change: the
  documented layout in `SimHost.cs` and the parser in `view.c` move together, in
  one change, so the two halves cannot drift. The golden snapshot already carries
  the field (`Snapshot.cs:196`, "seventh hole i64 = eventRadius", A-019 §7), so
  the payload-vs-snapshot acceptance criterion stays checkable and no new state
  is invented.
- **The visible hole is the event horizon.** `RenderLayer` draws
  `holeEventRadius` at the §5.3 zoom; `holeRadius` (the collision core) is not
  drawn as the hole. This is what §5.4 means by "Event Horizon Radius | Visual
  gravitational boundary".

**Status:**

- done (merged into develop via PR #31 at 79ed193; branch `feature/programmer-player-sdl` deleted locally and remotely; commits d8bdd34 docs(briefs):, c161717 code(player):, d1e98ee chore(repo):). Verified post-merge on develop from a clean obj: harness build 0 Warning(s) 0 Error(s), fork guard silent (forkExpected 0x99dd49de20c48bc4 unchanged), H-01 digest=6d25ff0add639448 steps=2828 droppedSteps=0 result=goal exit 0, fresh sim BYTE-IDENTICAL to the golden, H-02 PASS snapshotsCompared=6, player build 0/0 and replay goalStep=2828 droppedSteps=0 exit 0. Full checklist evidence and the three defects found by running and fixed (wu_to_px Q64.64 shift, --live flag, the documented gcc link command needing -lm) are recorded in PROGRESS.md 2026-10-08 entries. The visual desktop confirmation of the window is recorded as not-executable in this environment — never claimed green.)

## 5. Core game design plan

### 5.1 Concept

The player controls a small black hole moving through space. The black hole attracts nearby bodies with a gravitational field. Absorbed bodies increase the black hole's mass, which increases its event horizon and gravitational pull.

Core fantasy:

```text
small black hole
  -> attracts debris
  -> absorbs mass
  -> grows event horizon
  -> pulls larger objects
  -> upgrades gravity
  -> survives hazards
  -> reaches target mass
```

### 5.2 Controls

Default mobile control:

- Direct drag.
- Player drags the black hole across the screen.
- Camera follows the black hole.
- Optional joystick and tilt controls.

Recommended:

```text
Default: drag
Alternative: virtual joystick
Alternative: device tilt
```

### 5.3 Camera

Camera follows the black hole.

As the black hole grows:

- Zoom out smoothly.
- Show more field.
- Keep event horizon visible.
- Keep mass meter visible.

```text
camera follows black hole
zoom = f(event horizon radius, mass)
```

`f` is made concrete here (decided 2026-10-10, because A-026 shipped a fixed
`pixelsPerUnit` and the visible hole never grew):

```text
pixelsPerUnit = clamp(0.62 * viewportShortEdgePx / (2 * eventRadiusWU),
                      MIN_PPU, MAX_PPU)
MIN_PPU = 2      MAX_PPU = 32
```

The event horizon diameter is held at 62% of the viewport short edge, so it is
always on screen and the camera only ever zooms out as mass grows. `MAX_PPU`
keeps the opening view equal to the client's original fixed scale; `MIN_PPU`
keeps bodies at a few pixels so the field stays readable at the end of a run.
The value is continuous in `eventRadius`, so the zoom is smooth, not stepped.
`pixelsPerUnit` stays render-only: it is chosen by the renderer and never
reaches `SimCore`.

### 5.4 Black hole stats

| Stat | Meaning |
| --- | --- |
| Mass | Main progression stat |
| Event Horizon Radius | Visual gravitational boundary |
| Pull Strength | Gravitational attraction |
| Absorption Radius | Distance required to absorb |
| Speed | Player-controlled movement speed |
| Stability | Prevents runaway growth or overload |
| Efficiency | How much absorbed mass becomes useful mass |
| Combo | Streak bonus for chaining absorptions |

### 5.5 Starting black hole types

**Star Mass Black Hole**

Theme: stable, reliable, slow-growth.

Traits:

- Stable mass growth.
- Reliable pull.
- Better absorption efficiency.
- Lower volatility.
- Good for beginners.

Stats:

```text
Mass:        stable
Pull:        medium
Efficiency: high
Stability:   high
Speed:       medium
```

**Quantum Black Hole**

Theme: unstable, high-power, high-risk.

Traits:

- Stronger pull.
- Faster growth.
- More volatile.
- Can emit quantum particles.
- Can phase-shift absorption modes.
- Can chain-react with anomalies.
- Higher risk of instability.

Stats:

```text
Mass:        volatile
Pull:        high
Efficiency: medium
Stability:   low
Speed:       high
```

### 5.6 Physics model

Use arcade physics, not full real astrophysics.

Mass growth:

```text
massGain = bodyMass * efficiency
```

Event horizon growth:

```text
eventRadius = baseRadius + a * sqrt(mass)
```

Pull strength:

```text
pullStrength = pullScale * mass^0.75
```

Attraction:

```text
direction = blackHole.pos - body.pos
distanceSquared = direction.lengthSquared() + epsilon
force = pullStrength * bodyMass / distanceSquared
body.velocity += direction.normalized() * force * deltaTime
```

Absorption:

```text
if distance(body, blackHole) < absorptionRadius:
    absorb(body)
```

### 5.7 Object types

Small:

- Dust
- Pebbles
- Ice fragments
- Small debris

Medium:

- Rocks
- Metal shards
- Ship fragments
- Asteroids

Large:

- Asteroid chunks
- Small planets
- Star fragments
- Quantum anomalies

Hazards:

- Mines
- Gravity anchors
- Magnetic fields
- Collapsing debris
- Massive objects

### 5.8 Level goals

Examples:

```text
Reach target mass.
Survive time.
Maintain combo.
Avoid hazards.
Absorb anomalies.
Use efficiency challenge.
```

### 5.9 Upgrade systems

Use two systems:

- Permanent meta-progression.
- Temporary run upgrades.

Permanent upgrades:

- Gravity Lens
- Accretion Core
- Horizon Stabilizer
- Inertial Dampener
- Combo Anchor
- Quantum Core
- Star Core

Temporary run upgrades:

```text
+15% pull strength
+10% absorption radius
+10% movement speed
+10% mass efficiency
+1 quantum anomaly spawn
```

### 5.10 Progression curve

Use diminishing returns:

```text
mass gain is direct
event horizon grows with sqrt(mass)
pull grows with mass^0.75
speed decreases slightly as mass grows
```

Example target masses:

```text
Level 1: 1,000
Level 5: 10,000
Level 10: 50,000
Level 20: 250,000
Level 40: 1,000,000
```

### 5.11 Failure conditions

Possible:

- Time limit.
- Stability reaches zero.
- Too many hazard collisions.
- Mass drops below threshold.

Recommended:

```text
Time limit + stability + hazard pressure
```

### 5.12 Campaign worlds

**World 1: Dust Belt**

- Teaches movement, attraction, absorption.
- Dust, pebbles, small rocks.
- No hazards or very few.

**World 2: Asteroid Field**

- Teaches larger objects and clustering.
- Rocks, asteroids, metal shards.

**World 3: Ship Graveyard**

- Teaches high-value targets and path planning.
- Ship fragments, mines, magnetic fields.

**World 4: Planet Ring**

- Teaches large mass and stability management.
- Planet fragments, gravity anchors, heavy objects.

**World 5: Quantum Nebula**

- Teaches quantum mode, phase shifts, chain reactions.
- Quantum anomalies, phase particles, unstable cores.

### 5.13 Endgame modes

- Campaign.
- Endless mode.
- Daily challenge.
- Weekly challenge.
- Leaderboards.

### 5.14 Monetization

Recommended:

- Free-to-play.
- No pay-to-win.
- Optional ads.
- Optional IAP for cosmetics.
- Optional ad removal.
- Optional premium version.

Ads placement:

- Game over.
- Retry.
- Between levels.
- Optional rewarded ad.

Avoid:

- Ads during gameplay.
- Forced interstitials.
- Aggressive ad walls.

### 5.15 Tech stack recommendation

Best overall:

```text
Unity 2D
+ C#
+ URP
+ custom gravity simulation
+ spatial hashing
+ object pooling
+ simple shaders
+ Unity Input System
+ Unity Ads / IAP
+ analytics SDK
```

Lightweight alternative:

```text
Godot 4
+ GDScript
+ custom gravity simulation
+ spatial partitioning
+ object pooling
+ simple shaders
```

### 5.16 Performance plan

Targets:

```text
60 fps on modern phones
30 fps fallback on weak devices
low memory
low draw calls
low particle count
```

Use:

- Object pooling.
- Spatial hashing.
- Culling.
- LOD.
- Simple shaders.
- Fixed timestep simulation.

### 5.17 QA plan

Input:

- Drag works on small and large phones.
- Joystick works.
- Pause button is large enough.
- UI fits safe areas.

Performance:

- Stable memory.
- No leaks.
- Low draw calls.
- Simple particles.
- No shader spikes.

Balance:

- Level 1 completable.
- Later levels challenging but fair.
- Quantum mode not too strong.
- Star mode not too weak.
- Endless mode does not stall.

Store:

- Screenshots clear.
- Trailer short.
- Age rating correct.
- Privacy policy exists.
- IAP works.
- Ads work.
- Analytics works.
- Crash reporting works.

### 5.18 Development roadmap

**Phase 1: Prototype**

- One black hole.
- One level.
- Move, attract, absorb, grow.

**Phase 2: Game feel**

- Particles.
- Camera zoom.
- Sound.
- Mass meter.
- Combo.
- Upgrade feedback.

**Phase 3: Balance**

- Level goals.
- Time limits.
- Stability.
- Object values.
- Upgrade choices.
- Difficulty curve.

**Phase 4: Content**

- 30-60 levels.
- Worlds.
- Unlock system.
- Meta upgrades.
- Endless mode.

**Phase 5: Mobile polish**

- UI scaling.
- Settings.
- Accessibility.
- Localization.
- Analytics.
- Ads/IAP.
- Store compliance.
- QA.

## 6. First next actions for agents

As of 2026-10-09 every artifact `A-001`..`A-022`, `A-024` and `A-026` is done, so the list below is **historical**. `A-026` merged via PR #31 at 79ed193; `A-024` merged via PR #32 at 866fa08, its hosting gate closed by publishing the policy page at a real HTTPS URL and fetching it back. No further artifact is executable in this environment: `A-023` waits on the Unity toolchain and on real iOS/Android device classes, `A-025` waits on store consoles and on `A-023`, and contact verification needs a real inbox with a human reply within 7 days. The run is at a gate, not finished.

The next artifacts to develop should be:

```text
A-002 README.md
A-003 AGENTS.md
A-004 docs/spec.md
A-005 docs/architecture.md
A-006 docs/design.md
```

Suggested first agent order:

```text
coordinator:
  create README.md
  create AGENTS.md

planner:
  create docs/spec.md

architect:
  create docs/architecture.md

designer:
  create docs/design.md

balance:
  create docs/balance.md

level-designer:
  create docs/content.md
  create docs/levels.md
```

## 7. Recommended repository structure

```text
event-horizon-game/
├── PLAN.md
├── PROGRESS.md
├── README.md
├── AGENTS.md
├── docs/
│   ├── spec.md
│   ├── architecture.md
│   ├── design.md
│   ├── balance.md
│   ├── content.md
│   ├── levels.md
│   ├── ui.md
│   ├── input.md
│   ├── accessibility.md
│   ├── test-plan.md
│   ├── qa.md
│   ├── store.md
│   ├── monetization.md
│   ├── privacy-policy.md
│   ├── agent-rules.md
│   ├── prototype-scaffold.md
│   ├── ship.md
│   ├── policy-hosting.md
│   ├── submission.md
│   ├── ship-notes/
│   │   └── <version>.md
│   ├── briefs/
│   │   └── <artifact-id>.md
│   └── policy-page/
│       └── <policy-text-version>.md
├── Assets/
│   └── Runtime/
│       ├── Fixed/
│       ├── SimCore/
│       └── Bridge/Unity/
├── tools/
│   └── harness/
├── replays/
├── harness.csproj
```

## 8. Important design rule

Do not make the game a pure power fantasy.

More mass should create tension:

```text
more mass = stronger pull
more mass = larger collision risk
more mass = slower precise movement
more mass = harder navigation
more mass = stability may decay
```

This gives players real decisions:

```text
Grow fast and risk instability?
Stay smaller and move precisely?
Upgrade for efficiency or pull?
Absorb large objects or avoid hazards?
```

## 9. Agent runtime configuration (OpenCode)

The agents in section 1.1 are implemented as OpenCode agents. This section records the configuration so any agent or person can reproduce the setup.

### 9.1 Configuration location

- Global (applies to every project, currently in use): `~/.config/opencode/opencode.json`
- Project-scoped alternative (applies only to this repo): `.opencode/opencode.json` at the repo root
- Use one location or the other, not both, to avoid confusing precedence.

### 9.2 Required configuration

Set `default_agent` to `coordinator` and define the nine agents with `mode: "all"` so the coordinator can run as the session agent and launch the role agents as subagents. The coordinator also needs `subagent` permission to launch role agents.

Each agent definition names the artifacts it owns, the upstream documents it must read before writing, and the artifacts that consume its output. Those names MUST match the sequence table in section 2 and the dependency graph in section 3. When they disagree, fix the agent definition, not the table.

```jsonc
{
  "default_agent": "coordinator",
  "agents": {
    "coordinator": {
      "description": "Master orchestrator: runs the whole development plan, delegates each artifact to the right agent, and keeps progress resumable.",
      "mode": "all",
      "options": { "max_tokens": 16000 },
      "system": "You are the coordinator and single orchestrator for the Event Horizon game repo. PLAN.md is the canonical source of truth for design; PROGRESS.md at the repo root is the control file for run state. You own the document artifacts A-001 PLAN.md, A-002 README.md, and A-003 AGENTS.md; every other artifact is delegated to the role agent that owns it. Resume protocol: at the start of every session read PLAN.md then PROGRESS.md and continue exactly from the recorded state; never redo completed work and never skip dependencies. Orchestration loop: select the first artifact whose status is ready in PLAN.md; set it to doing in PLAN.md; launch the matching role agent as a subagent (planner, architect, designer, balance, level-designer, programmer, qa, release) passing the artifact ID, its purpose, and its acceptance criteria; when the subagent returns the artifact, review it against PLAN.md and its acceptance criteria; commit and merge through git-flow (feature/<agent>-<artifact-id> branch from develop, merge into develop, pull request into main only when approved); set the artifact to done in PLAN.md; then append a dated entry to PROGRESS.md recording artifact ID, branch, PR, key decisions, and the next ready artifact. Update PROGRESS.md after every meaningful step, including partial ones, so an interrupted run can resume exactly where it stopped. If interrupted or blocked, record the exact stopping point and reason in PROGRESS.md and stop. Do not make design decisions that conflict with PLAN.md; update PLAN.md first. Use the documented commit prefixes such as docs(plan): and chore(repo):.",
      "permissions": [
        { "action": "subagent", "resource": "*", "effect": "allow" }
      ]
    },
    "planner": {
      "description": "Writes specifications, design plans, and game systems.",
      "mode": "all",
      "options": { "max_tokens": 16000 },
      "system": "You are the planner for the Event Horizon game repo. You own A-004 docs/spec.md (depends on A-001 PLAN.md) and A-018 docs/agent-rules.md (depends on A-003 AGENTS.md). Before writing, read PLAN.md, PROGRESS.md, and the approved upstream documents named above. Your output is consumed by A-005 docs/architecture.md, A-006 docs/design.md, A-013 docs/test-plan.md, and A-019 docs/prototype-scaffold.md. Read PLAN.md first and follow the artifact workflow and Git-flow rules there. Write specifications, design plans, and game system documents that are consistent with PLAN.md. Do not start an artifact whose dependencies are not done. If a design decision conflicts with PLAN.md, update PLAN.md first. Work on a git-flow feature branch created from develop and open a pull request; never push directly to main."
    },
    "architect": {
      "description": "Defines technical architecture, data structures, modules, and platform plan.",
      "mode": "all",
      "options": { "max_tokens": 16000 },
      "system": "You are the architect for the Event Horizon game repo. You own A-005 docs/architecture.md (depends on A-004 docs/spec.md) and A-019 docs/prototype-scaffold.md (depends on A-005 and A-013 docs/test-plan.md). Before writing, read PLAN.md, PROGRESS.md, and the approved upstream documents named above. Your output is consumed by A-013 docs/test-plan.md, A-019 docs/prototype-scaffold.md, and all code work. Read PLAN.md first and keep every technical decision consistent with it and with approved upstream artifacts. Define technical architecture, modules, simulation loops, data structures, performance plans, and platform choices. Work on a git-flow feature branch created from develop and open a pull request; never push directly to main."
    },
    "designer": {
      "description": "Defines game feel, visuals, UI, camera, feedback, and accessibility.",
      "mode": "all",
      "options": { "max_tokens": 16000 },
      "system": "You are the designer for the Event Horizon game repo. You own A-006 docs/design.md (depends on A-004 docs/spec.md), A-010 docs/ui.md (depends on A-006), A-011 docs/input.md (depends on A-010), and A-012 docs/accessibility.md (depends on A-010). Before writing, read PLAN.md, PROGRESS.md, and the approved upstream documents named above. Your output is consumed by A-007 docs/balance.md and A-008 docs/content.md. Read PLAN.md first and keep every decision consistent with it and with approved upstream artifacts. Define game feel, visuals, UI layout, camera behavior, feedback, and accessibility for mobile. Work on a git-flow feature branch created from develop and open a pull request; never push directly to main."
    },
    "balance": {
      "description": "Defines progression curves, economy, difficulty, upgrade values.",
      "mode": "all",
      "options": { "max_tokens": 16000 },
      "system": "You are the balance agent for the Event Horizon game repo. You own A-007 docs/balance.md (depends on A-006 docs/design.md). Before writing, read PLAN.md, PROGRESS.md, and the approved upstream documents named above. Your output is consumed by A-008 docs/content.md and A-009 docs/levels.md. Read PLAN.md first and keep every number consistent with it and with approved upstream artifacts. Define progression curves, mass growth, upgrade values, difficulty targets, and economy. Respect the design rule that more mass creates tension; do not make the game a pure power fantasy. Work on a git-flow feature branch created from develop and open a pull request; never push directly to main."
    },
    "level-designer": {
      "description": "Defines objects, hazards, levels, worlds, and content.",
      "mode": "all",
      "options": { "max_tokens": 16000 },
      "system": "You are the level-designer for the Event Horizon game repo. You own A-008 docs/content.md (depends on A-006 docs/design.md and A-007 docs/balance.md) and A-009 docs/levels.md (depends on A-008). Before writing, read PLAN.md, PROGRESS.md, and the approved upstream documents named above. Your output is consumed by A-009 docs/levels.md and the shipping artifacts. Read PLAN.md first and keep every decision consistent with it and with approved upstream artifacts. Define objects, hazards, upgrades, modifiers, rewards, campaign worlds, level goals, and the difficulty curve. Work on a git-flow feature branch created from develop and open a pull request; never push directly to main."
    },
    "programmer": {
      "description": "Creates code scaffolding, prototypes, and implementation notes.",
      "mode": "all",
      "options": { "max_tokens": 16000 },
      "system": "You are the programmer for the Event Horizon game repo. You own A-021 Phase 1 prototype code (Assets/ + tools/harness/, depends on A-005 docs/architecture.md, A-013 docs/test-plan.md, and A-019 docs/prototype-scaffold.md as its entry point), A-022 Phase 2 frame driver and render state (depends on A-005, A-019, A-021), and A-023 Unity player package (depends on A-005, A-022). You also own A-026 Non-Unity player package, an SDL2 windowed client that renders the SAME SimState without Unity (depends on A-005, A-019, A-021, A-022), because A-023's Unity gate otherwise leaves the repo with nothing a human can see. Start only after A-005 docs/architecture.md and A-013 docs/test-plan.md are done, and follow A-019 docs/prototype-scaffold.md as the entry point. A-023 carries an environment gate: it is not startable without the Unity toolchain, and it is never reported done without the evidence its acceptance criteria name. Read PLAN.md, PROGRESS.md, and the approved upstream documents A-005 docs/architecture.md, A-006 docs/design.md, A-007 docs/balance.md, A-008 docs/content.md, A-009 docs/levels.md, A-010 docs/ui.md, A-011 docs/input.md, and A-012 docs/accessibility.md before writing code. Never write code that contradicts those documents. Create code scaffolding, prototypes, and implementation notes on git-flow feature branches created from develop, and open a pull request; never push directly to main."
    },
    "qa": {
      "description": "Defines tests, QA checklist, performance checks, and regression risks.",
      "mode": "all",
      "options": { "max_tokens": 16000 },
      "system": "You are the qa agent for the Event Horizon game repo. You own A-013 docs/test-plan.md (depends on A-005 docs/architecture.md) and A-014 docs/qa.md (depends on A-013). Before writing, read PLAN.md, PROGRESS.md, and the approved upstream documents named above. Your output is consumed by A-015 docs/store.md and A-019 docs/prototype-scaffold.md. Read PLAN.md first and define testing strategies, QA checklists, simulation and balance tests, performance checks, and regression risks. Work on a git-flow feature branch created from develop and open a pull request; never push directly to main."
    },
    "release": {
      "description": "Handles store compliance, privacy, analytics, monetization, submission.",
      "mode": "all",
      "options": { "max_tokens": 16000 },
      "system": "You are the release agent for the Event Horizon game repo. You own A-015 docs/store.md (depends on A-014 docs/qa.md), A-016 docs/monetization.md (depends on A-015), A-017 docs/privacy-policy.md (depends on A-015), A-020 docs/ship.md (depends on A-015, A-016, A-017), A-024 docs/policy-hosting.md (depends on A-017, A-020), and A-025 docs/submission.md plus docs/ship-notes/<version>.md (depends on A-020, A-023, A-024). Before writing, read PLAN.md, PROGRESS.md, and the approved upstream documents named above. Read PLAN.md first and keep store listings, monetization, privacy policy, hosting, and shipping steps compliant with it and with platform rules. Monetization must not block core game completion. A-024 and A-025 carry environment gates (a host for the policy page, store consoles): they are never reported done without the evidence their acceptance criteria name. Use git-flow release/* and hotfix/* branches for shipping work; never push directly to main without coordinator approval."
    }
  }
}
```

Global permissions must at least allow `edit` and `shell` so agents can draft artifacts and run git commands:

```jsonc
{
  "permissions": [
    { "action": "edit", "resource": "*", "effect": "allow" },
    { "action": "shell", "resource": "*", "effect": "allow" }
  ]
}
```

### 9.3 Starting autonomous development

1. Restart the OpenCode service so the configuration reloads:

   ```text
   opencode service restart
   ```

2. Start the coordinator from the repo root (it is the default agent):

   ```text
   cd /mnt/data/projetos/event-horizon-game
   opencode run "start development"
   ```

   Or open the interactive TUI with `opencode`; the coordinator is selected automatically.

3. The coordinator then works autonomously: it reads PLAN.md and PROGRESS.md, picks the first ready artifact, delegates it to the matching role subagent, reviews the result, commits and merges through git-flow, updates the artifact status in PLAN.md, and records every step in PROGRESS.md. It continues until all artifacts are done or it is interrupted or blocked.

4. To interrupt: stop the session at any point. To resume later, start a new session:

   ```text
   opencode run "resume development"
   ```

   The coordinator reads PROGRESS.md and continues exactly from the recorded stopping point.

5. To run one role standalone instead of the full loop:

   ```text
   opencode run --agent planner "create docs/spec.md"
   ```

### 9.4 Context briefs

Role agents never read `PLAN.md` or an approved upstream artifact in full. The
coordinator writes one brief per artifact at `docs/briefs/<artifact-id>.md`
before launching the agent, and the agent prompt names only that brief.

A brief contains:

- the artifact contract: ID, owner, dependencies, acceptance criteria, purpose,
  consumers
- the hard design rule from PLAN.md section 8
- only the PLAN.md excerpts that artifact depends on
- the open questions from upstream artifacts that this artifact must answer
- the scope boundaries that later artifacts own

Rules:

- The brief is distilled context, not a replacement for `PLAN.md`. If a brief
  and `PLAN.md` disagree, `PLAN.md` wins and the brief is corrected.
- A brief stays under roughly ten thousand tokens so a role agent session
  cannot approach the model context limit.
- The coordinator regenerates a brief when an upstream artifact changes.
- Briefs are committed with the artifact that used them.

### 9.5 Context controls

Requests fail with "prompt + max tokens exceeds the context; requests are never
truncated" when the prompt plus the model's output budget pass the context
window. Two controls prevent this:

- Every project agent in the configuration of section 9.2 sets
  `"options": { "max_tokens": 16000 }`. With the 262144-token context window
  of the bullet below, this caps any single request at prompt + 16000 output
  tokens, leaving headroom for the largest expected prompt. Never raise this
  value above a quarter of the context window (65536 tokens at 262144).
- Role agents work from the context briefs of section 9.4 and never read
  `PLAN.md` or approved upstream artifacts in full, so a role-agent prompt
  stays far below the context limit. The coordinator regenerates briefs
  instead of re-sending documents.
- The project model is a reasoning model whose internal reasoning counts
  against the output budget; a single request can spend the whole budget
  reasoning and return no content (observed with the programmer on A-021:
  reasoning-only messages of ~60,000 characters hit the 16,000 cap and the
  subagent produced no files). When an agent hits this failure mode, the
  coordinator sets a lower reasoning variant for that agent
  (`"model": "strata/qwen3.8-flash-next-coder-iq1_m#low"`) and splits large
  artifacts into staged subagent calls, each scoped to a few files, with
  files persisting on disk between stages.
- If the lower variant does not shrink the reasoning below the output budget,
  that agent's `max_tokens` and the provider model's declared `limit.output`
  are both raised to 32000 (OpenCode clamps agent-level `max_tokens` to the
  provider limit), still at most a quarter of the 131027-token context. This
  alone was not sufficient for a 15-file scope (reasoning-only message of
  86,004 characters at `finish: length`); the operative control is the
  scope split — subagent calls scoped to three or four files keep the
  model's drafting reasoning inside one message budget. The programmer
  agent for A-021 carries the raise.
- Launch guard: OpenCode rejects a request when prompt + max_tokens
  exceeds the model context ("requests are never truncated"). A
  programmer launch at the 32000 raise failed exactly this way (prompt
  99,714 + 32,000 = 131,714 > 131,027): a role-agent baseline prompt
  runs near 100,000 tokens, so the agent-level budget must leave
  headroom below the window, not just stay under a quarter of it. The
  programmer's max_tokens was therefore trimmed from 32000 to 24000
  (options + request.body), keeping the scope split as the operative
  control.
- Context-watch plugin: `opencode.json` carries a `plugins` entry for
  `opencode-context-watch` (npm package; OpenCode 2.x only, silent
  no-op on 1.x) with `warnPercent: 0.95`, `warnTokens: 249037`,
  `verbose: true`. Verified against the plugin source: the percent band
  is relative to the window the plugin reads from the model's declared
  `limit.context`, so it follows the window automatically; `warnTokens`
  is an absolute band (OR semantics) and must be kept equal to
  `warnPercent` of the window (0.95 × 262144 = 249037). It watches
  each session's context usage and injects a synthetic warning into
  every above-threshold request so agents wrap up or compact before
  the window fills. It adds no tool and never compacts; compaction
  remains OpenCode's job.
- Context budget (changed 2026-10-08): the project model declares
  `limit.context: 262144` (halved from 524288) and `limit.output: 64000`
  (halved with the window, keeping the same share of it). Derived
  controls scale with it: `compaction.buffer` 16000 (the same ~6%
  headroom, so automatic compaction starts at the same fraction of the
  window), `compaction.keep.tokens` 8000 (absolute, unchanged), and the
  plugin's `warnTokens` band at 95% of the window. Agent `max_tokens`
  (16000; programmer 24000) stay far below a quarter of the window
  (65536), and a role-agent baseline prompt near 100,000 tokens plus
  its budget still fits (124,000 < 262,144). The google model keeps
  its own real 1048576-token limit; it is not part of this budget.
  If the window changes again, update every one of these numbers in
  the same commit.
- Fit guard (changed 2026-10-09): the agent-level `max_tokens` of section
  9.2 bounds the requests an agent starts, not every request a session
  makes. A coordinator request at prompt 199,774 asked for 62,407 output
  tokens - the provider's `limit.output` 64000, not any agent's 16000 or
  24000 - and the serve engine refused it with a 400 ("prompt + max
  tokens exceeds the context; requests are never truncated", #545). The
  durable control is `"fit_max_tokens": true` in the model's run
  config `~/Strata/strata-coder-iq1_m.json`, which shortens such a
  `max_tokens` to the room left instead of refusing. A prompt that
  leaves no room at all is still refused. The serve engine reads that
  file when it starts, so the key takes effect at the next start of the
  server, not on the next request; the file lives outside the repo and
  is never committed.

If a coordinator session itself grows too large, OpenCode compacts it
automatically into a summary; the `max_tokens` cap bounds an agent-initiated
request and `fit_max_tokens` bounds the others, so an uncompacted request
never exceeds the context in either case.
