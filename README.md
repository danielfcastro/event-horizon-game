# Event Horizon

A mobile game (iOS, Android) where the player controls a growing black hole. It starts small, drifts across the screen, pulls nearby bodies into its gravitational field, absorbs them, and grows in mass, event horizon, and pull strength.

This repository holds the **design and coordination plan** for that game, not the game code. Everything is written so that AI agents can pick up work in a predictable order without duplicating effort or making incompatible decisions.

## What is in this repository

| File | Role |
| --- | --- |
| `PLAN.md` | Canonical source of truth: game concept, agent roles, artifact order, workflow rules, git-flow requirement. |
| `PROGRESS.md` | Run-state control file. Records the current artifact, what is done, what is next, and a dated log so an interrupted run can resume. |
| `README.md` | This overview. |
| `docs/` | Design and specification artifacts (`spec.md`, `architecture.md`, `design.md`, …), produced in the order defined in `PLAN.md`. |

`PLAN.md` wins on any disagreement. `PROGRESS.md` is run state only and carries no design decisions.

## Artifact sequence

Work proceeds through 20 artifacts, `A-001` through `A-020`, in the dependency order given in `PLAN.md`. Each artifact is one file or module, owned by one agent role, and moves through four statuses:

```text
proposed -> ready -> doing -> done
```

An artifact must not be started before its dependencies are `done`. The current status of every artifact is in the sequence table in `PLAN.md`; the current position of the run is in `PROGRESS.md`.

## Agent roles

`coordinator`, `planner`, `architect`, `designer`, `balance`, `level-designer`, `programmer`, `qa`, `release`. The coordinator owns repo workflow, dependency order, pull requests, merge decisions, and status updates; the others produce their assigned artifacts. Full responsibilities are defined in `PLAN.md` and `AGENTS.md`.

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
