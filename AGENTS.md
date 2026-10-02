# AGENTS.md

Repository rules for the AI agents (and humans) developing Event Horizon. `PLAN.md` remains the canonical source of truth for design decisions; this file is the operational manual for how work moves through the repo.

## 1. Agent roles

| Agent | Responsibilities | Artifacts |
| --- | --- | --- |
| coordinator | Owns repo workflow, dependency order, pull requests, merge decisions, status updates, and the run-state control file. Runs the whole plan and delegates each artifact to a role agent. | A-001, A-002, A-003 |
| planner | Writes specifications, design plans, and game system documents. | A-004, A-018 |
| architect | Defines technical architecture, modules, simulation loops, data structures, performance plans, and platform choices. | A-005, A-019 |
| designer | Defines game feel, visuals, UI layout, camera behavior, feedback, and accessibility. | A-006, A-010, A-011, A-012 |
| balance | Defines progression curves, mass growth, upgrade values, difficulty targets, and economy. | A-007 |
| level-designer | Defines objects, hazards, upgrades, modifiers, rewards, campaign worlds, and level goals. | A-008, A-009 |
| programmer | Creates code scaffolding, prototypes, and implementation notes. | none (implements A-019 after A-005 and A-013 are done) |
| qa | Defines testing strategies, QA checklists, simulation and balance tests, performance checks, and regression risks. | A-013, A-014 |
| release | Handles store compliance, privacy policy, analytics, monetization, and shipping steps. | A-015, A-016, A-017, A-020 |

Every role agent reads `PLAN.md` before writing and must keep every decision consistent with it and with approved upstream artifacts. Only the coordinator may change run state and merge decisions.

## 2. Branch naming

Every artifact is developed on its own feature branch created from `develop`:

```text
feature/<agent>-<artifact-id>
```

```sh
git checkout -b feature/coordinator-agents develop
```

Examples in use:

```text
feature/coordinator-plan
feature/coordinator-readme
feature/coordinator-agents
feature/planner-spec
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
```

If a needed branch is not in the list above, add it to the list in `PLAN.md` section 1.3 before creating it.

## 3. Git-flow rules

| Branch | Rule |
| --- | --- |
| `main` | Protected. Receives only finished work merged from `release/*` and `hotfix/*`. Never commit or push directly to `main`. |
| `develop` | Integration branch. All development work lives here. |
| `feature/*` | One per artifact, created from `develop`. Merged into `develop` when approved, then deleted. |
| `release/<version>` | Release preparation. Merged into `main` and back into `develop`. |
| `hotfix/<version>` | Urgent fixes against `main`, merged back into `develop`. |

Approved feature branches are merged into `develop` and deleted after merge. Never merge a feature branch straight into `main`.

## 4. Commit prefixes

```text
docs(plan): docs(readme): docs(agents): docs(spec): docs(architecture): docs(design):
docs(balance): docs(content): docs(levels): docs(ui): docs(input): docs(accessibility):
docs(test-plan): docs(qa): docs(store): docs(monetization): docs(privacy-policy):
docs(agent-rules): docs(prototype-scaffold): docs(ship): chore(repo):
```

One commit per artifact plus separate `chore(repo):` commits for `PROGRESS.md` state changes.

## 5. Pull request workflow

1. Read `PLAN.md`, then `PROGRESS.md`, and resume from the recorded state.
2. Select the first artifact whose status is `ready`.
3. Create the feature branch from `develop`.
4. Draft the artifact.
5. Update the artifact status in `PLAN.md`.
6. Open a pull request into `develop` using the template in `PLAN.md` section 1.5.
7. Wait for coordinator review.
8. Merge only when approved, then delete the branch.

The PR body must state the artifact ID, the artifact, the agent, its dependencies, a summary, open questions, the acceptance criteria met, and the verification checklist.

## 6. Status updates

Each artifact moves through these statuses in `PLAN.md`:

```text
proposed -> ready -> doing -> done
```

| Status | Meaning |
| --- | --- |
| proposed | The artifact is planned but its dependencies are not yet `done`. |
| ready | Dependencies are `done`; the artifact may be started. |
| doing | In progress on a feature branch. |
| done | Drafted, reviewed, and merged. |
| blocked | Not yet startable. Used in the sequence table for artifacts waiting on upstream artifacts. |

Rules:

- Never start an artifact whose dependencies are not `done`.
- Never change an artifact without updating `PLAN.md`.
- If a design decision conflicts with `PLAN.md`, update `PLAN.md` first.
- `PLAN.md` is the single source of truth; on disagreement it wins.
- Agent definitions in `PLAN.md` section 9.2 must name the same artifacts, dependencies, and consumers as the sequence table in section 2 and the dependency graph in section 3. When they disagree, fix the agent definition, not the table.

## 7. Progress control file

`PROGRESS.md` at the repo root is run state only, never design. It records current artifact, last completed artifact, next ready artifact, branch, PR, status (`idle`, `running`, `interrupted`, `blocked`), and a dated log.

- The coordinator reads `PLAN.md` then `PROGRESS.md` at the start of every session and resumes exactly from the recorded state.
- The coordinator updates `PROGRESS.md` after every meaningful step, including partial ones: branch created, artifact drafted, status changed, PR opened, merge done.
- When interrupted or blocked, set the status and record the exact stopping point and reason.

## 8. Runtime configuration

The roles in section 1 are implemented as OpenCode agents configured per `PLAN.md` section 9: `default_agent` set to `coordinator`, the nine agents defined with `mode: "all"`, and `subagent` permission for the coordinator so it can launch role agents. Global permissions must allow `edit` and `shell`.

Role agents never read `PLAN.md` or an approved upstream artifact in full. Before launching a role agent, the coordinator writes a brief at `docs/briefs/<artifact-id>.md` containing the artifact contract, the PLAN.md section 8 design rule, only the PLAN.md excerpts that artifact depends on, the upstream open questions this artifact must answer, and the scope boundaries later artifacts own. The agent prompt names only that brief. Briefs are distilled context: on disagreement `PLAN.md` wins. Keeping briefs short is what keeps role-agent sessions far below the model context limit.

```sh
opencode service restart
opencode run "start development"
opencode run --agent planner "create docs/spec.md"
```

## 9. Hard rules

- Never push directly to `main`.
- Never merge a pull request without coordinator approval.
- Never start an artifact out of dependency order.
- Never let monetization block completion of the core game.
- Never write code that does not follow the approved architecture, design, and balance documents.
