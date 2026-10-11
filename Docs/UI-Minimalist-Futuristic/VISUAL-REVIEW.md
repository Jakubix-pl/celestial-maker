# Observatory 2.2.1 — visual review, 2026-10-11

## Result

IMPLEMENTED: fixed navigation word wrapping at 200% text, control contrast and
focus/selection borders, nested scroll areas in absence and parallel-universe
dialogs, long JSON escaping input fields, and overly wide layouts on ultrawide
displays. The menu orbital mark now explicitly requires its CanvasRenderer;
thicker, denser line geometry avoids broken thin arcs. All gameplay Core files
remain byte-identical to the 2.2.0 backup.

COMPILED: Unity 6000.6.3f1; 117/117 Core tests, 20/20 Unity serialization/Core
checks, 1359/1359 UI checks. Five added regressions cover navigation labels at
200%, single-scroll dialogs, input clipping, both themes' button text contrast
in five states (at least 4.5:1), and bounded ultrawide width. No failed checks.

BUILT: Windows Release 2.2.1, zero build errors. The actual release executable
also passed 33 checks with `-batchmode -nographics`, an isolated save and no
runtime errors. ZIP contents were hash-compared against all 191 packaged files.

OBSERVED: actual Unity uGUI/TMP rendering on an NVIDIA GeForce RTX 4070 Ti,
using the project's Desktop Universal Render Pipeline and PC quality setting.
247 final PNGs are in `Logs/ObservatoryVisual/after`. These are generated from
real GameUI code, not design mockups. The [gallery](VISUAL-GALLERY.html) indexes
every final image, including full-resolution links and a name filter.

No desktop input, screen capture, window manipulation, Play Mode, or visible
game/editor window was used. Processes ran sequentially with `Start-Process
-WindowStyle Hidden`. Visual review uses a private orthographic camera and a
world-space fixture canvas at a one-pixel UI scale, rendered to RenderTexture.
The normal player uses its ordinary scene/canvas. Physical input, OS display
scaling, live animation and graphical player FPS remain UNVERIFIED. Static
offscreen renders and the separate null-graphics player test do not certify
those properties.

## Findings and corrections

| Finding | Correction | Evidence |
|---|---|---|
| At 1280x720 / 200%, navigation split `Overview`, `Production` and `Observatory` inside words. | Navigation width follows the text scale; full words fit without shrinking text. | `before/late-night-1280-200-production.png`, `after/late-night-1280-200-production.png` |
| Disabled primary buttons used dim turquoise behind dark text. Multiplicative tints also weakened Dawn states. | Explicit foreground/background pairs for normal, hover, pressed, selected and disabled controls. Disabled actions use neutral surfaces; selection has a two-pixel outline. | `after/control-night-*.png`, `after/control-dawn-*.png`; contrast regression |
| Absence dialog exposed only 100 px of a nested report, leaving most of the dialog empty. | One full-height dialog scroll, with Continue and Details at the top. | `before/modal-1280-100-offline.png`, `after/modal-1280-100-offline.png` |
| Parallel economy dialog similarly restricted content to 160 px. | One scroll for all process, law and transfer controls; Close is available at both ends. | `after/modal-1280-100-universe.png` |
| Save/profile JSON fields lacked their own clip mask. | RectMask2D clips editable text to the field; long populated saves/profiles were rendered. | `after/modal-1280-200-import-filled.png`, `after/modal-1280-200-strategy-filled.png` |
| At 3440 px, short text rows and controls stretched nearly across the screen. | Centered workspace capped at 2560 px at 100% text, increasing for larger text. | `after/wide-night-3440-*.png` |
| Orbital lines were too thin at the ellipse tips. | Increased stroke width and segment count. | `after/early-night-1920-menu.png` |

The rendering harness initially used the editor's Mobile quality and its Polish
culture. Final exports explicitly use Desktop/PC and en-US, matching runtime
choices. Before images are useful for geometry comparisons, not a pipeline or
locale comparison. The harness restores editor quality and culture on exit.

## Review coverage

The visual review covered all 20 named views, including locked/early and
unlocked/late content: Overview, Production, Research, Upgrades, Prestige,
Trees, Constellations, Challenges, Expeditions, Achievements, Codex, Settings,
Offline summary, Analytics, Automation, Roadmap, Megastructures, Reality laws,
Parallel universes and Credits. Main menu, Continue menu, process diagnostics,
reset confirmation, economy selection, universe details, strategy/save import,
absence dialog, active challenge and save-failure message were also rendered.

All named views have Night/Dawn late-game renders at 1920x1080 / 100%, early
Night at 1280x720 / 100%, large Dawn at 1280x720 / 200%, and QHD at 2560x1440 /
125%. Long lists include middle/bottom captures at 1920 and bottom captures at
200%. Four representative ultrawide views use 3440x1440. Dialogs include
1280x720 at 100% and 200%; the reset preview also covers Dawn. Representative
button states were rendered in both themes.

Contact sheets were inspected across all named screens; full-size views were
used for navigation, production, tree columns, menus, modal text, filled import,
and ultrawide layout. In the inspected images, labels remain legible and controls
do not overlap their text. Clipping at the top/bottom of a scroll viewport is
intentional; at 200% on a 720 px display, substantial scrolling is expected.
This is a review of sampled states, not proof of every possible save/input.

The broader geometric suite still covers 19 routes plus menu/reset across early
and late state, both themes, four resolutions, and 100/125/150/200% text. Five
specific new checks complement it. Pixel integrity checks decode all final PNGs,
check dimensions/nonuniformity and record SHA256; they do not score readability.

## Files and reproduction

- Build: `Builds/Windows/Observatory-Release/CelestialMaker.exe`.
- Package: `Builds/CelestialMaker-Observatory-Windows.zip`, 40,983,739 bytes.
- ZIP SHA256: `743973BB67AFE1BF97A031328CB78DE218A5BEEA655A3C68BC706BAF57FB70E1`.
- Test/build evidence: `Logs/Observatory/{core-tests,unity-core-tests,ui-headless-tests,build-release,player-headless,artifact}.json`.
- Render evidence: `Logs/ObservatoryVisual/after/render-manifest.json`,
  `after-unity.log`, `after-image-integrity.json`, `sheets-after/`.
- Preservation: `Logs/ObservatoryVisual/save-integrity.json` (5 unchanged user
  JSON files); `core-integrity.json` (28 unchanged Core source/meta files).
- Backup: `Backups/ObservatoryVisual-20261011-104846`, including previous ZIP.

Run `Tools/render-observatory.ps1 -After` for offscreen GPU images, then
`python Tools/review-observatory-images.py` (Pillow required) for the gallery.
Run `Tools/build-observatory.ps1` for sequential Core/UI checks, build, actual
null-graphics release test and packaging. Rendering intentionally omits
`-nographics` while retaining `-batchmode`; ordinary tests/builds use both.
If Unity holds the project lock, these scripts defer instead of closing it.

No balance/progression change, save migration, website update, remote Git,
publication or unrelated project changes were made in this pass. The existing
400-hour pacing calibration remains the basis; no new pacing measurement is
claimed by this visual review.
