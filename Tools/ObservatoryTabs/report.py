"""Record the scoped center-tab release only after fresh evidence is available."""
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

root = Path(__file__).resolve().parents[2]
out = root / "Logs/ObservatoryTabs"


def read(relative):
    return json.loads((root / relative).read_text(encoding="utf-8-sig"))


artifact = read("Logs/Observatory/artifact.json")
build = read("Logs/Observatory/build-release.json")
core = read("Logs/Observatory/core-tests.json")
unity = read("Logs/Observatory/unity-core-tests.json")
ui = read("Logs/Observatory/ui-headless-tests.json")
player = read("Logs/Observatory/player-headless.json")
render = read("Logs/ObservatoryVisual/after/render-manifest.json")
review = read("Logs/ObservatoryTabs/visual-review.json")
integrity = read("Logs/ObservatoryVisual/after-image-integrity.json")
saves = read("Logs/Observatory/save-integrity.json")
backup = (out / "backup-path.txt").read_text(encoding="utf-8-sig").strip()

assert artifact["version"] == player["version"] == render["version"] == "2.4.1"
assert build["result"] == "Succeeded" and build["errors"] == 0
assert core["fail"] == unity["fail"] == ui["fail"] == 0
assert player["pass"] and not player["errors"]
assert all(item["unchanged"] for item in saves)
assert len(render["files"]) == integrity["count"] == 32
zip_path = Path(artifact["path"])
assert hashlib.sha256(zip_path.read_bytes()).hexdigest().upper() == artifact["sha256"]
source_files = list((root / "Game/Assets/_CelestialMaker/Scripts").rglob("*.cs"))
latest_source = max(p.stat().st_mtime for p in source_files)
assert latest_source < datetime.fromisoformat(build["utc"].replace("Z", "+00:00")).timestamp()
assert latest_source < datetime.fromisoformat(render["utc"].replace("Z", "+00:00")).timestamp()

evidence = dict(version="2.4.1", utc=datetime.now(timezone.utc).isoformat(),
                core=core["pass"], unityCore=unity["pass"], ui=ui["pass"],
                playerChecks=len(player["checks"]), build=build, artifact=artifact,
                render=render, visualReview=review, userFilesUnchanged=len(saves),
                sourcesPredateBuildAndRender=True, backup=backup,
                physicalInput="UNVERIFIED", windowsDpi="UNVERIFIED",
                visiblePlayerFps="UNVERIFIED", newPerformanceBenchmark=False)
(out / "final-verification.json").write_text(json.dumps(evidence, indent=2), encoding="utf-8")

text = f"""Celestial Maker 2.4.1 — center view tabs, 2026-10-11

IMPLEMENTED
Individual color accents, subtle tinted fills and small vector pictograms for
the center tabs in Production, Development, Cosmos and Journal. Active tabs
also have bold labels and an underline. The compact current-view control and
Views dialog use the same colors/icons. Padding follows the text scale, and
existing wrapping and scrolling remain available. No per-frame icon logic.

Production: Processes blue, Research violet, Upgrades amber, Flows mint.
Development: Resets violet, Trees & profiles mint, Milestones blue, Challenges rose.
Cosmos: Megastructures blue, Reality laws violet, Universes mint, Connections
amber, Expeditions rose. Journal: Achievements amber, Codex blue, Last absence mint.
Colors have separate dark and light theme values. Existing English text,
game mechanics, saves and custom executable/shortcut icon are retained.

COMPILED
Core {core['pass']}/{core['pass']}, Unity Core {unity['pass']}/{unity['pass']},
UI {ui['pass']}/{ui['pass']}. The extended existing contrast check covers text
at least 4.5:1 and tab pictograms at least 3:1 across enabled control states
in both themes. Actual release EXE: {len(player['checks'])} isolated null-graphics checks.
An initial editor-test brace error was corrected before these successful runs.

BUILT
Windows Release 2.4.1, zero build errors; Unity {build['unity']}.
Build UTC: {build['utc']}
ZIP: {artifact['path']}
Bytes: {artifact['bytes']}; verified archive entries: {artifact['verifiedEntries']}.
SHA256: {artifact['sha256']}
All {len(saves)} protected user files retained their initial hashes.

OBSERVED
32 saved Unity URP renders of the actual UI, with six representative images
manually inspected (listed in Logs/ObservatoryTabs/visual-review.json).
Checks include Night/Dawn, 100/200 percent scale, wrapped Cosmos tabs at
1280x720 and compact Views at 960x720. Captions and icons remain separated;
selection is visible without relying only on color. Image integrity is checked
separately and is not an automated readability verdict.
Render UTC: {render['utc']}
No desktop input, desktop capture or visible game/editor window was used.
Physical input, Windows DPI and visible-player FPS remain UNVERIFIED.
No new FPS measurement was needed for this static visual change; previous
2.4.0 performance evidence must not be presented as a new 2.4.1 benchmark.

Evidence: Logs/ObservatoryTabs, Logs/Observatory, Logs/ObservatoryVisual.
Gallery: Docs/UI-Minimalist-Futuristic/VISUAL-GALLERY.html.
Previous release/source/evidence backup: {backup}
"""
(root / "Docs/UI-Minimalist-Futuristic/TABS-RELEASE.txt").write_text(text, encoding="utf-8")

state_path = root / "PROJECT_STATE.md"
state = state_path.read_text(encoding="utf-8-sig")
heading = "## Current 2.4.0 Observatory readability — 2026-10-11"
assert heading in state, "Expected previous release heading"
new = f"""## Current 2.4.1 Observatory center tabs — 2026-10-11

IMPLEMENTED: distinct center-tab colors and vector icons, subtle tinted fills,
bold active labels and underlines; matching compact Views controls in both themes.
COMPILED: Core {core['pass']}/{core['pass']}, Unity Core {unity['pass']}/{unity['pass']},
UI {ui['pass']}/{ui['pass']}; actual EXE {len(player['checks'])} null-graphics checks passed.
BUILT: Windows Release 2.4.1, zero errors; ZIP {artifact['bytes']:,} bytes,
{artifact['verifiedEntries']} verified files, SHA256 {artifact['sha256']}.
OBSERVED: 32 private Unity URP renders; six representative images inspected,
including 100/200 percent and narrow Views layouts. No desktop control/capture
or visible game window. Physical input, Windows DPI and visible-player FPS
remain UNVERIFIED; no new FPS benchmark for this static tab update.
All {len(saves)} protected user files unchanged.

Report: Docs/UI-Minimalist-Futuristic/TABS-RELEASE.txt.
Evidence: Logs/ObservatoryTabs/final-verification.json.
Prior 2.4.0 sources, package and evidence: {Path(backup).relative_to(root).as_posix()}.

## Previous 2.4.0 Observatory readability — 2026-10-11"""
state_path.write_text(state.replace(heading, new, 1), encoding="utf-8")
print(json.dumps({"version": "2.4.1", "verified": True, "ui": ui["pass"],
                  "player": len(player["checks"]), "renders": len(render["files"]),
                  "userFilesUnchanged": len(saves), "zip": str(zip_path)}))
