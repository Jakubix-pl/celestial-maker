"""Verify and record the resource-header release from actual build evidence."""
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

root = Path(__file__).resolve().parents[2]
out = root / "Logs/ObservatoryResources"


def read(path):
    return json.loads((root / path).read_text(encoding="utf-8-sig"))


artifact = read("Logs/Observatory/artifact.json")
build = read("Logs/Observatory/build-release.json")
core = read("Logs/Observatory/core-tests.json")
unity = read("Logs/Observatory/unity-core-tests.json")
ui = read("Logs/Observatory/ui-headless-tests.json")
player = read("Logs/Observatory/player-headless.json")
render = read("Logs/ObservatoryVisual/after/render-manifest.json")
review = read("Logs/ObservatoryResources/visual-review.json")
integrity = read("Logs/ObservatoryVisual/after-image-integrity.json")
before = read("Logs/Observatory/saves-before.json")
saves = read("Logs/Observatory/save-integrity.json")
backup = (out / "backup-path.txt").read_text(encoding="utf-8-sig").strip()
assert artifact["version"] == player["version"] == render["version"] == "2.4.2"
assert build["result"] == "Succeeded" and build["errors"] == 0
assert core["fail"] == unity["fail"] == ui["fail"] == 0
assert player["pass"] and not player["errors"]
assert all(item["unchanged"] for item in saves)
assert all(hashlib.sha256(Path(item["path"]).read_bytes()).hexdigest().upper() == item["sha256"] for item in before)
assert len(render["files"]) == integrity["count"] == 24
zip_path = Path(artifact["path"])
assert hashlib.sha256(zip_path.read_bytes()).hexdigest().upper() == artifact["sha256"]
latest_source = max(p.stat().st_mtime for p in (root / "Game/Assets/_CelestialMaker/Scripts").rglob("*.cs"))
assert latest_source < datetime.fromisoformat(build["utc"].replace("Z", "+00:00")).timestamp()
assert latest_source < datetime.fromisoformat(render["utc"].replace("Z", "+00:00")).timestamp()
evidence = dict(version="2.4.2", utc=datetime.now(timezone.utc).isoformat(),
                core=core["pass"], unityCore=unity["pass"], ui=ui["pass"],
                playerChecks=len(player["checks"]), build=build, artifact=artifact,
                render=render, visualReview=review, userFilesUnchanged=len(saves),
                sourcesPredateBuildAndRender=True, backup=backup,
                physicalInput="UNVERIFIED", windowsDpi="UNVERIFIED",
                visiblePlayerFps="UNVERIFIED", newPerformanceBenchmark=False)
(out / "final-verification.json").write_text(json.dumps(evidence, indent=2), encoding="utf-8")

report = f"""Celestial Maker 2.4.2 — resource header, 2026-10-11

IMPLEMENTED
Top-bar resources have their own colored names, small vector icons, subtle
tinted fields and left accent lines. Numeric stock values retain neutral,
high-contrast text. Negative NET values retain their minus sign and warning color.
Energy: amber / bolt. Particles: blue / connected particles.
Atoms: teal / atom. Planetary mass: green / ringed planet.
Starlight: peach / star. Gravity: rose / concentric field.
Spacetime: cyan / hourglass. Prestige Points: violet / diamond emblem.

The existing responsive meter count remains: four fields at sufficient width,
two at narrow widths or large text scale. Names are retained alongside icons.
The third field changes name, icon, tint and amount together as chapters change
or reset. The selected universe controls local resources; global pages use home.
Prestige Points read the live home balance directly, avoiding a stale satellite
copy. All stock counters still refresh on each presentation frame. Icon geometry
updates only when its resource identity changes; no new Update loop or assets
loaded per frame. Resource resolution no longer allocates a LINQ enumeration.
No balance constants, save formats or authoritative simulation logic changed.

COMPILED
Core {core['pass']}/{core['pass']}, Unity Core {unity['pass']}/{unity['pass']},
UI {ui['pass']}/{ui['pass']}; actual release EXE {len(player['checks'])} null-graphics checks.
Targeted resource checks cover chapter/reset/universe transitions, live home
prestige, text contrast >=4.5:1, icon contrast >=3:1, contained header geometry,
and 1e300 values at 960, 1280, 1500, 1920 and 3840 widths in both themes.
Existing checks verify 60 consecutive presentation updates from authoritative
stock; these are frame-update checks, not a measured desktop frame rate.

BUILT
Windows Release 2.4.2, zero errors, Unity {build['unity']}.
Build UTC: {build['utc']}
ZIP: {artifact['path']}
Bytes: {artifact['bytes']}; verified archive files: {artifact['verifiedEntries']}.
SHA256: {artifact['sha256']}
All {len(saves)} protected user files retained their initial hashes.

OBSERVED
24 private Unity URP renders; six full-resolution images and one contact sheet
manually inspected, listed in Logs/ObservatoryResources/visual-review.json.
The image matrix covers Night/Dawn, chapter identities, negative NET, selected
parallel economy, 100/125/200 percent scales and 960-3840 pixel widths. Labels,
icons and amounts remain separated in the inspected views.
Render UTC: {render['utc']}
No desktop control, desktop screenshots or visible game/editor windows.
Physical input, Windows DPI and visible-player FPS remain UNVERIFIED.
No new performance benchmark was run for this header change.

Report evidence: Logs/ObservatoryResources/final-verification.json.
Gallery: Docs/UI-Minimalist-Futuristic/VISUAL-GALLERY.html.
Previous release, sources and evidence backup: {backup}
"""
(root / "Docs/UI-Minimalist-Futuristic/RESOURCES-RELEASE.txt").write_text(report, encoding="utf-8")
state_path = root / "PROJECT_STATE.md"
state = state_path.read_text(encoding="utf-8-sig")
heading = "## Current 2.4.1 Observatory center tabs — 2026-10-11"
assert heading in state, "Expected previous release heading"
new = f"""## Current 2.4.2 Observatory resource header — 2026-10-11

IMPLEMENTED: stable resource colors, vector pictograms, tinted header fields
and accent lines; neutral stock values, preserved negative NET warning.
Resource identities follow chapter, reset and selected-economy changes together.
Home Prestige Points read live home state; counters retain per-frame updates.
COMPILED: Core {core['pass']}/{core['pass']}, Unity Core {unity['pass']}/{unity['pass']},
UI {ui['pass']}/{ui['pass']}; actual EXE {len(player['checks'])} null-graphics checks passed.
BUILT: Windows Release 2.4.2, zero errors; ZIP {artifact['bytes']:,} bytes,
{artifact['verifiedEntries']} verified files, SHA256 {artifact['sha256']}.
OBSERVED: 24 private Unity URP renders; six full-resolution images and a contact
sheet inspected. No desktop control/capture or visible game window.
Physical input, Windows DPI and visible-player FPS remain UNVERIFIED;
no new FPS benchmark. All {len(saves)} protected user files unchanged.

Report: Docs/UI-Minimalist-Futuristic/RESOURCES-RELEASE.txt.
Evidence: Logs/ObservatoryResources/final-verification.json.
Prior 2.4.1 release, sources and evidence: {Path(backup).relative_to(root).as_posix()}.

## Previous 2.4.1 Observatory center tabs — 2026-10-11"""
state_path.write_text(state.replace(heading, new, 1), encoding="utf-8")
print(json.dumps({"version": "2.4.2", "verified": True, "ui": ui["pass"],
                  "player": len(player["checks"]), "renders": len(render["files"]),
                  "userFilesUnchanged": len(saves), "zip": str(zip_path)}))
