# Agent Rules

## 1. Purpose and scope

This document is artifact A-018. It expands `AGENTS.md` (A-003): `AGENTS.md` states the base rules, and this document adds the operational detail that a running session needs to follow them without guessing. Where both would speak, `AGENTS.md` is the base and this document layers on top of it. This document never contradicts, restates-then-changes, or overrides `AGENTS.md`.

Precedence, in order:

1. `PLAN.md` — canonical design truth. Every decision in it wins over everything else.
2. `AGENTS.md` — operational base. Wins over this document where both speak.
3. This document — operational detail. It is the most specific instruction available, and it is the last word only on operational mechanics that neither of the above settles.

Nothing in this document changes design. It contains no game systems, numbers, content, or balance decisions; those belong to the artifacts that own them. If anything here appears to conflict with `PLAN.md` or `AGENTS.md`, the conflict is a defect in this document: fix this document, not the upstream ones. If a genuine design conflict is found, `PLAN.md` is updated first, by the coordinator, before any artifact work continues.

Consumers: there is no downstream document artifact. Every agent session reads this file, together with its brief, before doing work.

## 2. Status lifecycle

Vocabulary (`PLAN.md` §1.2): `proposed -> ready -> doing -> done`, plus `blocked` for artifacts waiting on upstream artifacts in the sequence table.

Only the coordinator changes statuses. A role agent never edits a status anywhere; if it believes a status is wrong, it says so in its PR review thread and leaves the file alone.

Exact edits, all three in the same commit, when a status moves:

| Place | Edit |
| --- | --- |
| Sequence table row | the artifact's Status cell |
| §4 agent definition contract | the artifact's `Status:` line |
| Front matter | `next_artifact` when the moved artifact is the current one |

The sequence table in §2 is authoritative. If a §4 contract disagrees with it, the contract is the defect: fix the contract, never the table.

Unblocking: when every dependency of an artifact is `done`, its row moves from `proposed`/`blocked` to `ready`, and `next_artifact` points at it. Never mark an artifact `ready` while a dependency is still `doing`, and never start an artifact whose dependencies are not `done`.

The `doing` marker is committed at the moment the branch is created, before drafting, so an interrupted run is visible in `PLAN.md`. The `done` marker is committed only after review approval and merge, never before. A status change is a separate `chore(repo):` commit when it is not part of the artifact's own commit.

## 3. The artifact loop, step by step

Run this loop once per artifact, in sequence-table order. Steps 1-3 and 8-10 are coordinator work; steps 4-6 are role-agent work.

1. Read `PLAN.md`, then `PROGRESS.md`; resume at the recorded state.
2. Select the first artifact whose status is `ready`.
3. Create the branch from `develop`, then commit the brief and the `doing` marker:

   ```sh
   git checkout -b feature/<agent>-<artifact-id> develop
   git add docs/briefs/<artifact-id>.md PLAN.md
   git commit -m "docs(<artifact-slug>): open <artifact-id> with brief and doing marker"
   ```

   Then a `chore(repo):` commit updating `PROGRESS.md` (artifact, branch, status `running`).
4. Launch the role subagent. Its prompt names only `docs/briefs/<artifact-id>.md`. It writes its artifact file, makes one commit on the feature branch with the artifact's commit prefix, pushes, and opens one PR into `develop`. It does not touch `PLAN.md` or `PROGRESS.md`.
5. Review the PR against the acceptance criteria for that artifact in `PLAN.md`, not against the draft's own self-report.
6. Apply pre-merge fixups on the feature branch when cross-references are wrong (wrong test IDs, contradicting upstream numbers, reused ID namespaces), committing each with the artifact's own prefix, e.g. `docs(spec): align test IDs with A-013 families`. Do not open a second PR for fixups.
7. Record approval, then merge (see §4 for why approval is a comment):

   ```sh
   gh pr merge <n> --merge --delete-branch
   ```

8. Update `PLAN.md`: sequence table row, §4 contract, `next_artifact` all move to `done`/next in one commit.
9. Commit `PROGRESS.md` with `chore(repo):` recording the completion entry: artifact ID, branch, PR number, key decisions, and the next ready artifact. Update front matter, `Current state`, and the dated `Log`.
10. Delete the branch (the merge command above does it) and continue with the next `ready` artifact.

`PROGRESS.md` gets a `chore(repo):` commit after every meaningful step, including partial ones: branch created, artifact drafted, status changed, PR opened, merge done.

## 4. Pull request rules

One PR per artifact, base `develop` only. Never open a PR into `main`, and never merge a feature branch into `main`.

```sh
gh pr create --base develop --head feature/<agent>-<artifact-id> --title "..." --body-file <body>
```

The body follows the `PLAN.md` §1.5 template with these fields, in this order, all present:

- Artifact ID
- Artifact (path)
- Agent
- Dependencies (artifact IDs, with their status at opening time)
- Summary
- Open questions
- Acceptance criteria met
- Verification checklist

Approval mechanics: GitHub rejects approving the author's own PR, so coordinator approval is recorded as a PR comment stating the acceptance criteria checked and the decision, posted before merging. The comment is the approval; a `reviewed` event is not required and must not be faked by a second account. Merge only after that comment exists. Never merge an unapproved PR, and never merge before the fixups from §3 step 6 are committed.

After merge: delete the branch. If `--delete-branch` was omitted, delete it explicitly; never leave a merged feature branch around, since a stale branch makes the branch-existence check in §6 wrong.

## 5. Conflict resolution

Order of authority: `PLAN.md` wins over everything. `AGENTS.md` wins over this document. This document wins only on operational mechanics neither upstream settles.

- A design decision that conflicts with `PLAN.md` is resolved by updating `PLAN.md` first, by the coordinator, and only then continuing the artifact. Never edit an artifact to work around `PLAN.md`.
- A §4 agent definition that disagrees with the sequence table is a definition defect: fix the definition, never the table.
- Cross-reference errors found in review are fixed pre-merge on the feature branch, with the artifact's own commit prefix: wrong test IDs, numbers contradicting an approved upstream artifact, reused ID namespaces.

ID-namespace rule: an ID family created by an artifact is claimed by that artifact and must not be reused by any later artifact for a different purpose. Observed families: A-013 owns the test families `S`, `B`, `U`, `I`, `A`, `P`, `R`; A-014 owns `M-` and `SG-`; later artifacts extend `SG-` as `SG-x.y` under A-014's system and never redefine it. Before writing an artifact that emits IDs, the role agent checks its brief for the families already claimed and picks a namespace that is not one of them.

Fail filing: a failure found in one artifact is filed against the artifact that owns the thing it tests, not against the artifact that discovered it. A performance failure belongs to the QA/performance artifact, a balance failure to A-007, a content failure to A-008/A-009. Route it to the owner and record the route in the PR thread.

## 6. Avoiding duplicate work

Resume protocol: read `PLAN.md`, then `PROGRESS.md`, and resume exactly at the recorded state. `PROGRESS.md` is run state only, never design.

- Never redo an artifact whose status is `done`.
- Never skip a dependency, and never start an artifact whose dependencies are not `done`.
- One branch per artifact, named `feature/<agent>-<artifact-id>`. Check it exists before creating it:

  ```sh
  git branch --list "feature/*"
  ```

  If the branch already exists, resume on it; do not create a second branch for the same artifact. If a needed branch is missing from the list in `PLAN.md` §1.3, add it there first, then create it.

- Update `PROGRESS.md` after every meaningful step, including partial ones. When interrupted or blocked, set the status and record the exact stopping point and reason: artifact, branch, PR number if any, last completed action, what to do next.
- Before drafting, check whether the artifact file already exists on the branch or in `develop`. If it does, continue it rather than rewriting it from scratch.
- Two agents never work on the same artifact at the same time. The coordinator launches one role agent per artifact and waits for it.

## 7. Brief discipline

Before launching a role agent, the coordinator writes `docs/briefs/<artifact-id>.md` containing exactly:

1. the artifact contract (artifact, agent, dependencies, consumers, purpose),
2. the `PLAN.md` design rule for the artifact,
3. only the `PLAN.md` excerpts the artifact depends on,
4. the upstream open questions this artifact must answer,
5. the scope boundaries later artifacts own.

Size limit: briefs stay under ten thousand tokens. Keeping them short is what keeps role-agent sessions far below the model context limit; distill, do not paste whole sections.

The role agent's prompt names only that brief. Role agents never read `PLAN.md` or an approved upstream artifact in full. Briefs are distilled context: on disagreement, `PLAN.md` wins, and the role agent reports the disagreement rather than resolving it by editing upstream files.

The brief is committed on the artifact's branch as part of the opening commit (§3 step 3), so the context a role agent ran under is reviewable after the fact.

## 8. Failure-mode recovery

Observed failure mode: a role subagent intermittently completes with no output and no artifact. The mitigation that works is write-first:

1. The role agent creates its artifact file and writes it one section per write call, starting with section 1, before reading anything else or running any other command.
2. The document stays under about 320 lines. Long single-response generation is what triggers the silent death.
3. Reading context happens after the first section is on disk, not before it.

When a subagent returns nothing, the coordinator verifies before concluding anything:

```sh
git ls-files "docs/<artifact>*"        # file present?
git log --oneline -5 feature/<agent>-<artifact-id>
```

If the file is absent and no commit exists, the artifact did not happen: relaunch the role agent with the write-first instruction and the size cap restated in its prompt. Record the attempt in `PROGRESS.md` as a dated log entry with the reason.

Never mark an artifact `done` unless all three exist: the file, a commit containing it on the feature branch, and a merged, approved PR. A subagent reporting success is not evidence; the filesystem and the commit log are.

## 9. Hard rules — checklist every session must pass before committing

From `PLAN.md` §1.6 / `AGENTS.md` §9, unchanged:

- [ ] Nothing pushed directly to `main`.
- [ ] No PR merged without coordinator approval (the approval comment).
- [ ] No artifact started out of dependency order.
- [ ] Monetization never blocks completion of the core game.
- [ ] No code written outside the approved architecture, design, and balance documents.

Operational additions from this document:

- [ ] Working on the artifact's own `feature/<agent>-<artifact-id>` branch, created from `develop`.
- [ ] PR base is `develop`; the branch is deleted after merge.
- [ ] Artifact content is one commit with the artifact's `docs(<slug>):` prefix; `PROGRESS.md` changes are separate `chore(repo):` commits.
- [ ] `PLAN.md` and `PROGRESS.md` untouched by role agents.
- [ ] The role agent read only its brief.
- [ ] The document is under about 320 lines and was written section by section.
- [ ] No ID family reused from an earlier artifact.
- [ ] Exactly one PR opened for the artifact.

A session that fails any box above stops and reports the failure instead of committing.

## 10. Open questions for downstream operational work

These are left open here on purpose; the artifacts that own them answer them, and this document must not decide them.

For A-019 (prototype scaffold) and the programmer:

- How the harness runs the simulation loop headlessly for A-013's test families, and which of those families it exposes as commands.
- Where harness output is filed so a failure routes to the owning artifact per §5.
- Whether harness runs get their own branch type or ride on the artifact branch they test.

For A-020 (ship):

- How `release/<version>` is prepared, merged into `main`, and back into `develop`, and what the version numbering is.
- Which `hotfix/<version>` mechanics apply to a documentation-only repo versus a shipped build.
- Store-compliance, privacy, and analytics steps that must not gate the core game per the hard rule in §9.

For the coordinator:

- Whether a run that ends mid-loop leaves `PROGRESS.md` status `interrupted` or `blocked`, and which one a dependency failure takes.
