# Event Horizon — Store Listing

Artifact A-015. Owned by the release agent. Covers store listings, screenshots, trailer, age rating, copy, and localization. Monetization details (IAP, ads, analytics) belong to A-016; privacy policy to A-017; crash reporting and shipping steps to A-020.

## 1. Purpose, scope, and inherited contract

This document is the compliance-ready store-listing package for Event Horizon. It exists so that whoever publishes the game can copy these blocks directly into the target storefront without re-deciding anything.

What this document owns:
- Store copy: titles, subtitles, short and long descriptions, feature bullets, tagline.
- Screenshots and trailer shot list (what must be captured, not the capture tooling).
- Age-rating decision and the evidence that supports it.
- Localization plan: supported store locales and which listing fields are localized.
- Compliance checklist against storefront rules.

What this document does NOT own:
- IAP (in-app purchases), ad placement, analytics, and monetization economics — A-016.
- Privacy policy text and data-scope declarations — A-017.
- Crash reporting, build shipping, and submission mechanics — A-020.
- Game systems, content, UI, input, accessibility implementation — upstream artifacts.

### 1.1 Inherited contract: the A-014 release gate

A-014 exposes a release gate (§9): all merge-gate stages green on the ship build, plus P-01/P-02 on named device classes, P-04/P-05 memory flatness, and the store checklist fully owned and marked ready by the release artifacts. Monetization items block ship only, never core completion.

**Decision on A-014 open question 1 (gate as-is vs extended):** A-015 **extends** the gate, never weakens it. The A-014 gate stays intact in full; A-015 adds store-side gates on top:

| Gate | Condition | Owner |
| --- | --- | --- |
| SG-1 | Listing copy final and approved (this document) | A-015 |
| SG-2 | Screenshot set captured from the ship build, accessibility states shown | A-015 |
| SG-3 | Trailer rendered within the length target and shot list | A-015 |
| SG-4 | Age rating chosen, evidence captured (see §4) | A-015 |
| SG-5 | Privacy policy URL live and non-placeholder | A-017 |
| SG-6 | IAP/ads/analytics disclosed in listing and working | A-016 |
| SG-7 | Crash reporting configured | A-020 |

Ship is blocked only when A-014's gate plus SG-1..SG-7 are green. No SG gate may ever be interpreted as blocking completion of the core game; they gate submission only.

## 2. Listing

### 2.1 Identity

| Field | Value |
| --- | --- |
| Title | Event Horizon |
| Subtitle (Android, ≤120 chars) | A gravity-driven black hole through forty levels of space |
| Tagline | Mass grows. So does the cost. |
| Platforms | iOS (App Store), Android (Google Play) |

Tagline rule: it states the tension rule, not a promise of power. Copy must never read as "upgrade to win", "get big and crush everything", or any other power-fantasy framing. Every gain path in Event Horizon has a cost (speed and control pay costs); marketing that hides that cost misrepresents the game and is a compliance bug in the copy, not in the game.

### 2.2 Short description (~100 words, App Store promo text)

> You are a small black hole adrift in space. A gravitational field pulls nearby bodies toward you; anything that reaches you is absorbed. Absorb, and you grow — and your reach grows with you. But mass pays costs: heavier is slower, harder to steer, easier to crash. Survive the hazards, chase the stars, chase the horizon. Forty levels across five worlds, endless mode, and rotating daily and weekly runs. One-handed tilt controls by default, with reduced motion, high contrast, and colorblind-safe palettes built in. Free to play. Optional ads and optional cosmetic purchases are disclosed in the listing.

### 2.3 Long description (Play Store, ≤1500 chars)

**Event Horizon** — a gravity game about tradeoffs.

You steer a small black hole through space. Your gravitational field attracts nearby bodies; bodies that reach you are absorbed. Mass grows, and your reach grows with it. So does the cost: more mass means more momentum, less precise control, and a wider collision profile. There is no build that wins by being big. There is a cost to every gain.

- **Campaign:** forty levels across five worlds, each with its own hazard grammar and star targets.
- **Endless:** one run, one score, how far you go.
- **Daily and weekly:** the same seed for everyone, the same leaderboard.

Accessibility is a first-class feature, not a patch: one-handed tilt control is the default, reduced motion is available, high-contrast and colorblind-safe palettes ship in the settings screen.

Event Horizon is free to play and fully playable without any purchase. Ads are optional and appear only at game over, on retry, between levels, and in optional rewarded moments — never during gameplay. Cosmetic purchases change how your hole looks, never how it plays. No violence, no horror, no user-generated content, no gambling.

### 2.4 Feature bullets

- Gravity, not engines: a field attracts, mass absorbs, reach grows.
- Every gain has a cost — speed and control pay for size.
- 40-level campaign in five worlds, endless mode, daily and weekly runs.
- Free to play; no pay-to-win; purchases are cosmetic only.
- Ads optional, never during gameplay.
- One-handed tilt by default; reduced motion, high contrast, colorblind-safe palettes.
- No violence, no horror, no user-generated content, no gambling.

### 2.5 Content descriptors

| Descriptor | Applies | Basis |
| --- | --- | --- |
| Violence / gore | No | Absorption is abstract; no depiction of harm |
| Horror / fear | No | No horror theming, jump scares, or dread mechanics |
| User-generated content | No | No UGC surface exists |
| Simulated gambling | No | No loot boxes, spin wheels, or paid randomization |
| In-game purchases | Yes | Optional cosmetic IAP — disclosure owned by A-016 |
| Ads / interactive elements | Yes | Optional ads at game over, retry, between levels, optional rewarded — disclosure owned by A-016 |
| Players interact / shares data | No | No multiplayer chat or sharing; leaderboards are score-only (A-017 confirms data scope) |

## 3. Localization plan

**Decision:** at ship, store copy is localized to a defined set; the in-game text stays English-only. Rationale: store copy is short, low-risk, and storefront-visible to every user, while in-game localization is a content/UI concern owned upstream and not yet scoped by any approved artifact. Localizing the listing without localizing the game is honest as long as the copy never claims otherwise.

| Locale | Store fields localized | Status at ship |
| --- | --- | --- |
| en-US | all | source of truth |
| es-419 | all | required (primary audience) |
| pt-BR | all | required (primary audience) |
| de-DE | all | required |
| fr-FR | all | required |
| ja-JP | all | required |
| zh-Hans | all | required |
| everything else | English fallback | storefront default |

Rules:
- The tagline is reviewed by a human per locale; it is a tension statement, and a machine translation that turns it into a power promise is a compliance failure.
- Numbers in copy must match the game: "forty levels", "five worlds". Any locale that rounds or drops these is wrong.
- Monetization sentences in localized copy must mirror the disclosure wording A-016 finalizes; if A-016 changes wording, localized strings are re-derived, not hand-edited.
- Screenshot and trailer assets are locale-neutral (no baked-in English text in shots; see §5).

## 4. Age rating

### 4.1 Chosen rating per platform family

| Platform family | Rating | Why |
| --- | --- | --- |
| iOS (App Store) | **Everyone** | No violence, no horror, no UGC, no gambling. Interactive elements (ads, IAP) exist but Apple's band for them is not an age band; disclosure is carried by content descriptors and the A-016 disclosure line. |
| Android (Google Play) | **Everyone but contains interactive elements (IE)** | Play requires the IE descriptor for ads and in-app purchases. No UGC, no violence, no paid randomization. |

Consistency rule: the rating must stay consistent with the disclosure A-016 and A-017 finalize. If A-016 ever ships a randomized cosmetic purchase (loot-box shape), Play requires the "Paid random items" add-on and the iOS band may need to move to Teen — A-015 flags this as a hook, A-016 decides the SKU shape.

### 4.2 Evidence bundle for the rating claim

The rating is not asserted from memory. It is asserted from:
1. Content descriptors table (§2.5) — A-015.
2. **M-5 SL-1 screenshots from A-014** — see decision below.
3. A-016 monetization disclosure (ads/IAP presence and placement) — hook.
4. A-017 privacy/data scope (no chat, no sharing) — hook.

**Decision on A-014 open question 2:** Yes — M-5 SL-1 screenshots **are** part of age-rating evidence. They are the only QA-owned visual proof of the shipped build, and the rating claim is partly a visual claim ("nothing on screen depicts harm or fear"). They are **necessary but not sufficient**: the screenshots prove the visual content, the descriptor table proves the mechanics, and the A-016/A-017 hooks prove the interactive elements. A rating submitted without the M-5 shots is not backed evidence and fails SG-4.

Capture requirement: the rating evidence bundle must include the M-5 SL-1 set as A-014 defines it (one full level, fixed seed, 10 checkpoint screenshots), plus SL-1-style captures covering at least one shot per world and the game-over/retry screens, because those are the screens where ad placement will appear and where a rating reviewer looks for interactive-element context. A-014 owns running the sessions; A-015 states the coverage SG-4 requires.

## 5. Screenshots

### 5.1 Named shot list

| ID | Scene | Must show | Notes |
| --- | --- | --- | --- |
| SH-01 | Campaign world 1, opening minutes | black hole, gravitational field, nearby bodies | first slot on both storefronts |
| SH-02 | Campaign mid-level | field radius vs grown mass, several bodies in flight | reads as "reach grows" |
| SH-03 | Hazard close call | hazard, near-miss, control pressure | reads as "costs, not free power" |
| SH-04 | Star capture | star target, absorption moment | campaign objective legible |
| SH-05 | Campaign world 5 late | large hole, dense field, hazard mix | late-game, not a fake endgame |
| SH-06 | Endless mode | score, distance, one hazard family | mode exists |
| SH-07 | Daily/weekly screen | seed label, leaderboard entry | mode exists |
| SH-08 | Settings screen | one-handed tilt default, reduced motion, high contrast, colorblind-safe palette | accessibility-forward line is provable |
| SH-09 | Upgrade/choice screen | a gain **and its cost** side by side | anti-power-fantasy proof shot |
| SH-10 | Game over / retry | the actual ad slot position | interactive-elements context for rating |

SH-09 is mandatory: if no shot shows a cost, the listing reads as a power fantasy and fails SG-2.

### 5.2 Capture procedure

- Captured from the **ship build only**, never a dev build with placeholder art.
- Source sessions: the A-014 simulation/QA sessions, including **M-5 SL-1** (which doubles as rating evidence per §4.2). A-015 names the session IDs at capture time; A-014 owns running them.
- Resolution: minimum 1242×2208 (iOS portrait) and 1080×1080 + 1920×1080 (Play); one capture set, cropped per storefront.
- No baked-in English text, no watermarks, no device chrome — keeps the set locale-neutral (§3).
- Aspect truth: shots must come from the real play aspect, not a stretched mock.

## 6. Trailer

### 6.1 Length target

PLAN §5.17 says "Trailer short." **Decision: 35 seconds**, hard ceiling 45, no intro card longer than 2 s. Both storefronts autoplay the first preview; 35 s keeps the hook inside the first 8 s and keeps the whole trailer below the point where users skip.

### 6.2 Shot list

| ID | Seconds | Content |
| --- | --- | --- |
| T-01 | 0–4 | Title card over live gameplay, field attracting bodies |
| T-02 | 4–10 | World 1: absorb, mass grows, reach grows |
| T-03 | 10–16 | The cost beat: heavier hole, slower, a hazard hit — shown, not narrated away |
| T-04 | 16–22 | Star chase, one world per cut across two worlds |
| T-05 | 22–27 | Endless + daily/weekly cards, real UI |
| T-06 | 27–32 | Accessibility beat: one-handed tilt, high contrast, colorblind palette on screen |
| T-07 | 32–35 | End card: title, "free to play", "no pay-to-win", "purchases are cosmetic" |

### 6.3 Audio and silence

- One ambient track, low dynamic range; no lyrics, no stingers.
- The trailer must be fully readable **with sound off**: every beat is carried by on-screen text or UI, because storefronts autoplay muted. A trailer that only works with audio fails SG-3.

### 6.4 No gameplay-misrepresentation rule

- Every clip is captured from the ship build at real simulation speed; no sped-up absorption, no removed hazards, no "assist" overlays.
- T-03 is mandatory. A trailer that shows growth without ever showing a cost advertises a power fantasy and violates the PLAN §8 tension rule.
- No text in the trailer may claim "upgrade to win", "unlimited power", or equivalent.

## 7. Compliance checklist

PLAN §5.17 checklist, made concrete. Each row names the artifact that owns the decision; A-015 only frames it.

| PLAN §5.17 item | How it is satisfied here | Owner | Status |
| --- | --- | --- | --- |
| Screenshots clear | SH-01..SH-10 shot list, ship-build capture, SH-09 cost shot mandatory | **A-015** | decided |
| Trailer short | 35 s target, ceiling 45 s, T-01..T-07 shot list | **A-015** | decided |
| Age rating correct | iOS Everyone / Play IE; evidence bundle incl. M-5 SL-1 | **A-015** | decided |
| Privacy policy exists | Listing carries one privacy URL placeholder; policy text and data scope are A-017's | A-017 | hook |
| IAP works | Listing discloses optional cosmetic IAP, "no pay-to-win"; SKUs/prices are A-016's | A-016 | hook |
| Ads work | Listing discloses optional ads, never during gameplay; placements are A-016's | A-016 | hook |
| Analytics works | Listing makes no analytics claim; schema is A-016's, data scope is A-017's | A-016 / A-017 | hook |
| Crash reporting works | No listing surface; A-020 configures it before submission | A-020 | hook |

Store-side requirements A-015 adds:

| Requirement | Where it goes on the storefront | Owner |
| --- | --- | --- |
| Age-rating disclosure matches the chosen band | Store metadata + in-game settings screen | A-015 (band), A-016 (interactive elements) |
| In-app purchase disclosure line | First three lines of the short description | A-015 wording, A-016 final wording |
| Ads disclosure line | Feature bullets + long description | A-015 wording, A-016 final wording |
| Privacy policy URL | Store metadata field | A-017 |
| "Free to play / no pay-to-win" line | Feature bullets + trailer end card | A-015 |
| No monetization gate on core completion | Copy states full playability without purchase | A-015, verified against A-016 |

## 8. Store readiness mapping

Restated from A-014's store readiness table (A-014 summary in the brief) with A-015's ownership decisions applied.

| A-014 store item | A-015 decision | Ready when |
| --- | --- | --- |
| Listing copy exists | §2 complete; tagline states the tension rule, not a power promise | SG-1 |
| Screenshots exist and are clear | §5 shot list SH-01..SH-10, ship build, SH-09 mandatory | SG-2 |
| Trailer exists and is short | §6, 35 s target, T-03 cost beat mandatory | SG-3 |
| Age rating decided and evidenced | §4, iOS Everyone / Play IE, M-5 SL-1 required | SG-4 |
| Privacy policy URL present | Placeholder only; live URL is A-017's | SG-5 |
| Monetization disclosed in listing | Disclosure lines present; final wording is A-016's | SG-6 |
| Crash reporting configured | Out of A-015 scope; A-020 | SG-7 |
| Release gate (§9) green on ship build | Consumed as-is, extended by SG-1..SG-7, never weakened | A-014 |
| Monetization items | Block ship only; never block core completion | A-016 |

## 9. Submission prerequisites

Before A-020 can ship the build, all of the following must be true. A-015 prepares this list; A-020 executes submission.

1. A-014 release gate green on the ship build (merge-gate stages, P-01/P-02 on named device classes, P-04/P-05 memory flatness) — unchanged by A-015.
2. SG-1..SG-4 green (owned by A-015, this document).
3. A-016 done: IAP SKUs, ad placements, analytics schema, and the exact disclosure wording the listing must carry.
4. A-017 done: privacy policy text live at the URL the listing references.
5. A-020 done: crash reporting configured, submission steps executed.
6. Localized copy for the seven locales in §3 reviewed by a human.
7. No monetization surface gates core completion: the game is completable with zero purchases, verified against A-016's final SKUs.

## 10. Open questions and conflicts

For A-016:
1. Final disclosure wording for ads and IAP — the listing carries placeholder wording; A-016's final strings replace it verbatim, and §3 localized strings are re-derived from it.
2. If any IAP SKU is randomized (loot-box shape), §4.1 rating bands must be revisited: Play "Paid random items" and possible iOS Teen. A-015 requires A-016 to answer explicitly.
3. Does A-016 add any store-side monetization gate beyond SG-6? A-015's gate may be extended, never weakened.

For A-017:
4. Privacy policy URL: the listing reserves one field. A-017 must publish a real URL before SG-5 can go green; A-015 will not ship a placeholder URL.
5. Leaderboards are described as score-only in §2.5. If A-017 finds any sharing or chat surface, the "Players interact" descriptor row is wrong and must be corrected here.

For A-020:
6. Submission order: A-020 must not submit until SG-1..SG-7 are green; A-020 owns the mechanics, A-015 owns the readiness conditions.

Upstream conflicts flagged: none. Nothing in this document contradicts the brief excerpts. The A-014 gate is extended (SG-1..SG-7), which is permitted; no A-014 condition was weakened or dropped.
