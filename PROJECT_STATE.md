# Celestial Maker — current project state, 2026-10-11

Root: C:/Users/Jakubix/Desktop/Idle; Unity project: Game, editor 6000.6.3f1.
Closure continued the existing project. Regional Logistics was untouched.
Public Windows v2.7.0 is published and its local delivery is verified.

## Public Windows delivery - 2026-10-11

Public repository: https://github.com/Jakubix-pl/celestial-maker.
Published release: https://github.com/Jakubix-pl/celestial-maker/releases/tag/v2.7.0.
Anonymous readback verified the README, all ten gallery/icon files and downloadable
checksums. GitHub asset SHA-256 digests match all three local release files.
English README, nine actual offscreen UI screenshots, release notes, portable
fixtures and a GitHub Actions template are prepared. Hosted CI is not enabled:
the authenticated publication token lacks workflow scope. Clean source: Core 250/250 and extended
system audit 63/63. Repeated Unity UI: 1479/1479. Runtime sources match the tested
2.7.0 player; no game binary change was needed for publication.
Offscreen performance: 22 cases, 5280 measured frames, zero >16.67 ms;
worst p95 3.6681 ms, maximum 15.507 ms on RTX 4070 Ti / Ryzen 7 9800X3D.
Physical FPS, Windows DPI and human play remain UNVERIFIED.
DELIVERED: installer, portable ZIP, SHA-256 and English START HERE.txt copied to
C:/Users/Jakubix/Desktop/Calestial Maker. All copied artifact hashes match.
Details: Docs/Release/VALIDATION.md. APK for S26 Ultra / Android 17 / One UI 9
is newly requested and will follow the Windows delivery; it is not yet built.

## Current 2.7.0 Discovery - completed 2026-10-11

IMPLEMENTED: all eight approved groups: engineering modules, ordered goals,
background six-hour comparisons, challenge rules/readiness, expedition signals
and permanent routes, universe roles, isolated practice, chronicle and separate
optional scenario saves. English game/installer. Fixed numerical tick stalls,
protected-stock accounting, plan comparison responsiveness and overview layout.
COMPILED: Core 261/261; Unity Core 20/20; UI 1479/1479; EditMode 17/17;
PlayMode 1/1; extended system audit 63/63; Discovery tick audit 4/4.
BUILT: Windows x64 Release 2.7.0, zero errors. Actual EXE 94/94 checks,
and the installed EXE 94/94. Per-user English installer, optional desktop icon,
no automatic launch, uninstall preserves saves. Installer is unsigned.
OBSERVED: full 60 Hz campaigns with six-hour visits and serialized reloads:
Energy 415.625284h, Matter 371.800284h, Time 407.082663h. Nine fresh scenario
combinations complete with measured scripted policies; Hungry cosmos needs
upstream investment. All twelve challenges completed from an endgame fixture.
56 private URP UI renders; representative views and populated lab inspected.
Physical FPS, Windows DPI and human desktop gameplay remain UNVERIFIED.
No desktop control/capture or visible game/editor. 13 protected files unchanged.
Silent install/reinstall/uninstall passed 26 checks plus installed runtime tests.
ZIP: 193 hash-verified entries. Report: Docs/Discovery/REPORT.txt.
Evidence: Logs/Discovery/final-verification.json. Backup: Backups/Discovery-20261011-150009.
Installer: Builds/CelestialMaker-Setup-2.7.0.exe.
Portable: Builds/CelestialMaker-Observatory-Windows.zip.

## Feature research after 2.6.0 - 2026-10-11

RESEARCH ONLY: compared current content/code with Steam, Reddit and itch.io
player/developer discussions. Priorities: meaningful late-game projects,
optional goal-based automation, six-hour plan comparison, varied challenges,
expedition discoveries, universe roles, replayable teaching and a richer finale.
Existing offline replay, profiles, diagnostics and recovery are not missing.
The 2.6.0 report's up-to-84h discovery gaps are the main local content concern.
Recommendations and sources: Docs/FeatureResearch/RESEARCH-2026-10-11.txt.
No gameplay changes, new build, runtime test or participant playtest in this pass.

## Previous 2.6.0 Offline cadence - 2026-10-11

IMPLEMENTED: literal 60 Hz offline replay through the live tick engine, active
research/purchases/resets/expeditions/parallel transfers, resumable cursor and
6 ms soft frame budget. Opt-in Development Plan for Energy/Matter/Time manages
known development between visits; the first reset of each type stays manual.
Basic guided research restores production after Prestige; construction base
work is 6.6h. Existing saves retain progress and start with Development Plan OFF.
COMPILED: Core 185/185, Unity Core 20/20, UI 1425/1425, PlayMode 1/1.
BUILT: Windows Release 2.6.0, zero errors, actual headless EXE 65/65 checks.
OBSERVED: full tick campaigns with strict six-hour visits from the start finish
Energy 421.675790h, Matter 377.909123h, Time 411.452733h. Enable Automation >
Development Plan after first Prestige for this cadence. Script choices and
reserves affect outcomes. Three six-hour fixtures each replay 1,296,000 ticks;
online and interrupted/reloaded replay agree. Tick audit acceptance 12/12.
The older event accelerator differs numerically on the huge-stock parallel
stress fixture; it is only a structural cross-check there, not the balance
reference. Event-based forecasts remain approximate. Full 1h/6h/24h replays
passed; 7/30/365-day queues preserve pending time but a full year was not replayed.
20 private URP renders generated and representative views inspected. Physical
FPS/input/Windows DPI remain UNVERIFIED. No desktop control/capture or visible
editor/player window. All 13 protected file hashes match baseline; user save
JSONs untouched. Only Unity's overwritten test-result XML was restored.
ZIP: 49685075 bytes, 193 verified entries.
SHA256: 9DC38D010662D29C94AC1F9A7B2B467F2BFEFAA7BFB68C65FB2E53F7CC34B5F4.
Report: Docs/OfflineCadence/REPORT.txt. Evidence: Logs/OfflineCadence/final-verification.json.
Backup: Backups/OfflineCadence-20261011-140629.

## Previous 2.5.0 Observatory Evolution - 2026-10-11

IMPLEMENTED: all eight approved improvements: diagnostics, return briefing/guides,
complete operations profiles, real-engine reset forecasts/history, interactive
flows, distinct strategic focus, evolving cosmos/earned appearances, restore points.
Second player-research pass added exact reserve presets, explicit bulk release,
fuller profile review and before/after construction estimates. Fixed automatic
upgrade reserve bypass, malformed checkpoint/history data and rollback throttling.
COMPILED: Core 171/171; Unity Core 20/20; UI 1422/1422; PlayMode 1/1.
BUILT: Windows Release 2.5.0, zero errors; actual EXE 54/54 headless checks.
ZIP: 49676181 bytes, 193 verified entries.
SHA256: F94BDF17C98684331DF3A58FB03E911D18A2BF98B3F8C7CBFA60D5C0BD79CD60.
OBSERVED: real-Core scripted active completion 367.25-384.96h; hourly background
408.66-436.50h. Offline Matter matches online across 18 daily reloads. Six-hour
check-ins take 803.14-822.45h. Late-game discovery gaps remain a design limitation.
354 private renders produced; nine representative images inspected. Final 16-case late-game
benchmark: worst UI CPU p95 0.525ms, simulation p95 0.137ms. Physical input, Windows DPI and visible-player FPS
remain UNVERIFIED. No desktop control/capture or visible game/editor window.
All 13 protected files match baseline; save JSON files were never changed. Unity's
automatically overwritten TestResults.xml was restored from the verified backup.

Report: Docs/ObservatoryEvolution/REPORT.txt; PLAYER-RESEARCH.txt contains sources.
Evidence: Logs/ObservatoryEvolution/final-verification.json.
Backup: Backups/ObservatoryEvolution-20261011-132444.

## Previous 2.4.3 Observatory currency visibility — 2026-10-11

IMPLEMENTED: undiscovered resource/reset-currency fields stay hidden; reset
currencies appear at first available reward or prior recorded discovery, and
remain visible at zero after spending, higher resets and reload. Locked reset
cards retain unlock guidance. No save-schema or balance changes.
COMPILED: Core 144/144, Unity Core 20/20,
UI 1389/1389; actual EXE 45 null-graphics checks passed.
BUILT: Windows Release 2.4.3, zero errors; ZIP 49,657,862 bytes,
193 verified files, SHA256 AF844255C817A559D35CCD053D12D1AF1A7CC90C7CCDC92A4F88412B5699D693.
OBSERVED: 14 private Unity URP renders, six representative views inspected.
No desktop control/capture or visible game window; all 13 protected files
unchanged. Physical input, Windows DPI and visible-player FPS remain UNVERIFIED.

Report: Docs/UI-Minimalist-Futuristic/UNLOCKS-RELEASE.txt.
Evidence: Logs/ObservatoryUnlocks/final-verification.json.
Prior 2.4.2 release, sources and evidence: Backups/ObservatoryUnlocks-20261011-130735.

## Previous 2.4.2 Observatory resource header — 2026-10-11

IMPLEMENTED: stable resource colors, vector pictograms, tinted header fields
and accent lines; neutral stock values, preserved negative NET warning.
Resource identities follow chapter, reset and selected-economy changes together.
Home Prestige Points read live home state; counters retain per-frame updates.
COMPILED: Core 144/144, Unity Core 20/20,
UI 1386/1386; actual EXE 45 null-graphics checks passed.
BUILT: Windows Release 2.4.2, zero errors; ZIP 49,657,248 bytes,
193 verified files, SHA256 D2B7BA48A36353607DD9DD806ED8175493E32F7F9CC776B16E1411CD6DBBB417.
OBSERVED: 24 private Unity URP renders; six full-resolution images and a contact
sheet inspected. No desktop control/capture or visible game window.
Physical input, Windows DPI and visible-player FPS remain UNVERIFIED;
no new FPS benchmark. All 13 protected user files unchanged.

Report: Docs/UI-Minimalist-Futuristic/RESOURCES-RELEASE.txt.
Evidence: Logs/ObservatoryResources/final-verification.json.
Prior 2.4.1 release, sources and evidence: Backups/ObservatoryResources-20261011-125640.

## Previous 2.4.1 Observatory center tabs — 2026-10-11

IMPLEMENTED: distinct center-tab colors and vector icons, subtle tinted fills,
bold active labels and underlines; matching compact Views controls in both themes.
COMPILED: Core 144/144, Unity Core 20/20,
UI 1384/1384; actual EXE 45 null-graphics checks passed.
BUILT: Windows Release 2.4.1, zero errors; ZIP 49,655,837 bytes,
193 verified files, SHA256 60F4FF441FF4F0DCAC88B705EF08A78EAD87CBE66922A69D17DEB67BD8CC49B9.
OBSERVED: 32 private Unity URP renders; six representative images inspected,
including 100/200 percent and narrow Views layouts. No desktop control/capture
or visible game window. Physical input, Windows DPI and visible-player FPS
remain UNVERIFIED; no new FPS benchmark for this static tab update.
All 13 protected user files unchanged.

Report: Docs/UI-Minimalist-Futuristic/TABS-RELEASE.txt.
Evidence: Logs/ObservatoryTabs/final-verification.json.
Prior 2.4.0 sources, package and evidence: Backups/ObservatoryTabs-20261011-123115.

## Previous 2.4.0 Observatory readability — 2026-10-11

IMPLEMENTED: six category accents/symbols, selected filters, compact expandable
research rows, exact requirements/ETA, real-engine purchase comparison, adaptive
navigation/filters, search/favorites/shortcuts, persisted preferences, pinned goal
notifications, per-economy research plans and opt-in gated queue execution.
Resets pause the queue and restore lost prerequisites. Online/offline decisions
use the same timing. English and the original custom icon remain in the build.

COMPILED: Core 144/144, Unity Core 20/20,
UI 1384/1384; actual EXE 45 null-graphics checks passed.
BUILT: Windows Release 2.4.0, zero errors; ZIP has 193 verified
files, 49,653,355 bytes, SHA256
2F5082D101C4F5FB2913E7A264FF4F42CAA389981B064E8D84A009FFE0418DAD.

OBSERVED: 297 private Unity URP renders; 16 late-game
performance cases, 0/3840 samples above 16.67 ms. This is offscreen evidence;
physical input, OS DPI and sustained visible-player FPS remain UNVERIFIED.
No desktop control, screen capture or visible game/editor windows.
All 13 protected user files stayed identical. Progression constants unchanged;
the 400-hour campaign has not been remeasured with the new optional queue.

Report: Docs/UI-Minimalist-Futuristic/READABILITY-RELEASE.md.
Renders: Docs/UI-Minimalist-Futuristic/VISUAL-GALLERY.html.
Evidence: Logs/ObservatoryReadability, Logs/Observatory, Logs/ObservatoryVisual,
Logs/ObservatoryPerformance. Prior release/source backup:
Backups/ObservatoryReadability-20261011-115344.

## Previous 2.3.0 Observatory performance and final visuals — 2026-10-11

IMPLEMENTED: per-frame resource/progress presentation (60 FPS desktop default,
120 optional), visible-row updates, separate 250 ms derived bindings, cached
economy, independent canvases, hidden 3D scene suppression, HD SDF font sources,
downsample-only artwork, reduced-motion-aware transitions, original art and icon.
Desktop and workspace shortcuts use the custom ICO; EXE embeds the same emblem.

COMPILED: Core 117/117, Unity Core 20/20, UI 1368/1368; actual
Windows EXE passed 33 null-graphics checks. No user save was used by fixtures.

BUILT: Windows Release 2.3.0, zero errors. Latest ZIP has
193 verified files, 49,638,729 bytes, SHA256
883C0432B7D28AAD7E8E05905B9932C51916FB67171D87856FD667734F0F3078.

OBSERVED: 255 offscreen URP images; thirteen measured late-game cases.
4K UI CPU mean 0.630 → 0.371 ms versus naive 60 Hz.
Final offscreen run: 0/3120 samples >16.67 ms; earlier runs had render/readback
spikes around 42 ms. This does not certify ordinary-player 60 FPS. Physical input, OS DPI and
visible-player frame pacing remain UNVERIFIED. No desktop control or visible window.

Five user files and all 28 Core source/meta files hash-unchanged. Prior 2.2.1
release backed up in Backups/ObservatoryPerformance-20261011-112036.
Report: Docs/UI-Minimalist-Futuristic/PERFORMANCE-RELEASE.md.
Design/research: Docs/UI-Minimalist-Futuristic/FINAL-DIRECTION.md.
Renders: Docs/UI-Minimalist-Futuristic/VISUAL-GALLERY.html.
Evidence: Logs/ObservatoryPerformance and Logs/Observatory.

## Previous 2.2.1 Observatory visual review 2026-10-11

IMPLEMENTED: navigation fits whole words at 200%, explicit high-contrast button
states in Night/Dawn, clearer focus/selection, full-height absence/universe
dialog scrolling, clipped JSON input, bounded ultrawide workspace, orbital line
polish. Gameplay Core remains unchanged (28 source/meta files hash-compared).

COMPILED: Core 117/117, Unity Core 20/20, UI geometry/callback/regression
1359/1359, all passing. Five new checks cover these readability regressions.

BUILT: Windows Release 2.2.1, zero errors; the actual EXE passed 33 null-graphics
runtime checks. Builds/CelestialMaker-Observatory-Windows.zip contains 191
verified files. Previous ZIP/source preserved in
Backups/ObservatoryVisual-20261011-104846.

OBSERVED: actual offscreen Unity URP rendering of all 20 named views plus menu,
dialogs and additional states, with 247 final PNGs. Night/Dawn, 720p/1080p/QHD/
ultrawide, 100/125/200% visual samples; broader automated geometry includes 150%.
Rendering used the Desktop pipeline and a private RenderTexture in batch mode.
No desktop control, screen capture or visible game/editor. Physical input,
OS display scaling, live graphical player behavior and FPS remain UNVERIFIED.
Five player JSON files remain unchanged; test saves were isolated.

Report: Docs/UI-Minimalist-Futuristic/VISUAL-REVIEW.md; full image gallery:
Docs/UI-Minimalist-Futuristic/VISUAL-GALLERY.html. Evidence:
Logs/ObservatoryVisual and Logs/Observatory. Offscreen renderer:
Tools/render-observatory.ps1. The 400-hour balance was not changed or remeasured.

## Historical 2.2.0 Observatory and English interface 2026-10-11

IMPLEMENTED: Observatory in Unity uGUI/TMP, six primary sections, all original
routes retained as subviews, compact production rows, stock/net/cost display,
filters, selected process diagnostics and upstream navigation. Local economy
selection covers home and parallel production/research/flows; global systems
are labeled separately. Energy/Matter/Time columns, profile previews, explicit
reset retention, confirmation before enabling automatic resets, construction
and law blockage reasons, Night/Dawn appearance, responsive text at 100-200%
(legacy 80% still valid), English UI/content and en-US runtime culture.
Save status follows successful writes; failures have a retry action.

COMPILED: pinned Unity 6000.6.3f1, sequential batchmode/nographics. Core 117/117,
Unity Core/serialization 20/20, Observatory edit-mode checks 1354/1354.
The matrix covers 19 routes, menu and reset dialog, early/late fixtures, two
themes, 1280x720 / 1920x1080 / 2560x1440 / 3440x1440 and 100/125/150/200% text.

BUILT: Windows Release 2.2.0, zero errors, Builds/Windows/Observatory-Release
and Builds/CelestialMaker-Observatory-Windows.zip. All 191 archived files hash
match the build folder. Play-CelestialMaker.cmd points to Observatory; the
launcher was not executed. Previous Expansion release and ZIP remain available.

OBSERVED: the actual Release EXE ran with -batchmode -nographics and a fresh
isolated save, passing 33 assertions including scene startup, all 19 routes,
live simulation, purchase, reset/cancel, offline catch-up, English culture,
200% text/theme rebuild and save/reload. No runtime errors were recorded.
No desktop input/window control, screen capture or visible game/editor was used.
Rendered appearance, physical input usability and real graphical FPS remain
UNVERIFIED; headless geometry/callback checks do not certify those properties.

Core balance/content are unchanged: exact source comparison permits only the
new lightUI preference and textScale validation ceiling change from 1.5 to 2.
No new pacing claim was measured; prior ~400-hour calibration remains the basis.
Five player JSON saves hash-match the pre-task snapshot. Runtime fixtures skip
player preferences; production save paths/schema and migrations are retained.

Report: Docs/UI-Minimalist-Futuristic/IMPLEMENTATION.md. Evidence:
Logs/Observatory. Reproduce with Tools/build-observatory.ps1. Source/old release
backup: Backups/Observatory-20261011-101622. No publication, remote Git,
Android/Web player or website download replacement in this task.

## Historical UI direction planning 2026-10-11

Requested scope: research and design planning. Nine popular/relevant idle and
incremental titles reviewed using current Steam listings and official promotional
material. Recommended direction: orbital observatory, six navigation groups,
compact process rows, contextual diagnosis and explicit reset consequences.
Plan: Docs/UI-Minimalist-Futuristic/PLAN.md. Two interactive concept layouts in
the thread visualization: celestial-observatory.html (Obserwatorium / Terminal).
120 headless document layout checks and selected local interactions passed;
no desktop input, screen capture or graphical game launch. This is prototype
evidence, not Unity/player acceptance. Pencil app connection was unavailable.
No game UI implementation or new Unity build was performed in this planning pass.
The planning baseline was the following 2.1.0 build, superseded by 2.2.0 above.

## Previous 2.1.0 approximately 400-hour campaign 2026-10-11

IMPLEMENTED: recalibrated earned-point thresholds (Ascension 8, Genesis 12),
Energy/Particle Prestige scoring, construction growth 2.06, concurrent planner
with progression targets, permanent campaign completion and PC progress display.
Completion means all 9 Ascension and 3 Genesis nodes, three universes, and all
three megastructures at stage 6. Challenges/records/endgame remain optional.
Desktop background clock and 5 FPS background rendering are configured; true
suspension saves its original timestamp and uses offline catch-up once.

COMPILED: 117/117 Core assertions; 20/20 Unity headless checks; pinned 6000.6.3f1,
sequential batchmode/nographics, no Play Mode. Website build and local HTTP test pass.
BUILT: Windows Release 2.1.0, zero errors, current Expansion-Release executable
and CelestialMaker-Expansion-Windows.zip. Website/dist-free serves the identical
ZIP; packaged runtime DLL matches the built DLL. Launcher was not run.

OBSERVED: six fresh-save real-Core policies, completion 364.23–436.44 hours,
mean 393.78. Frequent decisions every 15 minutes; background decisions hourly
after a two-hour initial session. A separate full offline Matter run with 18
save/reload cycles matches its 436.437-hour result exactly. This is a measured
policy range, not a guarantee for unattended or arbitrary strategies.

Old saves keep schema 2 and their invested resources/work. Eleven legacy fixtures
and three prior expansion fixtures passed; five user save files remain identical.
Backup of previous source/evidence/ZIP: Backups/Pacing400-20261011-093557.
Current report: Docs/Expansion/PACING400.md. Evidence: Logs/Pacing400, current
Unity logs in Logs/Expansion. Reproduce with Tools/measure-pacing.ps1.

No desktop interaction, screenshots, browser, graphical player or Play Mode.
Rendered UI, actual player FPS and focus/minimize/resume remain UNVERIFIED.
No new Web/Android player, remote publication, signing or unrelated project work.

## Historical: PC expansion 2.0.0 — superseded pacing and package

IMPLEMENTED: free-game conversion; schema-2 migration/retention; Prestige →
Ascension → Genesis; Energy/Matter/Time trees and three profiles; 3 staged
megastructures, 9 laws, home + 2 parallel economies, bounded exchange; automation,
analytics and new PC routes; 60 Hz logical clock and unlimited analytical offline.

COMPILED: Unity 6000.6.3f1, sequential batchmode/nographics, 0 build errors.
89/89 Core assertions, 18/18 Unity headless method/asset/JSON checks, backend
integration and local website HTTP verification passed. See Logs/Expansion.

BUILT: Builds/Windows/Expansion-Release/CelestialMaker.exe and
Builds/CelestialMaker-Expansion-Windows.zip. Current site Website/dist-free.
Launcher updated. Older packages/site outputs are historical. No new Web/Android
player, remote publication or production signing is claimed.

OBSERVED: three 90-day real-Core strategies: Prestige 2 h; Ascension 2.58 d;
Genesis 28.08/28.58/31.08 d; final system unlocks 80.62/82.12/84.62 d for
Energy/Matter/Time respectively. Two check-ins daily after an active first session.
These are measured policies, not universal timing or 100% collection guarantees.

No desktop control, keyboard/mouse, clipboard automation, screenshots, browser,
visible game/editor or Play Mode used. Visual interaction, actual player 60 FPS
and long-session graphical acceptance remain UNVERIFIED. Do not close user apps.
If an interactive editor locks Game, use an isolated copy or defer the batch.

Backup: Backups/Expansion-20261011-083628. Saves and historical entitlement rows
were preserved. All code remains local/uncommitted. No baseline commit exists.
Detailed report, migration matrix, balance protocol and research:
Docs/Expansion/REPORT.md, RETENTION.md, BALANCE.md, RESEARCH.md.

## Historical PC visual overhaul — before expansion, 2026-10-11

IMPLEMENTED: All requested PC screens, shared uGUI/TMP design system, comic-style
cosmic illustration and panels, fonts, tooltips, dialogs and offline report.
Pencil is connected; Design/CelestialMaker-UI.pen is saved with the 600×400 start
panel plus menu/overview/production/prestige art direction. Researched follow-up
adds input bottleneck diagnosis, research filters and exact next-horizon progress.
Economy, progression and save rules are unchanged.

COMPILED: Unity 6000.6.3f1, final Development and Release, zero build errors.
BUILT: Builds/Windows/UIOverhaul (Development) and Builds/Windows/PC-UI-Release.
Launch via Play-CelestialMaker.cmd. Portable: Builds/CelestialMaker-PC-UI-Windows.zip.
OBSERVED: Development Player at 1920×1080, 2560×1440 and 3440×1440: 40/40 focused
UI checks each, zero runtime errors and zero TMP overflow candidates. Inspected
actual screenshots. Release startup additionally passed a 10-second window/process
smoke check; Release interactions were not separately repeated. Test saves isolated.

The initial prefab component naming defect was fixed and passing runs supersede
its retained historical log. Report, file map, screenshots and research sources:
Docs/UIOverhaul/REPORT.md, RESEARCH.md and Logs/UIOverhaul. Backend/payment-provider
flows, Android and Web were outside this PC pass. Optional remaining UI polish:
chapter-specific artwork/icons and complete keyboard-focus navigation review.

## Previous game closure (2026-10-10; historical evidence)

Six stages; 24 generators, 60 upgrades, 30 research, 24 talents, 12 challenges,
48 achievements, 8 expeditions and 12 stories. Closure connected declared
bonuses, productive Resonance sources, actual automation reserves and reset
boundaries. Natural first resets: 65 minutes / 12.467 hours / 3.865 days.

Offline reporting persists one aggregate for all 15 currencies, before/after/
net, actual produced/spent/reset/reward totals and activity counters. Interrupted
catch-up and live remainder persist without a time cap. Event budget is 32.
Short absences still process fully; their report remains accessible without
an automatic modal until 60 seconds of absence.

Windows Development/Release and Web Release rebuilt with zero build errors.
Website/dist contains immutable game/site releases and the current Windows ZIP.
Browser-native validated import, wheel scrolling, persistent read-only status
and height-based landscape scaling were implemented. The last landscape fix
is built but not visually retested.

Local backend verification/idempotency/concurrency and OIDC SDK checks were
corrected. Native Release has no default localhost API; Web uses /api.
Provider end-to-end acceptance remains unverified.

## Collected evidence

Simulator 68 PASS/0 FAIL; Unity EditMode 15 PASS, PlayMode 1 PASS; backend 12 PASS.
Player measurements before final UI-only patches: about 60 FPS, 0.090–0.737 ms
mean simulation step; yearly catch-up maximum step 17.200 ms (previously 113.244).
See Docs/QA_REPORT.md, FEATURE_AUDIT.md and BALANCE.md. Build profiles, dates,
source fingerprints and checksums: Logs/artifact-manifest.json.
Previous working packages: Builds/History/20261010-before-closure.

## User-directed omissions and remaining acceptance

User deferred mobile and declined SDK/NDK license acceptance. Android module
and JDK are prepared; no SDK/NDK, APK/AAB or device acceptance is claimed.
The earlier request to skip further testing was superseded by explicit PC UI
testing authorization on 2026-10-11. The new PC ultrawide review is recorded above.
PWA offline/update acceptance and the old final transfer comparison remain
UNVERIFIED and outside this UI pass.

Provider sandbox configuration, physical Android, signing, operator/legal data
and public hosting remain external release gates. Full cross-platform 1.0 and
publication readiness are not claimed. No real purchases or public deployment.
