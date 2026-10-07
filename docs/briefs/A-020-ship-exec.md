# Brief — ship phase execution of A-020 (release/0.20)

Artifact ID: A-020 execution (ship phase), not a new artifact ID.
Agent: release. Branch: `release/0.20` (created from `develop` at `b10e89f`).
This brief is the only context source. On disagreement, `PLAN.md` wins, then the approved upstream documents named below.

## 1. Deliverable — exactly one file

Author the **filled privacy policy page source** at:

    docs/policy-page/1.0.md

`1.0` is the policy-text version named by A-017 §6 ("Serves the §3 text, version 1.0, with the effective date filled"), not the product version. The path is a decision this step records; A-017 §6 leaves hosting to A-020 and A-020 §6 chose GitHub Pages, but neither names a source file. Record the path decision in the file's own header comment.

## 2. Hard rules (A-020 §6, verbatim)

| Item | Value / rule |
| --- | --- |
| Host | GitHub Pages for this repo. The policy page is the published A-017 §3 text, built from the repo's own docs, served over HTTPS. |
| Verbatim rule | The published page is A-017 §3 copied verbatim. The only permitted changes are the two bracketed values A-020 supplies. No paraphrase, no added marketing copy, no removed section. |
| Single URL record | `POLICY_URL = https://event-horizon-game.github.io/privacy-policy/`. The org/user segment is fixed at the moment of publishing and the final value is written into ship notes. Every consumer reads the recorded value; nothing re-derives or hand-copies it. |
| Reuse | POLICY_URL appears in: every store listing privacy field, the in-game privacy screen, the credits line, and ship notes. One string, four places (PR-2, PR-3). |
| Contact | `CONTACT_CHANNEL = email`; `CONTACT_VALUE = support@event-horizon.game`; `MONITORED = yes`, held by the human ship decision. Taken from A-017 §5. No placeholder, no bot-only channel, no unmonitored inbox. |
| Bracket fill | The §3 bracket receives the AdMob disclosure from §5. The contact bracket receives CONTACT_CHANNEL/CONTACT_VALUE. Both filled before the page is published, so the published page never contains a bracket. |
| No placeholders | A build, listing, or policy page containing `TODO`, `TBD`, `<...>`, or an example address fails PR-4 and blocks submission. |

## 3. Excerpts to use — read only these line ranges

- `docs/privacy-policy.md` lines **74-168** = A-017 §3, the publishable text. Copy it **verbatim** into the deliverable; this is the only body content allowed.
- `docs/privacy-policy.md` lines **190-214** = A-017 §5 contact rules (fills the contact bracket).
- `docs/ship.md` lines **139-151** = A-020 §5, the AdMob SDK choice and its disclosure text (fills the third-party bracket).
- `docs/privacy-policy.md` lines **221-233** = A-017 §6 hosting requirements the page must satisfy.

Do not read these documents in full. Do not read `PLAN.md` or `PROGRESS.md`.

## 4. What is NOT in scope — do not produce these

- **Ship notes** (`docs/ship-notes/<version>.md`). A-020 §9 creates them during **STEP-13**, and their items require a submission date, screenshots of both store privacy flows, and one line of evidence per §4 row. None of those exist yet, so writing them now would fabricate evidence. Leave them absent; the coordinator has already recorded why.
- **Any store submission, listing copy, or gate marked green.** STEP-03..STEP-15 need a Unity player package, device classes, store consoles, and live policy hosting. None exist in this environment. Never report a gate as green that was not actually run.
- **Publishing.** STEP-09 (publish the page, fix the final `POLICY_URL`) is an external action the human performs; here you author the source only. Say so in the file header.

## 5. Upstream open questions this step must answer

From A-017 §9 (answer in the file header, one line each):
- item 2 (hosting execution): host chosen = GitHub Pages per A-020 §6; source file path = `docs/policy-page/1.0.md`; publishing deferred to STEP-09.
- item 3 (contact execution): `CONTACT_CHANNEL`/`CONTACT_VALUE` filled from A-017 §5, `MONITORED = yes` held by the human ship decision.

From A-020 §10:
- item 3: the final `POLICY_URL` org segment and the live monitored inbox are supplied by the human; A-020 supplies the values. Record which values are fixed now and which await the human.

## 6. Acceptance criteria

- [ ] Body is byte-identical to A-017 §3 except the two brackets, which are filled (the coordinator will `diff` the body against `docs/privacy-policy.md` lines 74-168 — do not paraphrase).
- [ ] No `TODO`, `TBD`, `<...>`, or example address anywhere in the file (PR-4).
- [ ] Both brackets filled: third-party bracket carries the A-020 §5 AdMob disclosure; contact bracket carries `email` / `support@event-horizon.game`.
- [ ] Effective date and policy version 1.0 present as A-017 §6 requires.
- [ ] Header records: path decision, that publishing is STEP-09 and not done, `POLICY_URL` as recorded (not re-derived), and which values await the human.
- [ ] No ship notes, no store copy, no gate claimed green.

## 7. Scope boundaries owned by later steps

STEP-09 publishes and fixes the final URL; STEP-10 transcribes §7 tables into App Privacy / Data Safety with screenshots; STEP-11 verifies ship-build exclusions; STEP-13 writes ship notes; STEP-14 is the human ship decision; STEP-16..STEP-18 are the merges. Do not anticipate them.
