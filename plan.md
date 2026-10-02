
status: draft
next_artifact: A-002
owner: coordinator
project: Event Horizon
platform: iOS, Android

Event Horizon Game Plan
0. Purpose
This plan defines a mobile game where the player controls a growing black hole. The black hole starts small, moves across the screen, attracts bodies with its gravitational field, absorbs them, and grows in mass, event horizon, and gravitational pull.

The repository is intended to be coordinated by AI agents. Each artifact is developed in a specific order so agents do not duplicate work or make incompatible design decisions.

1. Agent coordination protocol
1.1 Agent roles
Agent	Main responsibilities
coordinator	Owns repo workflow, dependency order, PRs, status updates, merge decisions.
planner	Writes specifications, design plans, and game systems.
architect	Defines technical architecture, data structures, modules, and platform plan.
designer	Defines game feel, visuals, UI, camera, feedback, and accessibility.
balance	Defines progression curves, economy, difficulty, upgrade values.
level-designer	Defines objects, hazards, levels, worlds, and content.
programmer	Creates code scaffolding, prototypes, and implementation notes.
qa	Defines tests, QA checklist, performance checks, and regression risks.
release	Handles store compliance, privacy, analytics, monetization, submission.
1.2 Artifact workflow
Every artifact must follow this process:

Read plan.md.
Select the first artifact whose status is ready.
Create a branch named:
```text
dev/<agent>/<artifact-id>
```

Draft the artifact.
Update the artifact status in plan.md:
```text
proposed -> ready -> doing -> done
```

Open a pull request.
Wait for coordinator review.
Merge to main only when approved.
1.3 Branch naming
Use:

text

dev/coordinator/plan
dev/coordinator/agents
dev/planner/spec
dev/architect/architecture
dev/designer/design
dev/balance/balance
dev/level-designer/content
dev/level-designer/levels
dev/designer/ui
dev/designer/input
dev/designer/accessibility
dev/qa/test-plan
dev/qa/qa
dev/release/store
dev/release/monetization
dev/release/privacy-policy
dev/planner/agent-rules
dev/architect/prototype-scaffold
dev/release/ship
1.4 Commit prefixes
Use:

text

docs(plan): ...
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
chore(repo): ...
1.5 Pull request template
Each PR should include:

markdown

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
- [ ] Artifact follows plan.md.
- [ ] Dependencies are complete.
- [ ] No conflicting design decisions.
1.6 Rules
Do not start an artifact if its dependencies are not done.
Do not push directly to main unless the coordinator approves it.
Do not change an artifact without updating plan.md.
If a design decision conflicts with plan.md, update plan.md first.
Keep one canonical source of truth: plan.md.
Code must follow approved architecture and balance documents.
Monetization must not block core game completion.
2. Artifact sequence
The sequence below is ordered so that AI agents can coordinate work safely.

ID	Artifact	Agent	Depends on	Purpose	Status	Acceptance criteria
A-001	plan.md	coordinator	none	Master plan and coordination protocol	done	Contains game concept, artifact order, agent rules.
A-002	README.md	coordinator	A-001	Repo overview	ready	Explains repo and next steps.
A-003	AGENTS.md	coordinator	A-002	Agent roles and workflow	ready	Defines agent responsibilities and branch rules.
A-004	docs/spec.md	planner	A-001	High-level game specification	blocked	Defines core loop, controls, goals, failure conditions.
A-005	docs/architecture.md	architect	A-004	Technical architecture	blocked	Defines modules, simulation loop, data structures, platform plan.
A-006	docs/design.md	designer	A-004	Game feel, visuals, UI, camera	blocked	Defines visual language, feedback, accessibility.
A-007	docs/balance.md	balance	A-006	Progression, economy, difficulty	blocked	Defines mass growth, upgrade values, target masses.
A-008	docs/content.md	level-designer	A-006, A-007	Object types, hazards, upgrades	blocked	Lists objects, hazards, rewards, modifiers.
A-009	docs/levels.md	level-designer	A-008	Level structure and campaign	blocked	Defines worlds, objectives, difficulty curve.
A-010	docs/ui.md	designer	A-006	HUD, menus, settings	blocked	Defines UI layout and mobile scaling.
A-011	docs/input.md	designer	A-010	Mobile controls	blocked	Defines drag, joystick, tilt, accessibility.
A-012	docs/accessibility.md	designer	A-010	Accessibility options	blocked	Defines contrast, reduced motion, larger controls.
A-013	docs/test-plan.md	qa	A-005	Testing strategy	blocked	Defines simulation tests, balance tests, UI tests.
A-014	docs/qa.md	qa	A-013	QA checklist	blocked	Defines manual QA and performance checklist.
A-015	docs/store.md	release	A-014	Store submission plan	blocked	Defines listing, screenshots, trailer, compliance.
A-016	docs/monetization.md	release	A-015	Monetization and economy	blocked	Defines ads, IAP, analytics, fairness.
A-017	docs/privacy-policy.md	release	A-015	Privacy policy	blocked	Defines data collected, contact info, store compliance.
A-018	docs/agent-rules.md	planner	A-003	Expanded agent rules	ready	Detailed rules for AI agents.
A-019	docs/prototype-scaffold.md	architect	A-005, A-013	First code scaffold	blocked	Defines first playable prototype modules.
A-020	docs/ship.md	release	A-015, A-016, A-017	Shipping checklist	blocked	Defines final release steps.
3. Dependency graph
text

A-001 plan.md
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
4. Artifact contracts
A-001 plan.md
Purpose:

Master source of truth.
Defines game concept.
Defines agent workflow.
Defines artifact order.
Status:

done
A-002 README.md
Purpose:

Human-readable repo overview.
Explains what the repo contains.
Explains how to push to a private remote.
Status:

ready
A-003 AGENTS.md
Purpose:

Defines AI agent roles.
Defines branch naming.
Defines PR workflow.
Defines status updates.
Status:

ready
A-004 docs/spec.md
Purpose:

High-level game specification.
Defines core loop, controls, objectives, failure conditions, and player experience.
Status:

blocked
A-005 docs/architecture.md
Purpose:

Technical architecture.
Defines simulation model, modules, data structures, rendering, performance, and platform choices.
Status:

blocked
A-006 docs/design.md
Purpose:

Game feel and presentation.
Defines camera, visuals, UI, particles, sound, feedback, and accessibility.
Status:

blocked
A-007 docs/balance.md
Purpose:

Progression and difficulty.
Defines mass growth, event horizon growth, pull strength, upgrade values, level targets, and economy.
Status:

blocked
A-008 docs/content.md
Purpose:

Content design.
Defines object types, hazards, upgrades, modifiers, rewards, and special black hole types.
Status:

blocked
A-009 docs/levels.md
Purpose:

Level design.
Defines campaign worlds, level goals, difficulty curve, endless mode, and daily challenges.
Status:

blocked
A-010 docs/ui.md
Purpose:

UI design.
Defines HUD, menus, settings, buttons, mobile scaling, and safe areas.
Status:

blocked
A-011 docs/input.md
Purpose:

Input design.
Defines drag control, joystick, tilt, touch handling, and accessibility.
Status:

blocked
A-012 docs/accessibility.md
Purpose:

Accessibility design.
Defines reduced motion, high contrast, larger buttons, colorblind-safe palette, and alternative controls.
Status:

blocked
A-013 docs/test-plan.md
Purpose:

Testing plan.
Defines simulation tests, balance tests, UI tests, performance tests, and regression risks.
Status:

blocked
A-014 docs/qa.md
Purpose:

QA checklist.
Defines manual QA, device testing, performance testing, and store readiness.
Status:

blocked
A-015 docs/store.md
Purpose:

Store submission plan.
Defines title, description, screenshots, trailer, age rating, localization, and compliance.
Status:

blocked
A-016 docs/monetization.md
Purpose:

Monetization plan.
Defines free-to-play structure, ads, IAP, analytics, and fairness rules.
Status:

blocked
A-017 docs/privacy-policy.md
Purpose:

Privacy policy.
Defines data collection, analytics disclosure, contact info, and store compliance.
Status:

blocked
A-018 docs/agent-rules.md
Purpose:

Expanded agent rules.
Defines how agents update status, create PRs, resolve conflicts, and avoid duplicate work.
Status:

ready
A-019 docs/prototype-scaffold.md
Purpose:

First code scaffold.
Defines modules for black hole, bodies, gravity, absorption, camera, UI, and input.
Status:

blocked
A-020 docs/ship.md
Purpose:

Shipping checklist.
Defines final build, store submission, remote setup, analytics, ads/IAP, and launch steps.
Status:

blocked
5. Core game design plan
5.1 Concept
The player controls a small black hole moving through space. The black hole attracts nearby bodies with a gravitational field. Absorbed bodies increase the black hole’s mass, which increases its event horizon and gravitational pull.

Core fantasy:

text

small black hole
  -> attracts debris
  -> absorbs mass
  -> grows event horizon
  -> pulls larger objects
  -> upgrades gravity
  -> survives hazards
  -> reaches target mass
5.2 Controls
Default mobile control:

Direct drag.
Player drags the black hole across the screen.
Camera follows the black hole.
Optional joystick and tilt controls.
Recommended:

text

Default: drag
Alternative: virtual joystick
Alternative: device tilt
5.3 Camera
Camera follows the black hole.

As the black hole grows:

Zoom out smoothly.
Show more field.
Keep event horizon visible.
Keep mass meter visible.
text

camera follows black hole
zoom = f(event horizon radius, mass)
5.4 Black hole stats
Stat	Meaning
Mass	Main progression stat
Event Horizon Radius	Visual gravitational boundary
Pull Strength	Gravitational attraction
Absorption Radius	Distance required to absorb
Speed	Player-controlled movement speed
Stability	Prevents runaway growth or overload
Efficiency	How much absorbed mass becomes useful mass
Combo	Streak bonus for chaining absorptions
5.5 Starting black hole types
Star Mass Black Hole
Theme: stable, reliable, slow-growth.

Traits:

Stable mass growth.
Reliable pull.
Better absorption efficiency.
Lower volatility.
Good for beginners.
Stats:

text

Mass:        stable
Pull:        medium
Efficiency: high
Stability:   high
Speed:       medium
Quantum Black Hole
Theme: unstable, high-power, high-risk.

Traits:

Stronger pull.
Faster growth.
More volatile.
Can emit quantum particles.
Can phase-shift absorption modes.
Can chain-react with anomalies.
Higher risk of instability.
Stats:

text

Mass:        volatile
Pull:        high
Efficiency: medium
Stability:   low
Speed:       high
5.6 Physics model
Use arcade physics, not full real astrophysics.

Mass growth:

text

massGain = bodyMass * efficiency
Event horizon growth:

text

eventRadius = baseRadius + a * sqrt(mass)
Pull strength:

text

pullStrength = pullScale * mass^0.75
Attraction:

text

direction = blackHole.pos - body.pos
distanceSquared = direction.lengthSquared() + epsilon
force = pullStrength * bodyMass / distanceSquared
body.velocity += direction.normalized() * force * deltaTime
Absorption:

text

if distance(body, blackHole) < absorptionRadius:
    absorb(body)
5.7 Object types
Small:

Dust
Pebbles
Ice fragments
Small debris
Medium:

Rocks
Metal shards
Ship fragments
Asteroids
Large:

Asteroid chunks
Small planets
Star fragments
Quantum anomalies
Hazards:

Mines
Gravity anchors
Magnetic fields
Collapsing debris
Massive objects
5.8 Level goals
Examples:

text

Reach target mass.
Survive time.
Maintain combo.
Avoid hazards.
Absorb anomalies.
Use efficiency challenge.
5.9 Upgrade systems
Use two systems:

Permanent meta-progression.
Temporary run upgrades.
Permanent upgrades:

Gravity Lens
Accretion Core
Horizon Stabilizer
Inertial Dampener
Combo Anchor
Quantum Core
Star Core
Temporary run upgrades:

text

+15% pull strength
+10% absorption radius
+10% movement speed
+10% mass efficiency
+1 quantum anomaly spawn
5.10 Progression curve
Use diminishing returns:

text

mass gain is direct
event horizon grows with sqrt(mass)
pull grows with mass^0.75
speed decreases slightly as mass grows
Example target masses:

text

Level 1: 1,000
Level 5: 10,000
Level 10: 50,000
Level 20: 250,000
Level 40: 1,000,000
5.11 Failure conditions
Possible:

Time limit.
Stability reaches zero.
Too many hazard collisions.
Mass drops below threshold.
Recommended:

text

Time limit + stability + hazard pressure
5.12 Campaign worlds
World 1: Dust Belt

Teaches movement, attraction, absorption.
Dust, pebbles, small rocks.
No hazards or very few.
World 2: Asteroid Field

Teaches larger objects and clustering.
Rocks, asteroids, metal shards.
World 3: Ship Graveyard

Teaches high-value targets and path planning.
Ship fragments, mines, magnetic fields.
World 4: Planet Ring

Teaches large mass and stability management.
Planet fragments, gravity anchors, heavy objects.
World 5: Quantum Nebula

Teaches quantum mode, phase shifts, chain reactions.
Quantum anomalies, phase particles, unstable cores.
5.13 Endgame modes
Campaign.
Endless mode.
Daily challenge.
Weekly challenge.
Leaderboards.
5.14 Monetization
Recommended:

Free-to-play.
No pay-to-win.
Optional ads.
Optional IAP for cosmetics.
Optional ad removal.
Optional premium version.
Ads placement:

Game over.
Retry.
Between levels.
Optional rewarded ad.
Avoid:

Ads during gameplay.
Forced interstitials.
Aggressive ad walls.
5.15 Tech stack recommendation
Best overall:

text

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
Lightweight alternative:

text

Godot 4
+ GDScript
+ custom gravity simulation
+ spatial partitioning
+ object pooling
+ simple shaders
5.16 Performance plan
Targets:

text

60 fps on modern phones
30 fps fallback on weak devices
low memory
low draw calls
low particle count
Use:

Object pooling.
Spatial hashing.
Culling.
LOD.
Simple shaders.
Fixed timestep simulation.
5.17 QA plan
Input:

Drag works on small and large phones.
Joystick works.
Pause button is large enough.
UI fits safe areas.
Performance:

Stable memory.
No leaks.
Low draw calls.
Simple particles.
No shader spikes.
Balance:

Level 1 completable.
Later levels challenging but fair.
Quantum mode not too strong.
Star mode not too weak.
Endless mode does not stall.
Store:

Screenshots clear.
Trailer short.
Age rating correct.
Privacy policy exists.
IAP works.
Ads work.
Analytics works.
Crash reporting works.
5.18 Development roadmap
Phase 1: Prototype

One black hole.
One level.
Move, attract, absorb, grow.
Phase 2: Game feel

Particles.
Camera zoom.
Sound.
Mass meter.
Combo.
Upgrade feedback.
Phase 3: Balance

Level goals.
Time limits.
Stability.
Object values.
Upgrade choices.
Difficulty curve.
Phase 4: Content

30-60 levels.
Worlds.
Unlock system.
Meta upgrades.
Endless mode.
Phase 5: Mobile polish

UI scaling.
Settings.
Accessibility.
Localization.
Analytics.
Ads/IAP.
Store compliance.
QA.
6. First next actions for agents
The next artifacts to develop should be:

text

A-002 README.md
A-003 AGENTS.md
A-004 docs/spec.md
A-005 docs/architecture.md
A-006 docs/design.md
Suggested first agent order:

text

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
7. Recommended repository structure
text

event-horizon-game/
├── plan.md
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
8. Important design rule
Do not make the game a pure power fantasy.

More mass should create tension:

text

more mass = stronger pull
more mass = larger collision risk
more mass = slower precise movement
more mass = harder navigation
more mass = stability may decay
This gives players real decisions:

text

Grow fast and risk instability?
Stay smaller and move precisely?
Upgrade for efficiency or pull?
Absorb large objects or avoid hazards?
