# Event Horizon — Privacy Policy

## 1. Scope and summary

This policy covers the Event Horizon game and any web page used to host it. It
describes, in plain language, what the game collects, why it is collected, and
what players can do about it. Event Horizon is a single-player space game. It
collects a small amount of information about how players play so that the
developers can measure progress, find bugs, and balance difficulty. The game
does not collect personal information and does not sell player data.

The policy applies only to Event Horizon. It does not apply to games or apps
made by other companies that may be linked from our store listing.

### 1.1 Inherited contract (what this document must honor, not re-decide)

| Inherited from | What it fixes here |
| --- | --- |
| A-015 store listing | SG-5 gate "Privacy policy URL live and non-placeholder", owned by A-017. The listing reserves exactly ONE privacy URL field and will not ship a placeholder. Content descriptor row "Players interact / shares data: No". |
| A-016 monetization | The analytics schema in A-016 §5 is canonical: 8 events with typed properties, referenced here, never renumbered or re-invented. Collection rules (no names, no contact info, no precise location, no device identifiers beyond store-SDK requirements, no screenshots, no text input) are binding. |
| A-014 QA | Store readiness table and release gate. Monetization and privacy gates block ship only, never core game completion. |

No-weakening rule: A-017 may add readiness conditions on top of SG-5 and the A-014
gate, never loosen them. Every condition in this document is submission-blocking
only; nothing here can block completion of the core game.

### 1.2 Answers to the inherited open questions

| Q | Answer given here |
| --- | --- |
| A-015 Q4 — publish a real URL before SG-5 goes green | §6 defines the hosting requirement: one stable HTTPS URL, public, non-placeholder, wired into the single reserved listing field and the in-game privacy screen. Execution (which host) is A-020's. |
| A-015 Q5 — does any chat/sharing surface exist? | No. §4 inventories the only outbound player surface (score-only leaderboards) and confirms the A-015 "Players interact / shares data: No" descriptor row stands unchanged. |
| A-016 Q1 — declare exactly the A-016 schema | §2 declares exactly the 8 events and their properties, nothing more, and §3 fixes retention and opt-out to that scope. |
| A-016 Q2 — contact info and policy URL ownership | §5 (contact structure, concrete value supplied at ship by A-020) and §6 (URL hosting requirement). |
| A-016 Q3 — ad SDK collecting beyond the schema | §7 carries a separate-disclosure hook: any collection by the ship-time ad SDK outside the 8-event schema must be disclosed separately in both store forms and in a third-party section of the policy text. |

## 2. Data collection inventory

This is the complete inventory. It is a one-to-one restatement of the A-016 §5
event/property set: 8 events, the properties listed, nothing more. If the game
ever emits an event or property not in this table, the inventory, the store
forms, and the policy text must all be updated before submission.

| # | Event | Properties (A-016 §5, canonical) | What it identifies | Apple "App Privacy" data type | Play "Data Safety" category |
| --- | --- | --- | --- | --- | --- |
| 1 | run_start | world_id int, seed_hash string | Play session start | Game Progress | Game progress |
| 2 | run_end | world_id, seed_hash, outcome enum (complete/fail), score int, duration_ms int | Play session outcome | Game Progress | Game progress |
| 3 | level_complete | level_id int, score int, stars int 0-3 | Level result | Game Progress | Game progress |
| 4 | upgrade_purchase | upgrade_id string, cost int | In-game upgrade bought with earned currency | App Usage Data | Usage & activity |
| 5 | iap_purchase | sku_id string, price_band enum | Real in-app purchase | Purchase History | In-app purchases |
| 6 | ad_impression | placement enum AD-1..AD-4 | Where an ad was shown (no user identity) | Advertising Data | Ads |
| 7 | settings_change | option_id string, value string | A setting was changed | App Usage Data | Usage & activity |
| 8 | crash | build_id string, stage string | Crash location | Crash Logs | App crashes |

Notes on the inventory:

- `seed_hash`, `world_id`, `level_id` identify generated content, not players;
  `outcome`, `score`, `stars`, `cost`, `price_band`, `placement`, `option_id`,
  `value`, `build_id`, `stage` are game-state or configuration values.
- No event carries a name, handle, avatar, email, contact, precise location,
  screenshot, or player-entered text (A-016 rules). Events 1-4 and 7-8 are
  first-party analytics; event 5 is the store's purchase record; event 6 is
  the only event whose collector may be third-party (§7.1).
- Analytics never feeds ranking. Daily/weekly leaderboards are a separate game
  surface (A-009 modes) and are score-only per A-015 §2.5.

Retention and purpose (fixed scope for §3):

- Purpose: measure progress, find bugs, balance difficulty and economy.
- Retention: aggregate counters keyed by no player identifier; kept for at most
  12 months; not sold, not rented, not used to build a cross-app profile.
- No cross-app or cross-site linking of any collected value.

## 3. The privacy policy TEXT

The block below is the publishable document. It is plain language, it matches §2
exactly, and it is what the URL in §6 serves. A-020 copies it verbatim; only the
bracketed ship-time values are filled in.

```text
Event Horizon — Privacy Policy
Policy version: 1.1    Effective date: [date this version is first published]

We publish Event Horizon as a game. This page explains what the game records
about you, why we record it, and how you can turn it off.

WHAT WE RECORD
When you play, Event Horizon records a small set of facts about the game itself,
not about you. Each recorded fact is one of these eight kinds of event:

1. Run started — which world you started, and the seed used to generate it.
2. Run finished — the world, the seed, whether you completed or failed, your
   score, and how long the run took.
3. Level completed — the level number, your score, and how many stars you got.
4. Upgrade bought in the game — the upgrade's name and its in-game cost.
5. Item purchased — the product you bought and its price band. This is the
   store's own purchase record.
6. Advertisement shown — which of the four ad slots was used. No information
   about you is part of this record.
7. Setting changed — the name of the setting and the value you chose.
8. Crash — the build number and the stage where the game stopped working.

We record these as numbers, short labels, and yes/no values. We never record your
name, your handle, your photo or avatar, your email or any other contact detail,
your exact location, your device's unique identifier, screenshots of your game,
or anything you type. We do not record what you write because the game has no
chat, no messaging, and no free-text field that we read.

WHY WE RECORD IT
To measure how far players get, to spot bugs and crashes, and to tune difficulty
and cost so the game stays fair. That is the only purpose. We do not use this
information to sell you anything, to build a profile about you, or to link this
game to your other apps and browsing.

WHAT WE DO NOT DO
We do not sell your data. We do not rent or share your data with anyone for
identification. These records are kept as combined counts for at most 12 months,
and we do not keep a per-player history beyond that window. Deleting your
progress on your device removes what the game holds locally.

YOUR CHOICE
In the game's settings there is a switch named "Play statistics". It is on by
default so we can tune the game. Turning it off stops all recording listed above,
immediately, for as long as it stays off; turning it back on resumes recording.
Your choice is remembered on the device you play on, and your device or
operating system may also offer broader controls over app data.

ADVERTISING
Event Horizon shows a limited number of ads placed by our advertising provider.
Ads are chosen by slot, not by who you are. The provider may use its own cookies
or identifiers to serve and count ads. If it records anything beyond the eight
events listed above, we name it here: [third-party collection
disclosure, filled at ship if applicable]. The "Play statistics" switch controls
our own recording only; anything extra the provider records is disclosed
separately in the store forms and in the sentence above.

CHILDREN
This game is not directed at children under 13 and is not sold as a children's
app, so a separate children's privacy policy does not apply. If we ever publish
a children-directed release, we will re-audit this page and add the required
children's policy before that release.

DATA SAFETY SUMMARY
Collected: game progress (runs, levels, scores, stars), in-game purchase records,
in-app purchase records, ad slot usage, settings values, and crash reports.
Not collected: name, contact info, precise location, device identifiers beyond
what the app store requires, photos, audio, text input.

LEADERBOARDS
Daily and weekly leaderboards show only your score, the level or world it came
from, and the seed that generated it. They never show your name, your handle, or
any other identifier, and you cannot send messages, share replays, or write
anything to another player.

CONTACT
If you have a question about this policy, or want to report a problem, contact
the developer through: [developer contact channel, supplied at release].
```

### 3.1 Rules for the publishable text

- The text is final at ship once the two bracketed values are filled by A-020;
  no bracketed placeholder may remain on the live page (SG-5, §6).
- The text may not claim a data type absent from §2, and §2 may not gain a data
  type without a matching sentence here. The advertising paragraph is the A-016
  Q3 hook in policy form: present only if the ship-time ad SDK collects beyond
  the schema, and then the bracket must name that collection.

## 4. Leaderboard data scope

The leaderboard is the only surface that publishes anything about a player. An
entry carries exactly: score (int), stars (int 0-3), world_id / level_id (int),
seed_hash (string). Not present: name, handle, avatar, country, device
identifier, timestamp linked to a person, free text, replay data, or any
shareable object.

Consequences:

- Leaderboards are score-only, matching A-015 §2.5 and the A-009 modes, and
  analytics (§2) never feeds them: the two are separate surfaces (A-016 rule),
  so a leaderboard entry is not an analytics record and vice versa.
- A-015 descriptor row "Players interact / shares data: No" stands unchanged.
  There is no chat, no gifting, no replay sharing, no friend system, no
  player-authored text.
- Extension condition: if any future surface lets players send text, name
  themselves, attach a replay, or share an object with another player, that row
  becomes wrong, A-015 must correct it, and this policy must be re-audited
  before submission.

## 5. Contact info

The policy must give players a way to reach the developer. A-017 defines the
structure and the requirement; A-020 supplies the concrete value at ship. No
email, handle, or address is invented here.

| Field | Meaning | Owner |
| --- | --- | --- |
| CONTACT_CHANNEL | Kind of channel: email, store developer portal, or web form | A-020 at ship |
| CONTACT_VALUE | The concrete, reachable value shown on the live policy page and in the store listing contact fields | A-020 at ship |
| CONTACT_MONITORED | Must be true: someone reads it | A-020 at ship |
| CONTACT_PURPOSE | Privacy questions, removal requests, reporting a problem | fixed here |

Requirements:

- CONTACT_VALUE must be real and monitored. A placeholder ("TODO", "example.com",
  "dev@placeholder") fails SG-5 exactly as a placeholder URL does, and A-020 may
  not submit until it is filled; that is a submission condition, never a
  condition on playing the game.
- The same CONTACT_VALUE appears in the policy text (§3 CONTACT block), the App
  Store and Play developer contact fields, and the in-game privacy screen. If
  the channel is a web form rather than an email, the policy sentence reads
  "contact the developer through the form on the game's page" and the form URL
  must be stable per §6.

## 6. Hosting and URL

SG-5 is owned here: "Privacy policy URL live and non-placeholder". A-015 reserves
one privacy URL field in the listing and refuses to ship a placeholder. A-017
defines what a passing URL is; A-020 executes the hosting.

| Requirement | Why |
| --- | --- |
| HTTPS, public, no login, no paywall, no region gate, reachable at review time | Reviewers and players must be able to open it; a dead link blocks submission |
| One single URL used everywhere | The listing has exactly one privacy URL field; the in-game privacy screen and both store listings point at the same URL |
| Stable, not a redirect chain, not a search result, not a file path that changes on rename | Reviewers re-check between submissions; a moved URL silently breaks SG-5 |
| Serves the §3 text, version 1.1, with the effective date filled | The page must be the policy, not a stub or a repo README |
| No placeholder token anywhere on the page (URL, contact, third-party bracket) | Placeholder content is what SG-5 exists to catch |
| Page names the game and the data types in §2 terms | A reviewer can match the page to the Data Safety / App Privacy forms |

Hosting options are A-020's to choose (project site, store developer page, or a
static page in the repo's published site): the chosen host must satisfy the
table above, and the URL is recorded once in A-020's ship notes and reused in
every field.

Wiring (A-015 owns the fields, A-017 owns the value rule):

- App Store: "Privacy Policy" field = the URL.
- Play: "Privacy policy" link on the listing = the same URL.
- In-game: the settings/privacy screen shows the same URL as selectable text and
  names the "Play statistics" switch described in §3.

## 7. Store compliance checklist

One row per A-016 event category. The rows are what A-020 fills in; they must
match §2 with nothing added and nothing missing.

| Event(s) | Apple "App Privacy" data type | Apple app-functionality label | Play Data Safety data type | Play declared purpose(s) | Linked to user? |
| --- | --- | --- | --- | --- | --- |
| 1-3 run_start, run_end, level_complete | Game Progress | Used with the app — analytics | Game progress | "Monitor app usage or activity", "Analytics" | No |
| 4 upgrade_purchase | App Usage Data | Used with the app — analytics | Usage & activity | "Analytics" | No |
| 5 iap_purchase | Purchase History | Used with the app — purchase history | In-app purchases | "Recognise purchases in the app" | Store account only |
| 6 ad_impression | Advertising Data | Used with the app — advertising | Ads | "Serve advertising in the app" | SDK-controlled, see hook |
| 7 settings_change | App Usage Data | Used with the app — analytics | Usage & activity | "Monitor app usage or activity" | No |
| 8 crash | Crash Logs | Used with the app — crash logs | App crashes | "Debug the app" | No |

Fill steps (A-020 executes, A-017 owns the shape):

1. App Store: enter the five data types above (Game Progress, App Usage Data,
   Purchase History, Advertising Data, Crash Logs), each "Used with the app"
   with its functionality checkbox, and confirm none is marked as identifying
   the user.
2. Play: enter the five categories above (Game progress, Usage & activity,
   In-app purchases, Ads, App crashes), select the purposes per row, and mark
   rows 1-4 and 7-8 "Not linked to users".
3. Play: state that data is stored, protected, and deletable by the developer,
   and that the Data Safety summary text matches §3.
4. Both: the policy link is the single URL from §6; run the §7.1 hook if an ad
   SDK is enabled at ship.

### 7.1 Ad SDK separate-disclosure hook (A-016 Q3)

The ad SDK choice belongs to A-020. If that SDK collects anything outside the
8-event schema in §2 — device or advertising identifiers, coarse or precise
location, cookie or browser state, cross-app tracking, ad interaction data, or
its own crash and usage logs — then:

- App Store: add the SDK's own data types as separate rows, each labelled with
  the SDK's functionality, and add the SDK to the "Third-party services"
  disclosure.
- Play: add a separate Data Safety entry for the SDK, declare its purposes, and
  add the SDK to the developer-account third-party declaration.
- Policy: the bracketed third-party sentence in §3 must name that collection in
  plain language, and the DATA SAFETY SUMMARY block must list it.
- §2 does not grow: SDK collection is disclosed separately, never merged into
  the 8-event schema, so the A-016 schema stays canonical. If the SDK collects
  nothing beyond §2, the hook is a no-op and the §3 bracket stays empty.

## 8. Readiness conditions

These extend SG-5 ("Privacy policy URL live and non-placeholder") and never
weaken SG-5, the A-015 listing gates, or the A-014 store readiness gate: no
condition is removed. PR-1..PR-10 gate store submission only; they do not gate
the prototype, the build, or core game completion (A-014 and A-015 rules).

| ID | Condition (all must hold before store submission) |
| --- | --- |
| PR-1 | §3 policy text is final, version 1.1, no bracketed placeholder left |
| PR-2 | Hosted at one stable HTTPS URL per §6, live and reachable at review time |
| PR-3 | The URL is non-placeholder and is the only URL wired into the single listing privacy field (A-015) and the in-game privacy screen |
| PR-4 | CONTACT_VALUE in §5 is real and monitored; the same value appears in policy, listing, and in-game screen |
| PR-5 | App Store App Privacy rows and Play Data Safety rows in §7 are filled and match §2 exactly — 8 events, 5 Apple data types, 5 Play categories, nothing more |
| PR-6 | Ad SDK hook §7.1 resolved: either the SDK collects nothing beyond §2, or its collection is disclosed separately in both forms and in §3 |
| PR-7 | No collection beyond §2 exists in the shipped build (verified by A-014 QA: no names, no contact info, no precise location, no extra device identifiers, no screenshots, no text input) |
| PR-8 | Leaderboard scope in §4 unchanged: score-only, no chat or sharing surface, so the A-015 "Players interact / shares data: No" row stands |
| PR-9 | Game is not published as a children-directed app, or a children's policy has been added per §3 |
| PR-10 | Effective date on the live page is filled and is the publication date |

## 9. Open questions for A-020

1. Ad SDK choice: which SDK, and does it collect beyond §2? If yes, A-020 must
   run §7.1 and fill the §3 bracket before submission (PR-6). Any new player-
   facing surface (sharing, replay, naming) must be flagged: §4 and the A-015
   descriptor row must be corrected first (PR-8).
2. Hosting execution: pick the host, publish the §3 text, record the single URL
   in the ship notes, and reuse it in every listing field and the in-game
   screen (PR-2, PR-3).
3. Contact execution: supply CONTACT_CHANNEL and CONTACT_VALUE in §5, monitored,
   no placeholder (PR-4).
4. Submission order vs SG gates: SG-5 must be green before either store
   submission, so the policy page is published before the listing is submitted,
   not after (PR-1..PR-3, PR-10).
5. Form transcription: enter the §7 tables verbatim into the App Privacy and
   Data Safety flows and attach screenshots of both to the ship notes so A-014's
   store readiness table can be closed (PR-5).
