# Celestial Maker Observatory implementation and validation

Version 2.2.0, 11 October 2026. The selected Observatory direction is implemented
in the Unity PC interface. The Windows release is packaged and has passed an
isolated headless runtime check. Authored game content, controls, settings,
reports and runtime number formatting use English.

## Implemented interface

The graphite and turquoise interface uses IBM Plex Sans for ordinary text and
Rajdhani for branding. Flat panels and a static orbital menu mark replace the
illustrated workspace. Night and Dawn appearances use the same layout.

| Main section | Views and actions |
|---|---|
| Overview | Next decision, input bottleneck, chapter horizon, campaign requirements, last absence and tutorial |
| Production | Processes, Research, Upgrades and Flows; shared economy selector |
| Development | Resets, Energy/Matter/Time trees, Genesis foundations, strategies, specializations, milestones and challenges |
| Automation | Purchases, routing, research, construction, expeditions, all three reset rules, reserves, priority and stored automation profiles |
| Cosmos | Megastructures, reality laws, parallel universes, Resonance Links and expeditions |
| Journal | Achievements, Codex and the most recent offline report |

Settings and manual save remain on the navigation rail. All original functions
remain reachable. Locked views explain their requirements; they retain stable
positions as the game progresses.

Production uses aligned rows with units, running/paused/input-limited state,
actual output, stock, net rate and the affordable purchase cost. Buy 1, Buy 10,
Max and next milestone share one selector. Chapter and affordability/bottleneck
filters update on request, so rows do not move during a purchase. Buy and rate
refreshes retain the selected process and scroll position.

Process details appear in the side rail when space permits, or in a scrollable
dialog on smaller layouts. They distinguish installed capacity from actual
output, show consumption/reserves, offer pause/resume and reserve controls, and
link to an upstream producer. Parallel purchases and research go through the
existing universe editing API. Global development and cosmic views explicitly
identify their scope.

Reset previews show the live Core reward and requirements plus separate
retained/reset columns. Their details include expeditions, reserves, advanced
automation, parallel economy restarts and Cosmic Memory retention. Enabling an
automatic reset requires its own confirmation. Strategy application previews
rates before reallocating points. Megastructure buttons identify missing
unreserved resources or progression requirements; laws display the remaining
cooldown or occupied capacity.

Text scales through 100%, 125%, 150% and 200%; imported 80% values remain valid.
Wrapping rows and automatic content height prevent forced text shrinking.
Tab navigation is confined to a modal while it is open; closing it restores the
invoking control. These behaviors have programmatic evidence below; physical
keyboard and pointer usability still require a graphical session.

English was already used in most shipped authored content. This release keeps
the complete content set English, labels the language in Settings, and explicitly
sets the runtime/thread culture to en-US. User-authored imported profile names
are preserved. Save status distinguishes pending, successful and failed writes,
with a retry action for failures.

## Validation evidence

| Status | Result | Evidence |
|---|---|---|
| COMPILED | 117/117 Core assertions | `Logs/Observatory/core-tests.json` and `core-tests.log` |
| COMPILED | 20/20 Unity Core, serialization and asset checks | `Logs/Observatory/unity-core-tests.json` and `unity-core.log` |
| OBSERVED in edit mode | 1354/1354 UI geometry and callback assertions | `Logs/Observatory/ui-headless-tests.json`, final matrix log `ui-full-02.log` |
| BUILT | Windows Release 2.2.0, Unity 6000.6.3f1, 0 errors | `Logs/Observatory/build-release.json` and `unity-release.log` |
| OBSERVED in release executable | 33 headless assertions, no recorded runtime errors | `Logs/Observatory/player-headless.json`, `Player-20261011-104044/player.log` |
| VERIFIED archive | All 191 ZIP files match the release folder by SHA-256 | `Logs/Observatory/artifact.json` |
| VERIFIED preservation | Five player JSON files unchanged | `Logs/Observatory/save-integrity.json` |

The layout matrix covers 19 views plus menu/reset dialog, fresh and late campaign
fixtures, Night/Dawn, 1280×720, 1920×1080, 2560×1440 and 3440×1440, and all four
selectable text scales. Measurements compare preferred text height with assigned
rectangles and verify vertical-only scrolling. Additional assertions exercise
purchase cost/context, upstream navigation, reset preview/cancel/confirmation,
automatic-reset confirmation, parallel spending isolation, settings validation,
offline accounting, save failure presentation, focus restoration and authored
language checks. These are geometry/callback checks, not pixel inspection.

The actual packaged Release executable was run with a null graphics device using
`-batchmode -nographics`. It entered the menu and game scenes, advanced production,
used the actual purchase and reset callbacks, opened every route, rebuilt at
200% text in Dawn, completed offline catch-up, and saved/reloaded its isolated
profile. Its test hook is inactive in ordinary play and rejects execution with
graphics enabled. Test settings bypass PlayerPrefs; test saves live only under
the supplied fresh artifacts directory.

The first full matrix detected a short menu footer at 200%; that was corrected
before the successful final run. Earlier fixture issues were distinguished from
game behavior: a scroll assertion used a page too short to scroll, the edit-mode
EventSystem needed initialization, and Unity serialization of a null report
created a transient report ID. The final reset test uses a stable report and
compares the complete serialized state before preview and cancellation.

Existing Unity obsolete-API warnings remain in the build log. They are warnings,
not build errors. No Android/Web build, signing, publication or website download
replacement was performed.

## Save and balance compatibility

The save schema, filename, migration logic and resource/content IDs are unchanged.
`lightUI` is an optional settings field defaulting to Night. The text scale
validation ceiling is extended from 150% to 200% without altering old values.

`Logs/Observatory/core-balance-proof.json` compares all 13 Core source files and
content against the pre-task hashes. Twelve files are identical; the two changed
files differ only by the appearance setting and scale ceiling. Production,
automation rules, thresholds, construction speed, offline logic, resets and the
campaign completion definition remain identical. The previous approximately
400-hour calibration still applies; the six long campaign simulations were not
rerun for this UI-only release.

The previous release ZIP and source snapshot are retained in
`Backups/Observatory-20261011-101622`. The prior Expansion build and ZIP remain in
their existing locations. Five player save files are unchanged from this task's
initial fingerprint. No automatic migration of the user's live files was run.

## Delivery and reproduction

Windows ZIP: `Builds/CelestialMaker-Observatory-Windows.zip`.

Executable: `Builds/Windows/Observatory-Release/CelestialMaker.exe`.

`Play-CelestialMaker.cmd` now points to that executable. The launcher itself was
not run. The ZIP contains the game, required player files and an English README.
Its SHA-256 is
`71DB4D7AEA24D51439579779A07D992F2F0FBDB680596820D98F81B17B85E153`.

Run `Tools/build-observatory.ps1` to repeat Core tests, sequential Unity checks,
the release build, isolated headless executable validation and packaging. The
script uses the editor pinned in `Game/ProjectSettings/ProjectVersion.txt`.
`Tools/test-observatory-player.ps1` checks an existing release with a fresh
artifacts directory. The older graphical review fixture belongs to the previous
UI and was not used for this release.

## Remaining acceptance limits

Rendered appearance, glyph quality, physical keyboard/pointer interactions,
focus/minimize behavior in a desktop session and actual graphical FPS remain
**UNVERIFIED**. No desktop input, window automation, screen capture, visible game
or visible editor was used. The headless runtime result establishes functioning
release code and bindings; it does not establish graphical performance or
usability. User-study targets from the plan, such as finding a bottleneck in ten
seconds, remain unmeasured.
