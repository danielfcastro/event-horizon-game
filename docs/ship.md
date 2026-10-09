# Ship — A-020 docs/ship.md

Agent: release. Status: doing. Depends on: A-015 docs/store.md (done), A-016 docs/monetization.md (done), A-017 docs/privacy-policy.md (done). Consumed by: the coordinator (submission execution) and the human ship decision.

## 1. Purpose, scope, inherited contract

A-020 is the shipping checklist: the final release steps for Event Horizon. It decides version numbering, the release-branch procedure for this repo, one ordered submission checklist that aggregates every inherited gate, the ad SDK choice and its disclosure consequences, policy hosting and contact execution, crash-reporting configuration, ship-build exclusions, ship notes, and the hotfix policy.

A-020 does not redefine any gate's readiness condition, renumber any upstream ID, weaken any gate, decide design or content, or let any monetization surface gate core completion. Gates are consumed as-is and may only be extended.

New ID namespace declared by this document: `STEP-01..STEP-nn` (ship execution steps). No other namespace is introduced. Namespaces already claimed and not reused here: A-013 S/B/U/I/A/P/R; A-014 M-; A-015 SH-, SG-1..SG-7; A-016 SG-6.1..SG-6.6; A-017 PR-1..PR-10; A-019 H-.

### 1.1 Inherited gate table (conditions are owned upstream; this document only orders them)

| Gate | Owner | Readiness condition (upstream, quoted in intent) | What A-020 executes |
| --- | --- | --- | --- |
| A-014 release gate | qa | merge-gate stages green on the ship build: determinism core first, no-softening second, SL-1, canonical numbers, tiers green, R re-runs; P-01/P-02 on named device classes; P-04/P-05 memory flatness | run the stages against the ship build, record evidence in ship notes |
| SG-1..SG-6 | release (A-015) | store listing readiness conditions (A-015 §9 prerequisite list) | transcribe listing fields, submit listing only when all are green |
| SG-7 | release (A-015) | crash reporting configured (readiness is A-015's) | configure the reporter, then mark SG-7 green (§8) |
| SG-6.1..SG-6.6 | release (A-016) | monetization gates, ship-blocking only, never core completion | verify each on the ship build before submission (§4, §5) |
| PR-1..PR-10 | release (A-017) | privacy gate extending SG-5; submission-blocking only | publish policy, fill brackets, transcribe store forms, attach screenshots (§4, §6) |

Hard rule honored throughout: monetization blocks ship only, never core completion. The game must be completable with zero purchases (A-015 §9 item 7).

## 2. Version numbering

Two tracks, decided once and never renumbered afterwards.

| Track | Numbering | Meaning |
| --- | --- | --- |
| Documentation milestones (this repo today) | `0.<artifact-id>` — e.g. `0.15`, `0.16`, `0.17`, `0.20` | A `release/0.x` branch carries approved documents only. No player-facing build exists, so no store action is taken. |
| First store build | `1.0.0` | The first build that can pass A-014's release gate and A-015's SG-1..SG-7. Only this number may ever appear in a store listing. |
| Post-release builds | `1.0.1`, `1.0.2`, … semver patch bumps | Hotfix and content-fix releases against `main`. |

Rules:

- `0.x` numbers are never used in a store field, a privacy policy footer, or a build label. Anything that says `0.x` is documentation, not a product.
- A-020 is drafted under `0.20`. The first store build is `1.0.0`; the version string is stamped into the build manifest, the in-game credits line, and the ship notes, and all three must agree.
- The version recorded in ship notes is the single source of truth for what was submitted. If a listing is resubmitted, the version bumps; the listing URL and the policy URL do not change.

### 2.1 What a `release/<version>` branch carries today vs after code exists

| Phase | Branch carries | Merges |
| --- | --- | --- |
| Documentation phase (now) | the artifact documents being finalized: `docs/store.md`, `docs/monetization.md`, `docs/privacy-policy.md`, `docs/ship.md`; the filled policy page source; the ship notes file | into `main`, then back into `develop`. No store submission happens in a `0.x` branch. |
| Shipped build (after code exists) | the build manifest, the built player package, the store listing copy for all seven locales, the published policy page, the ship notes, and the gate-evidence log | into `main`, then back into `develop`, only after the ordered checklist in §4 is green through the submission steps. |

The branch name is `release/<version>` with the version from the table above, created from `develop`, never from `main` and never from a `feature/*` branch.

## 3. Release procedure

### 3.1 Ordered release procedure (documentation phase and shipped build share the same shape)

```text
STEP-01  Confirm the artifact dependency chain is done: A-015, A-016, A-017.
STEP-02  Create release/<version> from develop:  git checkout -b release/<version> develop
STEP-03  Run the A-014 merge-gate stages on the ship build in upstream order
         (determinism core, no-softening, SL-1, canonical numbers, tiers, R re-runs).
STEP-04  Run P-01/P-02 on the named device classes; run P-04/P-05 memory flatness.
STEP-05  Run SG-1..SG-4 (A-015 listing readiness).
STEP-06  Run SG-6.1..SG-6.6 (A-016 monetization gates). Ship-blocking only.
STEP-07  Run PR-1..PR-10 (A-017 privacy gate).
STEP-08  Execute SG-7: configure crash reporting (§8). Mark green only after config is on.
STEP-09  Publish the policy page (§6). Record POLICY_URL. This precedes any listing submission.
STEP-10  Transcribe A-017 §7 tables into App Privacy / Data Safety; screenshot both (§6, PR-5).
STEP-11  Verify ship-build exclusions (§7): harness, replays, dev.* knobs absent.
STEP-12  Verify zero-purchase completion and that no monetization surface gates it.
STEP-13  Write ship notes (§9) with the version, POLICY_URL, screenshots, and gate evidence.
STEP-14  Human review of seven-locale copy (A-015 §9 item 6) and the human ship decision.
STEP-15  Submit listings (§4 order). Only after SG-1..SG-7 are green.
STEP-16  Merge release/<version> into main (protected branch, coordinator approval required).
STEP-17  Merge release/<version> back into develop.
STEP-18  Delete the release branch after merge.
```

`STEP-01..STEP-18` is the fresh namespace declared in §1.1. Steps are execution; the gates they consult keep their upstream IDs and conditions.

### 3.2 Merge mechanics

```sh
git checkout main
git merge release/1.0.0        # coordinator-approved; never a direct push to main
git checkout develop
git merge release/1.0.0        # bring the release back so develop carries the shipped state
git branch -d release/1.0.0
```

### 3.3 Hotfix policy for `main`

- Urgent fixes against `main` use `hotfix/<version>`, branched from `main`, merged into `main`, then back into `develop`. Never a direct push to `main`.
- Documentation phase: a `hotfix/0.x` branch may correct factual errors in published documents only (a wrong URL, a wrong contact value, a transcription slip). It may not weaken, rename, or renumber a gate, and it may not change a design decision.
- Shipped build: a `hotfix/1.0.n` branch may fix a crash, a listing transcription error, a policy-page rendering fault, or an analytics/crash-reporting misconfiguration. Any fix that changes player-facing behavior re-opens the A-014 no-softening stage: it must be re-run and green before the hotfix merges. A hotfix may never introduce a dev knob, a time-scale control, or any affordance that softens the mass tension.
- A hotfix that changes what the player sees or collects requires re-running the affected PR gates and re-screenshotting the store forms before the hotfix is merged.
- No hotfix may be merged without coordinator approval.

## 4. Ordered submission checklist

Execution order. Each row names the gate owner (whose readiness condition it keeps) and what A-020 executes. Nothing here restates or relaxes a condition; the gate documents win on any wording difference.

| # | Gate | Owner | Executed by A-020 | Blocking |
| --- | --- | --- | --- | --- |
| 1 | A-014 release gate (all merge-gate stages) | qa | run stages in upstream order on the ship build; paste the stage log into ship notes | ship |
| 2 | A-014 P-01/P-02, P-04/P-05 | qa | run on the named device classes; record device names and results | ship |
| 3 | SG-1..SG-4 | A-015 | fill listing fields for all seven locales; capture the draft listing URL | ship |
| 4 | A-016 disclosure wording | A-016 | carry the exact disclosure wording verbatim into the listing; diff against A-016 §6 disclosure strings, zero tolerance | ship |
| 5 | SG-6.1..SG-6.6 | A-016 | verify each monetization gate on the ship build; record pass evidence per gate | ship only |
| 6 | Zero-purchase completion (A-015 §9 item 7) | A-015 | playtest a full completion with no purchase; record the run | ship |
| 7 | PR-1..PR-4 | A-017 | confirm inventory, contact channel/value, no placeholders | ship |
| 8 | PR-6 (collection beyond A-016 §2 schema) | A-017 | run A-017 §7.1 for the chosen SDK; fill the §3 bracket; see §5 | ship |
| 9 | PR-8 (new player-facing surface) | A-017 | if any surface is added, correct A-017 §4 and the A-015 descriptor row before continuing | ship |
| 10 | PR-2/PR-3 (single URL) | A-017 | publish the policy page; record POLICY_URL; reuse it in every listing field and the in-game privacy screen | ship |
| 11 | SG-5 / PR-1..PR-10 green | A-015 + A-017 | confirm the whole privacy gate green before either store submission | ship |
| 12 | PR-5 (form transcription) | A-017 | enter A-017 §7 tables verbatim into App Privacy and Data Safety; attach screenshots of both to ship notes | ship |
| 13 | SG-7 (crash reporting configured) | A-015 | configure the reporter per §8; SG-7 turns green only after the config is on | ship |
| 14 | Ship-build exclusions | A-019 | run the two verification steps in §7 | ship |
| 15 | Seven-locale copy review | A-015 | human review of all seven locales; record reviewer and date in ship notes | ship |
| 16 | Submission itself | A-015 | submit the listing per store, in the order in §4.1 below | ship |

Ordering rules that this document enforces:

- Policy page published before listing submitted, never after (A-017 §4 question 4). Rows 10 precede row 16.
- SG-6.1..SG-6.6 must pass before submission, and they never gate core completion (A-016 question 2).
- SG-7 is green only after crash reporting is configured; A-015 owns that condition, A-020 owns the switch.
- Do not submit until SG-1..SG-7 are green (A-015 Q6). A-015 owns readiness; A-020 owns mechanics.

### 4.1 Submission mechanics per store

```text
1. Create the developer account / console project for the version.
2. Complete the store's own compliance flows (age ratings, data-safety, privacy disclosures).
3. Attach the screenshots from row 12 to the compliance flows.
4. Enter POLICY_URL in every privacy field of the listing.
5. Enter the disclosure wording from row 4 verbatim in the listing description.
6. Upload the ship build; confirm the version string matches §2.
7. Set the release to draft, run the store's own review, then publish.
8. Record the public listing URL in ship notes.
```

## 5. Ad SDK choice

**Chosen SDK: Google Mobile Ads SDK (AdMob) for the native ship build.** If the eventual ship target is browser-only, the equivalent Google AdSense product is used and every disclosure consequence below applies unchanged. No other ad network, no mediation layer, no second SDK.

| Concern | Decision |
| --- | --- |
| Placement | Ads render only at the A-016 slots AD-1..AD-4. No ad may appear during a run, inside a level, on a pause screen, or as a reward gate. Verification: enumerate every ad call site in the build; the set must equal {AD-1, AD-2, AD-3, AD-4}. This is the SG-6.3 condition, owned by A-016. |
| Collection vs A-016 §2 schema | AdMob collects **more** than A-016's §2 schema: advertising cookies, device/app request data, IP address, and coarse-grained location. |
| Consequence | PR-6 fires. Before submission: run A-017 §7.1 for the SDK, fill the §3 bracket with the resulting disclosure, and re-check PR-1..PR-10. The policy text is copied verbatim from A-017 §3 with only its two bracketed values filled by A-020. |
| New player-facing surface | None added by the SDK beyond AD-1..AD-4. If any opt-out, consent banner, or settings surface appears, PR-8 requires correcting A-017 §4 and the A-015 descriptor row first. |
| Premium version | **Absent** for `1.0.0`. No premium SKU is created. Opting in later requires A-016 §4.3: same build, ads off, identical digests. |
| Monetization vs completion | Ads are an offer surface only. Removing every ad must not change any run, any reward, or any completion path (row 6 of §4). |

## 6. Policy execution

| Item | Value / rule |
| --- | --- |
| Host | GitHub Pages for this repo. The policy page is the published A-017 §3 text, built from the repo's own docs, served over HTTPS. |
| Verbatim rule | The published page is A-017 §3 copied verbatim. The only permitted changes are the two bracketed values A-020 supplies. No paraphrase, no added marketing copy, no removed section. |
| Single URL record | `POLICY_URL = https://danielfcastro.github.io/privacy-policy/`. The org/user segment is fixed at the moment of publishing and the final value is written into ship notes. Every consumer reads the recorded value; nothing re-derives or hand-copies it. Corrected 2026-10-09 by A-024: the pre-named event-horizon-game.github.io segment is squatted by an unrelated GitHub org (probed, not assumed); the account created is danielfcastro. See docs/policy-hosting.md. |
| Reuse | POLICY_URL appears in: every store listing privacy field, the in-game privacy screen, the credits line, and ship notes. One string, four places (PR-2, PR-3). |
| Contact | `CONTACT_CHANNEL = email`; `CONTACT_VALUE = support@event-horizon.game`; `MONITORED = yes`, held by the human ship decision. Taken from A-017 §5. No placeholder, no bot-only channel, no unmonitored inbox. |
| Contact verification | Before row 7 of §4 turns green: send one test message to CONTACT_VALUE and confirm a human reply within 7 days; record the date in ship notes. |
| Bracket fill | The §3 bracket receives the AdMob disclosure from §5. The contact bracket receives CONTACT_CHANNEL/CONTACT_VALUE. Both filled before the page is published, so the published page never contains a bracket. |
| No placeholders | A build, listing, or policy page containing `TODO`, `TBD`, `<...>`, or an example address fails PR-4 and blocks submission. |

## 7. Ship-build exclusions

Answers A-019 questions 5 and 6. Each has a verification step that must be run on the actual shipped package, not on the source tree.

| Exclusion | Rule | Verification step |
| --- | --- | --- |
| `tools/harness` | Absent from shipped player and store packages. Harness code is a development surface; shipping it is a dev knob. | STEP-11a: list the contents of the built package and assert no path matches `tools/harness`, `harness`, or `golden`. A match blocks submission. |
| Harness binary | Absent. The harness binary must not be bundled, embedded, or reachable by a URL inside the package. | STEP-11b: grep the built package for the harness entrypoint name and for any harness CLI flag; zero hits required. |
| `replays/` goldens | Absent. Golden replays are test fixtures; shipping them would let a player reach reference solutions. | STEP-11c: assert no `replays/` directory and no golden replay digest appears in the package manifest. |
| `dev.*` knobs (`dev.targetFps`, `dev.timeScale`) | Not reachable in a store build. A reachable time-scale knob is an affordance that could soften the mass tension, so it is a hard ship blocker. | STEP-11d: enumerate every input handler and settings entry in the built package; assert none reads a `dev.*` key. Then run the A-014 no-softening stage again on the built package and require green. |
| Debug power state | Not shippable in any state. No build flag may enable one at runtime. | STEP-11e: build the package with the release flag only; assert the debug symbol/flag set is empty in the shipped artifact. |

If any step fails, the release branch does not merge and the checklist stops at row 14.

## 8. Crash reporting configuration (SG-7 execution)

SG-7's readiness condition belongs to A-015; A-020 executes the configuration.

| Item | Configuration |
| --- | --- |
| Reporter | First-party only. The build's crash handler (native crash handler / web error handler) posts to the same analytics endpoint A-016 already uses. No third-party crash SDK, no Sentry-style processor, no consent screen. |
| Analytics event | Feeds exactly A-016's existing `crash` analytics event. No new event name is introduced. |
| Payload | Version string, build label, platform class, stack frame identifiers, and the session's own run metadata already inside A-016's §2 schema. Nothing else. |
| Data categories | No new data categories beyond A-017's inventory. If the payload would need a category outside the inventory, the reporter is disabled and PR-1..PR-10 are re-run before submission. |
| Opt-out | None. There is no player-facing crash-report control, so PR-8 stays not-fired. |
| Off-state test | With the reporter disabled, the game must still complete; crash reporting is never a completion dependency. |
| Turning SG-7 green | After the handler is wired and one synthetic crash produces one recorded `crash` event with the correct version string. Evidence goes to ship notes. |

## 9. Ship notes

One file per shipped version, filed at `docs/ship-notes/<version>.md` (e.g. `docs/ship-notes/1.0.0.md`), created during STEP-13 and committed on the release branch.

Contents:

1. Version string and the date of submission.
2. `POLICY_URL` — the single recorded URL, once, as the canonical value.
3. `CONTACT_CHANNEL` / `CONTACT_VALUE` / `MONITORED` with the verification date.
4. Screenshots of both store privacy flows (App Privacy and Data Safety), attached, so A-014's store readiness table can be closed (PR-5).
5. Gate evidence, one line per row of §4: gate ID, owner, result, evidence pointer. IDs are cited, never renumbered.
6. The exclusion evidence from STEP-11a..STEP-11e.
7. The crash-reporting evidence from §8.
8. The public listing URLs per store, and the seven-locale reviewer and date.
9. Any deviation from this checklist, with the gate it touches and why the gate still holds.

Ship notes are the record the coordinator and the human ship decision read. They never restate a gate's condition; they record that it was run.

## 10. Open questions for downstream

Kept minimal; A-020 is the last document.

1. Coordinator: whether the human ship decision gates the merge into `main` for `1.0.0`, or whether a green §4 checklist plus coordinator approval is sufficient. A-020 assumes the latter and flags STEP-14 as the human checkpoint.
2. Coordinator: whether `docs/ship-notes/<version>.md` is treated as a repo artifact with its own branch or as a release-branch-only file. A-020 writes it on the release branch.
3. Human: the final `POLICY_URL` org segment and the live `CONTACT_VALUE` inbox must be created and monitored before row 7 turns green; A-020 supplies the values, the human supplies the account.




