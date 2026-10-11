# Celestial Maker — PC UI delivery, 11 October 2026

## Outcome and evidence states

| State | Result |
|---|---|
| IMPLEMENTED | All requested PC screens use the new uGUI/TMP presentation. The existing simulation, progression, save validation and account bindings remain in place. |
| COMPILED | Unity 6000.6.3f1 compiled the project for both final Windows builds. Zero build errors; existing deprecation/toolchain warnings are not represented as zero warnings. |
| BUILT | Development: `Builds/Windows/UIOverhaul/CelestialMaker.exe`. Release: `Builds/Windows/PC-UI-Release/CelestialMaker.exe`. |
| OBSERVED | Real Development Player screens captured at 1920×1080, 2560×1440 and 3440×1440; 40 checks passed at each size. Release separately launched with an isolated profile and remained alive with a game window for 10 seconds. Release interactions were not separately repeated. |

Start the final version with `Play-CelestialMaker.cmd` in the repository root, or
the Release executable above. The portable package is
`Builds/CelestialMaker-PC-UI-Windows.zip`; extract the whole archive before running.
Normal launch uses the existing local save. Acceptance tests used separate saves.

## Visual redesign

The interface uses a dark navy observatory, illustrated ringed-planet panorama,
clipped comic-panel corners, fine illuminated borders, warm gold primary actions,
teal production values and six chapter accents. A persistent resource header and
navigation rail frame a scrollable workspace. A contextual right rail appears
when there is enough width; ultrawide uses three columns of cards. Large-text
mode retains the next goal inside Overview when the inspector is omitted.

Implemented screens: Main Menu, Continue/New Game, HUD, Overview, Production,
Research, Prestige, Constellations, Challenges, Expeditions, Achievements, Codex,
Shop, Settings, Credits, tooltips, confirmation dialogs, offline summary,
import dialog and reward notifications. Export retains the existing file and
clipboard operation, with an on-screen result. The Shop renders unavailable
products clearly when payment services are not configured.

Production retains quantity modes 1/10/100/Max/milestone, pause and reserves.
Prestige displays reward, reset and retained state before confirmation. Challenge
entry/restart/exit use confirmations. Offline summaries retain all discovered
currencies and the before/after/production/spending/reset/reward breakdown.

## Pencil document

MCP `pencil` was connected and the active document was
`Design/CelestialMaker-UI.pen`. It was saved through Pencil's Save action.
The saved file is 93,096 bytes; final MCP readback confirmed all five top-level
frames and no reported clipped-node layout problems.

| Frame | Node | Size |
|---|---|---|
| Celestial Maker · Start panel | bi8Au | 600×400 |
| Main Menu PC | WxN5Q | 1600×900 |
| Overview PC | Va1bB | 1600×900 |
| Production PC | nSbiA | 1600×900 |
| Prestige confirmation PC | Q0WkmA | 1600×900 |

The original panel has the exact text `Celestial Maker` and `Start Game`, plus
the cosmic illustration. Exports: `Docs/DesignEvidence/bi8Au.png` and
`Docs/UIOverhaul/Pencil/*.png`. The later Unity layouts follow this direction.

## Reusable style and modified files

Paths below are relative to `Game/Assets/_CelestialMaker/` unless stated otherwise.

| File/asset | Purpose |
|---|---|
| `Scripts/UI/GameUI.cs` | Routes landscape PC presentation to the new partial class; legacy layout remains for Web/mobile. |
| `Scripts/UI/GameUI.PC.cs` | Shared PC shell, typography, cards, buttons, scrolling, screens, dialogs and binding helpers. |
| `Scripts/UI/GameUI.PCInsights.cs` | Follow-up production diagnosis, research filtering and next-horizon progress. |
| `Scripts/UI/CosmicStyle.cs` | Palette and six chapter identities. |
| `Scripts/UI/CosmicPanel.cs` | Scalable beveled panel mesh with subtle shading and border. |
| `Scripts/UI/CosmicArtwork.cs` | Aspect-preserving crop of the cosmic background. |
| `Scripts/UI/GameTooltip.cs` | Styled, content-sized tooltip clamped within the canvas. |
| `Scripts/UI/PcUiReview.cs` | Explicitly opt-in Development/Editor acceptance fixture. Excluded from normal Release. |
| `Editor/PcUiAssets.cs` | Unity Editor asset creation/import and dedicated PC Development/Release build. |
| `Resources/UI/PC/Panel.prefab` | Reusable panel prefab made with PrefabUtility. |
| `Resources/UI/PC/DisplayFont.asset`, `BodyFont.asset` | Rajdhani headings and IBM Plex Sans body TMP assets generated through Editor APIs. |
| `Resources/UI/PC/CosmicBackdrop.png` | Imported Pencil illustration. |
| `Art/Fonts/*` | Original font sources and OFL license texts. |
| `Game/ProjectSettings/ProjectSettings.asset` (root-relative) | 1920×1080 default, resizable windowed PC presentation. |

No scene YAML was rewritten. Existing `00_Bootstrap`, `01_MainMenu` and `02_Game`
scenes continue using their GameUI component, which constructs the new runtime UI.
Unity generated/preserved asset metadata. `Tools/UIOverhaul/*.before.cs.txt`
preserves the original UI source for reference. Root README, PROJECT_STATE and
this report now identify the new build; older Web/Windows packages are retained.

Style rules: 18-unit body text, 14–16-unit captions/secondary text, Rajdhani display
headings, minimum 42-unit buttons, 20-unit card padding, 14-unit grid gaps,
gold action hierarchy, cyan selected borders and readable disabled labels.
Chapter numbers and color accents provide a compact identity without introducing
an unrelated icon library. Cosmic art is kept behind uncluttered content.

## Researched improvements implemented after the first review

See [RESEARCH.md](RESEARCH.md) for sources and reasoning. The added features are:

- Production diagnosis distinguishes full output, input constraints, pause and
  unowned processes; shows actual/capacity, input stock and reserve; routes to
  the input's production chapter. Values in the dialog are a snapshot at opening.
- Research filters All / Not owned / Purchased, with live ready/acquired counts.
- Exact next-chapter lifetime requirement and progress bar in Overview.
- Visible reserve On/Off state and corrected scrolling after text-scale rebuild.

## Focused runtime validation

| Resolution | Checks | Runtime errors | TMP overflow candidates |
|---|---:|---:|---:|
| 1920×1080 | 40 PASS / 0 FAIL | 0 | 0 |
| 2560×1440 | 40 PASS / 0 FAIL | 0 | 0 |
| 3440×1440 | 40 PASS / 0 FAIL | 0 | 0 |

Checks cover: exact render size, passive simulation, buying, research purchase,
filters, all primary screens, tooltips, scrolling, constrained/healthy production,
diagnosis navigation, modal raycasts and callback blocking, export/valid import,
invalid import rejection, prestige confirm/cancel, challenge start/restore,
expedition launch, offline report, large numbers, 125% text, valid save and Continue.

Tests run actual Player UI callbacks; scrolling and hover use EventSystem handler
events. They do not claim a manual physical-mouse playthrough. Screenshot review
covered menu, early/late overview and production, research, prestige confirmation,
offline report, import, large values, enlarged text and the other primary screens.
Screenshots are unaltered Unity ScreenCapture output; a parked mouse causes a
normal Codex tooltip in some 1440p/ultrawide shots.

Evidence: `Logs/UIOverhaul/{resolution}/result.json`, `player-{resolution}.log`,
`build.json`, `build-release.json`, `release-startup.json` and
`player-release-startup.log`. Initial failures (incorrect panel component filename)
are retained separately as historical evidence; the corrected final runs above
are the acceptance results. Builds ran sequentially in the pinned editor.

Useful screenshot paths (repository-relative):

- `Logs/UIOverhaul/1920x1080/01-main-menu.png`
- `Logs/UIOverhaul/1920x1080/02-overview-early.png`
- `Logs/UIOverhaul/3440x1440/late-production.png`
- `Logs/UIOverhaul/1920x1080/13-research-filter.png`
- `Logs/UIOverhaul/1920x1080/14-production-diagnosis.png`
- `Logs/UIOverhaul/1920x1080/07-prestige-confirmation.png`
- `Logs/UIOverhaul/1920x1080/08-offline-summary-modal.png`
- `Logs/UIOverhaul/1920x1080/11b-large-text-overview.png`

## Scope and remaining polish

This is PC UI acceptance. No unrelated full-game regression, Web/Android rebuild,
long-session performance soak, payment purchase or provider sign-in test is claimed.
Account/payment services remain unconfigured in the local player. The dedicated
Release contains no default backend URL; the source config is restored after build.
No publishing, remote Git or production signing was performed.

Optional future polish: chapter-specific illustrations/icons, full keyboard focus
navigation/auto-scroll, and human long-session comfort testing. The existing theme
setting affects the underlying cosmos; the new readable UI palette stays consistent.
