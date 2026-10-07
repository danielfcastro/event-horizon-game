---
status: draft
next_artifact: none (all 21 artifacts done; ship phase executed STEP-01..STEP-02 and STEP-16..STEP-18 on release/0.20, STEP-03..STEP-15 await a player build and store access)
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
│   └── ship.md
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
      "system": "You are the programmer for the Event Horizon game repo. You own A-021 Phase 1 prototype code (Assets/ + tools/harness/, depends on A-005 docs/architecture.md, A-013 docs/test-plan.md, and A-019 docs/prototype-scaffold.md as its entry point). Start only after A-005 docs/architecture.md and A-013 docs/test-plan.md are done, and follow A-019 docs/prototype-scaffold.md as the entry point. Read PLAN.md, PROGRESS.md, and the approved upstream documents A-005 docs/architecture.md, A-006 docs/design.md, A-007 docs/balance.md, A-008 docs/content.md, A-009 docs/levels.md, A-010 docs/ui.md, A-011 docs/input.md, and A-012 docs/accessibility.md before writing code. Never write code that contradicts those documents. Create code scaffolding, prototypes, and implementation notes on git-flow feature branches created from develop, and open a pull request; never push directly to main."
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
      "system": "You are the release agent for the Event Horizon game repo. You own A-015 docs/store.md (depends on A-014 docs/qa.md), A-016 docs/monetization.md (depends on A-015), A-017 docs/privacy-policy.md (depends on A-015), and A-020 docs/ship.md (depends on A-015, A-016, A-017). Before writing, read PLAN.md, PROGRESS.md, and the approved upstream documents named above. Read PLAN.md first and keep store listings, monetization, privacy policy, and shipping steps compliant with it and with platform rules. Monetization must not block core game completion. Use git-flow release/* and hotfix/* branches for shipping work; never push directly to main without coordinator approval."
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
  `"options": { "max_tokens": 16000 }`. With a 131027-token context this caps
  any single request at prompt + 16000 output tokens, leaving headroom for the
  largest expected prompt. Never raise this value above a quarter of the
  context window.
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
  no-op on 1.x) with `warnPercent: 0.7`, `warnTokens: 90000`,
  `verbose: true`. It watches each session's context usage and injects
  a synthetic warning into every above-threshold request so agents
  wrap up or compact before the window fills. It adds no tool and
  never compacts; compaction remains OpenCode's job.

If a coordinator session itself grows too large, OpenCode compacts it
automatically into a summary; the `max_tokens` cap guarantees that even an
uncompacted request never exceeds the context.
