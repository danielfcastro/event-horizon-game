# A-024 — Policy hosting (docs/policy-hosting.md)

Artifact A-024. Records the host, the published `POLICY_URL`, and the policy text
version actually published, and closes the hosting gate by fetching the live URL and
comparing its served body against the page source. This artifact documents hosting
that was already executed; it does not publish the page again and it does not change
the policy text.

Depends on A-017 `docs/privacy-policy.md` and A-020 `docs/ship.md` (both done).
Consumers: A-025 `docs/submission.md` and ship notes, A-015 listing fields, and the
in-game privacy screen. Branch `feature/release-policy-hosting`.

## 1. Recorded values — the single source of truth for POLICY_URL

| Field | Value |
| --- | --- |
| `POLICY_URL` | `https://danielfcastro.github.io/privacy-policy/` |
| Host | GitHub Pages — user `danielfcastro`, repository `privacy-policy`, HTTPS (`https_enforced: true`) |
| Policy text version published | 1.1 (policy TEXT version, not the product version) |
| Effective date on the live page | 2026-10-07 (filled; equals the policy text publication date) |
| Page source on disk | `docs/policy-page/1.1.md` (the only source; superseded `1.0.md` deleted) |
| Contact | `CONTACT_CHANNEL = email`; `CONTACT_VALUE = support@event-horizon.game` (from A-017 §5 / A-020 §6) |

Every consumer reads the `POLICY_URL` string above. Nothing re-derives or hand-copies
it. The org/user segment is fixed at the moment of publishing per A-020 §6, and the
value recorded here is the one that was actually created.

## 2. Why the recorded segment is `danielfcastro` (host segment probed, not assumed)

A-020 §6 pre-named `https://event-horizon-game.github.io/privacy-policy/`. That
segment is squatted by an unrelated GitHub org: `api.github.com/orgs/Event-Horizon-Game`
exists (created 2024-07-26, zero repositories), `gh api user/orgs` returns no org for
the authenticated account `danielfcastro`, and GitHub names are unique and case-insensitive.
The pre-named URL can therefore never be published by us. A-020 §6's own rule fixes the
org/user segment at the moment of publishing, so the canonical recorded value is the
`danielfcastro` user-pages URL in §1. The account and repository were created by the
user on the coordinator's turn, with approval; this artifact records what was actually
created. The stale pre-named value has been corrected in `docs/policy-page/1.1.md` and
in `docs/ship.md` §6 so no consumer can read a URL that cannot exist.

## 3. Reuse — one string, four places (PR-2, PR-3)

`POLICY_URL` appears verbatim, as one string, in exactly four places:

1. every store listing privacy field (owned by A-015),
2. the in-game privacy screen,
3. the credits line,
4. ship notes (owned by A-025).

There is no second URL, no custom domain, no CDN, no redirect chain. The listing has
exactly one privacy URL field and it points at this URL.

## 4. The page body

The published body is the body of `docs/policy-page/1.1.md` after the header comment,
copied verbatim: A-017 §3's fenced block with exactly three fills (the effective date
2026-10-07, the third-party AdMob disclosure from A-020 §5, and the developer contact).
No paraphrase, no added marketing copy, no removed section. The section heading, its
editorial intro, and the code fence are document scaffolding and are not on the page.

## 5. Verification evidence (gate closed by running, 2026-10-09)

The gate is closed only because the live URL was fetched and its served body compared
against the page source. A repo created and a Pages build scheduled is not evidence that
a page was published.

| Check | Command / method | Result |
| --- | --- | --- |
| Reachable, no redirect | `curl -s -o /dev/null -w '%{http_code} %{num_redirects} %{url_effective}' POLICY_URL` | `200 0 https://danielfcastro.github.io/privacy-policy/` — final URL identical to the requested URL |
| Served body vs source | fetch page, strip host scaffolding (script/style blocks and the theme h1 heading), strip tags, NFKC-normalize, collapse whitespace, drop ordered-list markers on the source side (kramdown renders them as an ordered list); diff against the body of `docs/policy-page/1.1.md` | **zero differences** — policy text byte-identical (both normalized strings 4061 chars) |
| Placeholders on served text | scan for the to-do marker, the to-be-decided marker, a bare angle-bracket token, and the stale pre-named URL string | none present |
| Version / date on served text | regex `Policy version:` and `Effective date:` | `Policy version: 1.1`, `Effective date: 2026-10-07` |

Host artifact, recorded not removed: the default GitHub Pages theme emits an h1 heading
reading "privacy-policy" above the policy. Front-matter `title:` and `_config.yml`
`site_title:` had no effect on it; a `_config.yml theme: null` attempt left the Pages
build at `building` for 23 minutes with no error and was reverted, so the default theme
is kept. The heading is host scaffolding, not policy text; every policy word matches the
source.

Pages-repo commits behind the published page: `ac7cc12` page source, `f091945` front-matter
title, `a2e6b4a` site_title attempt, `80d986f` theme:null attempt, `837cfde` revert of
theme:null. Pages build status after the final push: `built`.

## 6. What remains gated (honest, not claimed green)

- Contact verification (A-020 §6 / STEP-14, human): send one test message to
  `CONTACT_VALUE` and confirm a human reply within 7 days, recording the date in ship
  notes. Needs a real inbox and a human reply; not executable here.
- PR-4, PR-5, PR-10 at review time: reviewers re-check the live page between submissions.
  The page is live and correct now, but the review-time assertion belongs to review, not
  to this artifact.
- Everything A-025 owns: `docs/submission.md`, ship notes, and the store consoles (iOS/Android
  developer accounts) are not present here. A-025 must read the recorded `POLICY_URL` in §1.
- A-015 owns the listing privacy fields themselves; A-017 owns the policy text and its
  version. This artifact changed only the recorded URL value, not the policy text.
