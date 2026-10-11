"""Verify the currency-discovery release against fresh artifacts."""
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

root = Path(__file__).resolve().parents[2]
out = root / "Logs/ObservatoryUnlocks"


def read(path):
    return json.loads((root / path).read_text(encoding="utf-8-sig"))


artifact = read("Logs/Observatory/artifact.json")
build = read("Logs/Observatory/build-release.json")
core = read("Logs/Observatory/core-tests.json")
unity = read("Logs/Observatory/unity-core-tests.json")
ui = read("Logs/Observatory/ui-headless-tests.json")
player = read("Logs/Observatory/player-headless.json")
render = read("Logs/ObservatoryVisual/after/render-manifest.json")
review = read("Logs/ObservatoryUnlocks/visual-review.json")
integrity = read("Logs/ObservatoryVisual/after-image-integrity.json")
before = read("Logs/Observatory/saves-before.json")
backup = (out / "backup-path.txt").read_text(encoding="utf-8-sig").strip()
assert artifact["version"] == player["version"] == render["version"] == "2.4.3"
assert build["result"] == "Succeeded" and build["errors"] == 0
assert core["fail"] == unity["fail"] == ui["fail"] == 0
assert player["pass"] and not player["errors"]
assert all(hashlib.sha256(Path(item["path"]).read_bytes()).hexdigest().upper() == item["sha256"] for item in before)
assert len(render["files"]) == integrity["count"] == 14
assert hashlib.sha256(Path(artifact["path"]).read_bytes()).hexdigest().upper() == artifact["sha256"]
latest_source = max(p.stat().st_mtime for p in (root / "Game/Assets/_CelestialMaker/Scripts").rglob("*.cs"))
assert latest_source < min(datetime.fromisoformat(item["utc"].replace("Z", "+00:00")).timestamp() for item in (build, render))
evidence = dict(version="2.4.3", utc=datetime.now(timezone.utc).isoformat(),
                core=core["pass"], unityCore=unity["pass"], ui=ui["pass"],
                playerChecks=len(player["checks"]), build=build, artifact=artifact,
                render=render, visualReview=review, userFilesUnchanged=len(before),
                sourcesPredateBuildAndRender=True, backup=backup,
                physicalInput="UNVERIFIED", windowsDpi="UNVERIFIED",
                visiblePlayerFps="UNVERIFIED", newPerformanceBenchmark=False)
(out / "final-verification.json").write_text(json.dumps(evidence, indent=2), encoding="utf-8")

report = f"""Celestial Maker 2.4.3 — progressive currency visibility, 2026-10-11

IMPLEMENTED
Prestige, Ascension and Genesis currency details become visible when the first
corresponding reset can award a point, or persisted progression already records
the currency (balance, current/higher reset, historical earned points or resets).
Spending a balance to zero, a higher reset and loading a save do not erase known
currencies. No new save field or migration was needed; authoritative progression
and balance rules remain unchanged.

Fresh top bars show Energy and Particles. The chapter-resource meter stays hidden
until that resource's chapter is open or its stock/run lifetime is nonzero; the
Prestige meter follows currency discovery. Hidden fields leave no empty slots.
Visibility follows the selected economy; reset currency belongs to home.
Stock counters still refresh per rendered frame; header visibility is checked
in the existing 250 ms derived-refresh path, without an extra Update loop.

Locked reset cards keep a name, Locked state and relevant unlock requirements,
without a currency balance or reward preview. Review becomes available on unlock.
Requirements mentioning an unknown lower currency use an introductory goal until
that currency is known. Roadmap, Genesis horizon and specialization views follow
the same rule. The legacy reset/specialization presentation uses the shared guard.
Existing offline report discovery filtering is retained.

COMPILED
Core {core['pass']}/{core['pass']}, Unity Core {unity['pass']}/{unity['pass']},
UI {ui['pass']}/{ui['pass']}; actual EXE {len(player['checks'])} null-graphics checks.
New fixtures cover fresh screens without unknown currency labels, collapsed
meter gaps, first available Prestige/Ascension/Genesis, earning and spending the
first Prestige point, JSON reload, actual Ascension clearing the lower balance,
historical metadata, imported balances and selected parallel economies.
Existing layout, contrast and consecutive-frame counter checks also pass.

BUILT
Windows Release 2.4.3, zero errors, Unity {build['unity']}.
Build UTC: {build['utc']}
ZIP: {artifact['path']}
Bytes: {artifact['bytes']}; verified files: {artifact['verifiedEntries']}.
SHA256: {artifact['sha256']}
All {len(before)} protected user files retained their initial hashes.

OBSERVED
14 private Unity URP renders in Night/Dawn. Six representative images inspected:
fresh production/reset views, ready Prestige, spent Prestige, ready Ascension,
and fresh 200-percent text. The newly hidden fields collapse correctly; discovered
zero balances remain shown. The legacy presentation was compiled, not rendered.
Render UTC: {render['utc']}
No desktop control, desktop screenshots or visible game/editor windows.
Physical input, Windows DPI and visible-player FPS remain UNVERIFIED.
No new FPS benchmark was run for this UI visibility change.

Evidence: Logs/ObservatoryUnlocks/final-verification.json.
Gallery: Docs/UI-Minimalist-Futuristic/VISUAL-GALLERY.html.
Previous release, sources and evidence: {backup}
"""
(root / "Docs/UI-Minimalist-Futuristic/UNLOCKS-RELEASE.txt").write_text(report, encoding="utf-8")
state_path = root / "PROJECT_STATE.md"
state = state_path.read_text(encoding="utf-8-sig")
heading = "## Current 2.4.2 Observatory resource header — 2026-10-11"
assert heading in state
new = f"""## Current 2.4.3 Observatory currency visibility — 2026-10-11

IMPLEMENTED: undiscovered resource/reset-currency fields stay hidden; reset
currencies appear at first available reward or prior recorded discovery, and
remain visible at zero after spending, higher resets and reload. Locked reset
cards retain unlock guidance. No save-schema or balance changes.
COMPILED: Core {core['pass']}/{core['pass']}, Unity Core {unity['pass']}/{unity['pass']},
UI {ui['pass']}/{ui['pass']}; actual EXE {len(player['checks'])} null-graphics checks passed.
BUILT: Windows Release 2.4.3, zero errors; ZIP {artifact['bytes']:,} bytes,
{artifact['verifiedEntries']} verified files, SHA256 {artifact['sha256']}.
OBSERVED: 14 private Unity URP renders, six representative views inspected.
No desktop control/capture or visible game window; all {len(before)} protected files
unchanged. Physical input, Windows DPI and visible-player FPS remain UNVERIFIED.

Report: Docs/UI-Minimalist-Futuristic/UNLOCKS-RELEASE.txt.
Evidence: Logs/ObservatoryUnlocks/final-verification.json.
Prior 2.4.2 release, sources and evidence: {Path(backup).relative_to(root).as_posix()}.

## Previous 2.4.2 Observatory resource header — 2026-10-11"""
state_path.write_text(state.replace(heading, new, 1), encoding="utf-8")
print(json.dumps({"version": "2.4.3", "verified": True, "ui": ui["pass"],
                  "player": len(player["checks"]), "renders": len(render["files"]),
                  "userFilesUnchanged": len(before), "zip": artifact["path"]}))
