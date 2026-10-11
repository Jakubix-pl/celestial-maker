> Historical pre-expansion evidence. Current implementation and acceptance: [Expansion report](Expansion/REPORT.md). Previous payment systems and visual results below do not describe the free expansion.

# Feature audit — Celestial Maker 1.0 closure

Audit of existing implementation, not a second specification. `content.json`
is the actual data source; Simulation and Models are the shared Unity/simulator
Core. Evidence distinguishes exercised logic from every individual UI action.
No debug unlock is used as evidence of natural reachability.

| System | Data / execution | Player access / persistence | Status and evidence |
|---|---|---|---|
| Six stages / 24 processes | Content.generators, StageOpen, ComputeAllocation | Production: 1/10/100/Max/milestone, pause, reserves; State owned/paused/reserves | VERIFIED: natural policy, coupled online/offline, real six-stage Windows captures |
| 60 upgrades / 30 research | Content definitions; Purchase, indexed multipliers and prerequisites | Production / Research; purchases | VERIFIED logic/reachability, connected UI; not every button manually clicked |
| Three prestige layers | ResetRules, PrestigeReward, Rebirth, permanent upgrades and laws | Prestige previews/confirmation and thresholds; currencies/resets/laws | VERIFIED natural first/second/third progression, reset tests and repeated endgame |
| 24 talents / three specializations | Effect drives production, Resonance strength and voyage duration; Talent/Respec | Constellations; talents persisted, free exact refund | VERIFIED reachability/all specializations, respec cannot duplicate currency; UI connected |
| Resonance Links | Non-recursive baseline productivity; actual 15% output diversion / target gain | Constellations; links persisted | VERIFIED actual input-limited output loss and inactive source exclusion |
| Automation / profiles | Autobuy priority, repeat prestige threshold, automatic voyage claims | Prestige / Expeditions; profiles now include actual resource reserves | VERIFIED combined and reset online/offline comparisons; configuration persisted |
| 12 challenges | Real resource-specific restrictions, goal boundary, reward, original economy restore | Challenges: enter/restart/exit; records and active snapshot persisted | VERIFIED natural completion of all twelve; premium exclusion; no duplicate rewards |
| 48 achievements | Gross lifetime thresholds, actual configured reward | Achievements; persistent discovery IDs | VERIFIED all naturally reached, moderate permanent multipliers |
| 8 expeditions | Resource cost, level scaling, timer and guaranteed reward | Expeditions; voyages / levels, automatic returns | VERIFIED all eight natural launches/claims and offline exactly-once accounting |
| 12 story chapters | Authored original text and resource thresholds | Codex; discoveredStories persisted across resets | VERIFIED all naturally discovered; non-blocking archive |
| Offline / huge numbers | Bounded analytical events, Number pairs; one saved aggregate for 15 currencies | Scrollable changed-resource summary / details / last report | VERIFIED 1h–365d, buys/resets/depletion/unlock/boost/expeditions, interrupted serialized resume; Unity null migration |
| Saves / transfer | Native temp/backup replace; schema validation and migration; browser IndexedDB | Free import/export; independent entitlement store | VERIFIED core/native save tests; current browser/transfer checks in QA report |
| Menu / tutorial / settings / 2.5D | Runtime scenes, uGUI/TMP, Input System, mesh/rings/light/texture and procedural audio | Responsive pages, tooltip, confirmation/text/audio/quality/FPS settings | VERIFIED startup and running desktop visuals; exact viewport evidence in browser report; physical touch UNVERIFIED |
| Windows | Mono Development / Release profiles | EXE / ZIP; isolated -cmQA and automated -cmSmoke only in Development | VERIFIED build and Player evidence; no clean-commit claim |
| Web / Website / PWA | Real loader/wasm/data, immutable game release paths, public-assets-only SW | dist served by existing script, optional account UI, isolated qaProfile | Build/runtime VERIFIED; final offline/update evidence reported separately |
| Backend financial logic | Durable SQLite ledger, verified account session, provider response checks | Optional restore/activation/shop; export cannot create ownership | VERIFIED local unit/HTTP/contract tests including concurrent requests; providers BLOCKED by absent sandbox setup |
| OIDC / Stripe / Google | Existing official SDK adapters; PKCE/state/ID token expected + signature checks, verified paid/account/product state | Recovery and storefront, configured-only checkout | IMPLEMENTED_UNVERIFIED end to end; BLOCKED sandbox tests need credentials/accounts, not new implementation |
| Android | Exact editor Android module installed, OpenJDK prepared; ARM64 IL2CPP APK/debug AAB/production AAB profiles | Existing package ID; secret signing values outside source | BLOCKED SDK/NDK download/install needs explicit Google license acceptance. APK/AAB not created. Device tests UNVERIFIED |
| Publication | Deployment instructions and truthful draft policies | No fabricated operator/contact or live prices | BLOCKED operator/legal data, hosting/store/provider setup and platform acceptance |

Actual counts: 24 / 60 / 30 / 24 / 12 / 48 / 8 / 12, 218 unique content IDs,
zero empty names/story entries; acyclic resources/research/talent prerequisites.
Evidence: Logs/content-audit.json (node Tools/audit-content.mjs).

Natural strategy reaches Stellar Rebirth in 65 min, Galactic Collapse in
12.467 h and Universal Genesis in 3.865 days. The 30-day main-path run reaches
all ordinary content; extended late-game testing completes challenges,
expeditions and talents without currency injection. This is accelerated
simulation using the actual Core/data, not human month-long playtesting.

Closure boundary: user deferred mobile and then skipped further testing. Consult QA_REPORT.md for final UI patches built but not retested and remaining acceptance gaps; VERIFIED entries above are limited to their listed evidence.
