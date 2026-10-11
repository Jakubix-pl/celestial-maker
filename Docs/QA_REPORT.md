> Historical pre-expansion evidence. Current implementation and acceptance: [Expansion report](Expansion/REPORT.md). Previous payment systems and visual results below do not describe the free expansion.

# Celestial Maker — final local closure record, 2026-10-10

Implementation and local Windows/Web builds were updated. Full cross-platform
1.0 and production monetization are not accepted. User deferred mobile and
then instructed skipping further testing. Results below are collected evidence;
omitted checks are not marked passing. Earlier QA report is preserved in
Logs/History/QA_REPORT-before-final-closure.md.

## CONFIRMED PASS

- Simulator 68 PASS/0 FAIL: simulator-result.json. Unity EditMode 15 PASS,
  PlayMode 1 PASS: editmode-closure.xml, playmode-closure.xml. Those runs precede
  final UI-only patches; changed UI paths were separately checked where noted.
- Backend 12 PASS/0 FAIL: backend-final-tests.log; subsequent targeted HTTP
  checks passed in backend-http-final.log, including concurrency/idempotency
  and unavailable developer endpoints. These are local/stub tests, not providers.
- Offline 1h/24h/7d/30d/365d, depletion, unlocks, boost expiry, automatic buys/
  resets, expeditions, huge Number pairs and interrupted serialized resume.
  Baseline 24h comparison error 0; coupled 24-process/autobuy 1h about
  1.1191e-13 relative difference, matching ownership. No offline time cap.
- Actual content: 24/60/30/24/12/48/8/12, 218 unique IDs, acyclic dependencies.
  Natural strategy first resets: 65 min, 12.467 h, 3.865 d. Main-path policy
  reached ordinary content in 30 simulated days; extended late fixture reached
  all challenges/expedition claims/talents in 11.20625 additional simulated days.
- Visible Windows Development smoke: passive progression, save/load, actual
  six-stage screenshots and four Player scenarios; zero runtime errors.
  windows-smoke.json timestamp 18:25:33 UTC precedes final UI-only patches.
- Windows Release menu/gameplay and isolated reload were observed; final
  landscape build startup/menu was observed. Final physical native import
  interaction was interrupted; it is not marked passing.
- Web: Home → Play → Bootstrap, New Game, passive progression, Settings,
  real validated import of a Windows copy and JSON export. Reopened save worked.
  Backend was stopped. Two-tab writer protection blocked the reader; closing
  the writer and reloading restored access. Its obscured read-only notice was
  subsequently corrected and built, but that notice was not retested.
- Wheel scrolling/offline details observed after fixes. Screenshots cover
  1920x1080, 2560x1440, 3440x1440 and portrait 400x900 browser viewports.
  Ultrawide exposed clipped lower navigation: height-based scaling was fixed
  and built afterwards, not visually retested. Portrait overview/navigation
  and repeated Full screen/Exit/Escape worked.
- Final Windows Development/Release and Web Release: Succeeded, zero build
  errors. Website HTTP evidence: 9 passing route/asset/download conditions,
  proper MIME/length, missing assets 404, absent backend 503, served Windows
  ZIP SHA-256 equal to local current ZIP (website-http-final.json).

## PERFORMANCE RESULTS

Development Player, 1920x1080, 600 frames (~10 s) per scenario. Ryzen 7 9800X3D,
RTX 4070 Ti, 31,653 MB RAM. Limit 60 FPS, VSync 0, display ~165 Hz. Background
applications included Cities Skylines II/Chrome/Steam; unrelated processes
were not stopped. Measurements precede final UI-only patches.

| Scenario | Owned units / flowing types | FPS | Mean/p95 frame ms | Mean/max tick ms | Unity allocated MiB |
|---|---:|---:|---:|---:|---:|
| Early Overview | 1 / 1 | 60.00 | 16.668 / 16.668 | 0.090 / 0.890 | 130.73 |
| Late Production, 24 types unpaused | 317 / 16 | 59.99 | 16.668 / 16.668 | 0.367 / 2.038 | 135.07 |
| Autobuy and expeditions | 317 / 16 | 60.00 | 16.668 / 16.668 | 0.528 / 2.239 | 135.21 |
| 365-day catch-up and summary | 595 / 14 at end | 59.82 | 16.718 / 16.667 | 0.737 / 17.200 | 135.83 |

Main-thread means 0.513–1.487 ms, present-wait means 15.225–16.149 ms. The cap
explains ~60 FPS. Batches reduced from 256 to 32 lowered yearly maximum tick
from 113.244 to 17.200 ms; live remainder persists rather than being discarded.
Unit counts are numerical ownership, not one rendered object per unit; visuals
are bounded. Allocator memory is not process working set. Phone FPS/hardware
upper limits are N/A.

## KNOWN LIMITATIONS

Accelerated balance/reachability is not month-long human playtesting. Mantissas
have ~15 significant digits. Progress is client-owned; import cannot create
verified paid rights. Full screen fills the viewport, not guaranteed OS mode.
Unity warns IAP 4 is deprecated; installed resolver requests Billing 8.3.0,
while actual device acceptance and an IAP migration decision remain open.
Operator/legal content and production service setup remain absent.

## HISTORICAL / NON-BLOCKING ISSUES

Prior packages/logs remain in History/20261010-before-closure. Old 50/8/8 counts
and 59.40 FPS are earlier evidence. Intermediate profile-selector escaping
compile errors were corrected and superseded by successful builds. Transparent
report, Web paste field and weak wheel scrolling were fixed and checked.
A raw transfer comparison found unequal dynamic fields after simulation
continued; this is not a confirmed corruption or transfer failure.

## UNVERIFIED / OMITTED

- PWA offline after server shutdown and complete worker-update acceptance.
  Immutable files/release-pinned cache entry and manifest are implemented.
- Final ultrawide visual retest, persistent read-only notice retest and final
  post-UI-patch performance repetition: omitted at user's request.
- Final Windows → Web → Windows comparison: Web import/export and native copied
  profile load were exercised; stable profile/purchases/prestige/talent/challenge/
  story fields matched. Resources/ownership/time changed after further gameplay.
  Final interpretation and physical native import-field acceptance unfinished.
  Raw evidence: save-transfer-final.json and copied JSON files.
- Storage-denial/loading-error/retry acceptance, physical ultrawide, physical
  touch/safe area/thermal behavior and human balance acceptance.
- Android explicitly deferred: matching module installed/JDK prepared; no
  SDK/NDK, APK/AAB, physical device or production signing acceptance.
- Real OIDC recovery, Stripe sandbox, Google Play receipt/restore/refund:
  provider setup absent. Local tests do not substitute for these.
- Public HTTPS deployment, production signing and publication not performed.

## Commands and artifacts

Executed during closure: Tools/validate.ps1; Unity -runTests -testPlatform
EditMode/PlayMode with XML/log output; node --test test/*.test.ts and targeted
HTTP test; node Tools/audit-content.mjs; Tools/build-windows.ps1 with/without
-Release; Tools/build-web.ps1 -Release; Tools/start-website.ps1;
node Logs/ImplementationScripts/check-site.mjs; Development Player -cmSmoke
-cmFixture Logs/late-game-fixture.json -cmArtifacts Logs; isolated Release
-cmProfile closure-release-20261010.

Windows ZIPs: Builds/CelestialMaker-Windows-Development.zip and
CelestialMaker-Windows-Release.zip (Windows.zip is the Release alias).
Players: Builds/Windows/Development and Release; Web: Builds/Web;
site: Website/dist. Provenance/SHA-256: Logs/artifact-manifest.json.
Start at repository root: ./Tools/start-website.ps1, then
http://localhost:4310/play. Test server is stopped at closure end.
No new tests ran after the instruction to skip testing.
