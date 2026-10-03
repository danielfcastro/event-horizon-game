# Event Horizon — Monetization

Artifact A-016. Owner: release agent. Depends on A-014 (docs/qa.md) and A-015 (docs/store.md).

This document defines the monetization model for Event Horizon: what may be sold, what may
never be sold, where ads may and may not appear, the disclosure strings shipped to players,
and the analytics schema that A-017 (privacy policy) must enumerate. It is the compliance
contract for store listings; A-020 (ship) blocks shipping until the readiness conditions in
Section 7 are met.

## 1. Purpose, scope, and inherited contract

A-016 defines the monetization model only. It does not own listing layout (A-015), privacy
text (A-017), or submission mechanics (A-020). Inherited, non-renumberable upstream facts:

| Source | Inherited fact | How A-016 uses it |
| --- | --- | --- |
| PLAN §5.14 | Free-to-play; no pay-to-win; optional ads; optional cosmetic IAP; optional ad removal; optional premium version. Ad placements and avoid list. | Sections 2–4 reproduce and honor these verbatim. |
| PLAN §1.6 hard rule | Monetization must not block core game completion. | Every monetization gate here is ship-blocking only, never completion-blocking. |
| PLAN §8 tension rule | Every gain path has a cost; no purchase may reduce drain tables, absorb gating, failure pressure, or pull behavior. Purchases change appearance only. | Section 2 fairness rules; Section 4 SKU contents. |
| A-012 / A-013 precedent | Options must leave the run digest byte-identical (machine-checked as A-10/A-11). | Extended in Section 2 to monetization: digests stay byte-identical with any SKU owned. |
| A-007 economy (canonical) | Permanent-upgrade total 2,195 cores obtainable by play only; economy supply 10,200; economy is play-only; no pay-to-win. | Referenced, never renumbered. IAP adds no stat-carrying currency. |
| A-015 SG-6 gate | "IAP/ads/analytics disclosed in listing and working", owner A-016. Gates block submission only. May be extended, never weakened. | Extended concretely in Section 7. |
| A-015 disclosure placeholders | Placeholder wording and feature bullets in the listing. | Replaced verbatim by the FINAL strings in Section 6. |
| A-015 rating hook | iOS Everyone / Play "Everyone but contains interactive elements (IE)"; randomized IAP would force Play "In-app purchases of random items" and possibly iOS Teen. | Answered explicitly in Section 4 (Q2). |
| A-014 QA gate | Monetization items block ship only; store readiness row "Monetization disclosed in listing | SG-6 | A-016". | Section 7 and 8 add conditions only. |
| A-013 test plan | Store category is a release-owned placeholder; A-10/A-11 machine checks exist. | Section 8 reuses A-10/A-11; no new test IDs or families. |

## 2. Free-to-play structure and fairness rules

The game ships free-to-play. All content required to complete the core game is free.

Fairness rules (binding):

1. **No pay-to-win.** The economy is play-only per A-007: the full permanent-upgrade total
   (2,195 cores) is obtainable by play alone against economy supply 10,200. No IAP SKU
   contains, converts to, or unlocks any stat-carrying currency. Purchasing nothing changes
   nothing about obtainability.
2. **Purchases are cosmetic only.** Per the PLAN §8 tension rule, no purchase may reduce
   drain tables, soften absorb gating, reduce failure pressure, or alter pull behavior.
   Every SKU in Section 4 changes appearance only.
3. **Digest-identity rule (extends the A-012 no-softening precedent, machine-checked by
   A-013 as A-10/A-11):** owning any monetization SKU — cosmetic, ad-removal, or premium —
   must leave the run digest byte-identical. The simulation never reads SKU ownership,
   ad state, or analytics state. Two players, one with every SKU and one with none,
   produce identical digests for identical inputs.
4. **Ad removal is never a stat buff.** The ad-removal SKU removes ad slots only. It grants
   no currency, no modifier, no continue, no revive, and no simulation input.
5. **Monetization never blocks core completion.** No gate, price, ad state, or SKU
   ownership is required to finish the campaign. Monetization readiness conditions
   (Section 7) block ship/submission only, mirroring A-015 SG gates and the A-014 gate.

Structure summary:

| Surface | Present at ship | Blocks completion? |
| --- | --- | --- |
| Base game | Free | — |
| Ads | Optional, skippable | No |
| Cosmetic IAP | Optional | No |
| Ad-removal IAP | Optional | No |
| Premium version | Absent (see §4.3) | No |

## 3. Ads

Placements are exactly the PLAN §5.14 list — no others may be added:

| # | Placement | Behavior |
| --- | --- | --- |
| AD-1 | Game over | One interstitial slot on the game-over screen, before retry options. |
| AD-2 | Retry | One slot on the retry screen. |
| AD-3 | Between levels | One slot on the level-transition screen. |
| AD-4 | Optional rewarded ad | Player-initiated only (see fairness rule below). |

Avoid list (PLAN §5.14, honored verbatim as prohibitions):

- No ads during gameplay.
- No forced interstitials (every slot is skippable and dismissible).
- No aggressive ad walls (at most one slot per screen; no chained or timed re-serves).

Additional binding rules:

- **Ads are optional.** Players may disable ads in settings; disabling removes AD-1..AD-4
  entirely and never affects gameplay. Ad state is not read by the simulation (digest rule).
- **Rewarded-ad fairness rule (AD-4):** the reward is cosmetic-only — an exclusive cosmetic
  tint or banner skin. It never grants cores, never grants a stat, never revives, never
  skips a level, and never touches drain tables, absorb gating, failure pressure, or pull
  behavior. AD-4 is player-initiated, skippable, and never auto-served.
- **Ad SDK choice is A-020's**, but whatever SDK ships must render only at AD-1..AD-4 and
  must not introduce placements outside this list.
- SH-10 (A-015) shows the actual slot position at game over/retry; AD-1/AD-2 geometry is
  therefore fixed before ship and must match the screenshot.

## 4. IAP SKU catalog

All SKUs are deterministic cosmetics. **Answer to A-015 open question 2:** no SKU is
randomized; there is no loot-box shape. Therefore the A-015 rating hook stands unchanged:
iOS Everyone, Play "Everyone but contains interactive elements (IE)". Play's "In-app
purchases of random items" band and iOS Teen are NOT required. If any future SKU becomes
randomized, rating bands must be revisited before submission (SG-6 extension, Section 7).

### 4.1 Cosmetic SKUs

| SKU id | Contents | Price band | Randomized? |
| --- | --- | --- | --- |
| iap.skin.aurora | Ship hull paint: aurora drift | Low | No — direct purchase, fixed contents |
| iap.banner.drift | Banner set: drift patterns | Low | No |
| iap.trail.comet | Trail cosmetic: comet tail | Low | No |
| iap.bundle.meteor | Bundle of the three above (no discount dependency) | Mid | No |

Bundle contents are identical to direct purchases; buying either path yields the same
cosmetics. No SKU contains cores, currency, modifiers, or stat-carrying items.

### 4.2 Ad-removal SKU

| SKU id | Contents | Price band |
| --- | --- | --- |
| iap.ads.off | Removes AD-1..AD-4. No other effect. | Mid |

Present at ship. Rationale: PLAN §5.14 recommends optional ad removal; it is the cheapest
compliance-safe way to honor "optional ads" for players who want none.

### 4.3 Premium version

Treatment: **absent at ship.** Rationale: the base game is free-to-play with optional ad
removal already covering the paying-audience case; a second SKU would duplicate content and
complicate submission (two SKUs to keep in sync) without fairness benefit. If A-020 later
ships a premium variant, it must be the same build with ads off and identical digests —
never a stat or content advantage.

## 5. Analytics schema

A-017 must enumerate exactly this scope. The game has no chat and no user-generated
content, so no UGC surface exists to moderate. Analytics is telemetry-only: no analytics
surface grants any gameplay advantage, and the simulation never reads analytics state.

Event names and properties (complete list — A-017's data-scope section must match):

| Event | Properties | Type |
| --- | --- | --- |
| `run_start` | `world_id` (int), `seed_hash` (string) | session |
| `run_end` | `world_id`, `seed_hash`, `outcome` (enum: complete/fail), `score` (int), `duration_ms` (int) | session |
| `level_complete` | `level_id` (int), `score` (int), `stars` (int 0–3) | progress |
| `upgrade_purchase` | `upgrade_id` (string), `cost` (int) | economy |
| `iap_purchase` | `sku_id` (string), `price_band` (enum) | monetization |
| `ad_impression` | `placement` (enum: AD-1..AD-4) | monetization |
| `settings_change` | `option_id` (string), `value` (string) | accessibility/UI |
| `crash` | `build_id` (string), `stage` (string) | stability |

Collection rules:

- No names, no contact info, no precise location, no device identifiers beyond what the
  store SDK requires, no screenshots, no text input of any kind.
- `seed_hash` and scores are gameplay telemetry only; they are never used to rank, match,
  or compare players. Daily/weekly leaderboards exist as a game surface (A-009 modes, score-only
  per A-015 §2.5) and are a separate concern A-017 covers; analytics never feeds them.
- Events fire client-side at the moments listed; storage/retention policy is A-017's to
  declare, scope is fixed here.

## 6. Disclosure wording — FINAL strings

These are the FINAL disclosure strings. A-015 replaces its placeholders with these
verbatim, and §3 localized strings are re-derived from this wording. Do not paraphrase.

Disclosure sentence (replaces A-015's placeholder "Optional ads and optional cosmetic
purchases are disclosed in the listing"):

> This game is free to play and can be completed without any purchase. It offers optional
> advertisements and optional in-app purchases of cosmetic items only. Purchases never
> affect gameplay, difficulty, or progression. Ad removal is available as an optional
> purchase. All in-app purchases are deterministic; no random-item or loot-box purchases
> exist.

Feature bullets (replace A-015's placeholders verbatim):

- Free to play; no pay-to-win; purchases are cosmetic only.
- Ads optional, never during gameplay.
- No randomized purchases; rating: Everyone with interactive elements.

## 7. Monetization readiness conditions (SG-6 extension)

SG-6 ("IAP/ads/analytics disclosed in listing and working", owner A-016) is extended —
extension only, never weakening — into these concrete conditions. All are ship/submission
blocking; none may ever gate core-game completion (PLAN §1.6 hard rule).

| ID | Condition | Verifies |
| --- | --- | --- |
| SG-6.1 | Disclosure shipped: the Section 6 FINAL strings appear verbatim in the live listing (both platforms). | A-015 SG-6 "disclosed" half |
| SG-6.2 | IAP works: every Section 4 SKU purchases, restores after account change, and grants only its listed cosmetics. | A-013 store placeholder "IAP works" |
| SG-6.3 | Ads work: exactly one slot renders at each of AD-1..AD-4; no slot renders elsewhere; ad-off setting removes all four. | A-013 store placeholder "Ads work"; SH-10 geometry |
| SG-6.4 | Analytics works: every Section 5 event fires once at its defined moment with only its defined properties; no undeclared property is collected. | A-013 store placeholder "Analytics works"; feeds A-017 scope |
| SG-6.5 | Rating bands match Section 4: no randomized SKU present; iOS Everyone, Play Everyone+IE. | A-015 rating hook |
| SG-6.6 | Fairness: digest-identity rule holds with every SKU owned and with ads off (see Section 8). | PLAN §8 tension rule, A-012 precedent |

These conditions extend the A-014 readiness row "Monetization disclosed in listing | SG-6 |
A-016" without loosening it. A-020 blocks ship until SG-6.1..SG-6.6 pass.

## 8. Fairness evidence for the QA gate

The digest-identity rule is checked by reusing the A-013 A-10/A-11 machine-check pattern
(no new test IDs, no new test families):

1. Run the A-10/A-11 digest harness with baseline SKU set (nothing owned, ads on).
2. Re-run with every Section 4 SKU owned and ads off; feed identical inputs (same seed,
   same world, same upgrade path).
3. Assert digests byte-identical. Any difference means a purchase softened the cost
   structure (drain tables, absorb gating, failure pressure, pull behavior) and fails the
   gate.
4. Repeat with the rewarded-ad reward granted (AD-4 cosmetic owned): digest must remain
   byte-identical, proving the reward never touched simulation stats.

This evidence satisfies SG-6.6 and is the fairness record A-014's monetization row points
at. It blocks ship only.

## 9. Open questions for downstream artifacts

For A-017 (privacy policy):

1. Data-scope enumeration: A-017 must declare exactly the Section 5 event/property set —
   nothing more. Confirm retention, opt-out, and SDK-level collection match that scope.
2. Contact info and policy URL: A-017 owns; A-016 supplies none.
3. Does the ad SDK (A-020's choice) collect data beyond the Section 5 schema? If yes,
   A-017 must disclose the SDK's own collection separately.

For A-020 (ship):

1. Ad SDK configuration: which SDK, and confirm it renders only at AD-1..AD-4 (SG-6.3).
2. Submission order vs SG gates: SG-6.1..SG-6.6 must pass before submission; monetization
   gates never gate core completion.
3. Premium version stays absent unless A-020 explicitly opts in under the Section 4.3
   constraint (same build, ads off, identical digests).

## 10. Answers to A-015's open questions (summary)

1. Final disclosure wording: Section 6 strings, to be inserted verbatim by A-015.
2. Randomized-SKU rating decision: all SKUs deterministic; rating bands unchanged
   (iOS Everyone / Play Everyone+IE); no "random items" band required.
3. Gate extension beyond SG-6: yes — SG-6.1..SG-6.6 (Section 7), extension only, all
   ship-blocking only.

