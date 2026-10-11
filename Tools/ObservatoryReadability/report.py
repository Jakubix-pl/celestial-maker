"""Summarize measured, isolated Observatory verification. Never runs the player."""
from pathlib import Path
import json, hashlib, datetime

root=Path(__file__).resolve().parents[2]
def read(path): return json.loads((root/path).read_text(encoding='utf-8-sig'))
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest().upper()
core=read('Logs/Observatory/core-tests.json')
unity=read('Logs/Observatory/unity-core-tests.json')
ui=read('Logs/Observatory/ui-headless-tests.json')
player=read('Logs/Observatory/player-headless.json')
artifact=read('Logs/Observatory/artifact.json')
build=read('Logs/Observatory/build-release.json')
render=read('Logs/ObservatoryVisual/after/render-manifest.json')
integrity=read('Logs/ObservatoryVisual/after-image-integrity.json')
perf=read('Logs/ObservatoryPerformance/after.json')
for result in (core,unity,ui): assert result['fail']==0,result
assert player['pass'] and player['version']==artifact['version']==render['version']==perf['version']=='2.4.0'
assert build['errors']==0 and build['result']=='Succeeded'
assert integrity['count']==len(render['files'])
for entry in integrity['images']: assert sha(root/'Logs/ObservatoryVisual/after'/entry['file']).lower()==entry['sha256']
assert sha(Path(artifact['path']))==artifact['sha256']
saves=read('Logs/ObservatoryReadability/saves-before.json')
for save in saves: assert sha(Path(save['path']))==save['sha256']
build_time=datetime.datetime.fromisoformat(build['utc'].replace('Z','+00:00')).timestamp()
runtime=list((root/'Game/Assets/_CelestialMaker/Scripts').rglob('*.cs'))
assert all(p.stat().st_mtime<=build_time for p in runtime), 'Source changed since build'
previous=read('Backups/ObservatoryReadability-20261011-115344/Logs/ObservatoryPerformance/after.json')
frames=sum(r['frames'] for r in perf['results']); over=sum(r['overBudget'] for r in perf['results'])
evidence={'version':'2.4.0','utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'core':core['pass'],'unity':unity['pass'],'ui':ui['pass'],'player':len(player['checks']),'renders':integrity['count'],'performanceCases':len(perf['results']),'measuredFrames':frames,'framesOver16_67ms':over,'userFilesUnchanged':len(saves),'sourcePredatesBuild':True,'packageSha256':artifact['sha256'],'packageBytes':artifact['bytes'],'packageVerifiedFiles':artifact['verifiedEntries'],'desktopControlled':False,'visibleGameWindow':False}
(root/'Logs/ObservatoryReadability/final-verification.json').write_text(json.dumps(evidence,indent=2),encoding='utf-8')

table=['| Case | UI mean, ms | Complete frame p95, ms | p99, ms | Maximum, ms | >16.67 ms |','|---|---:|---:|---:|---:|---:|']
for r in perf['results']:table.append(f"| {r['page']} | {r['uiMeanMs']:.3f} | {r['p95Ms']:.3f} | {r['p99Ms']:.3f} | {r['maxMs']:.3f} | {r['overBudget']} |")
evidence_text=f'''COMPILED: Core **{core['pass']}/{core['pass']}**, Unity Core
**{unity['pass']}/{unity['pass']}**, UI **{ui['pass']}/{ui['pass']}**.
The actual Windows EXE passed **{len(player['checks'])}** null-graphics runtime checks,
including debounced search, favorite/goal/queue callbacks and save/reload.

BUILT: Windows Release **2.4.0**, **0 build errors**. ZIP:
`Builds/CelestialMaker-Observatory-Windows.zip`, {artifact['bytes']:,} bytes,
{artifact['verifiedEntries']} archive entries hash-compared with the build folder.
SHA256: `{artifact['sha256']}`.
Existing desktop shortcut still targets this EXE and its custom ICO.

OBSERVED: **{integrity['count']} actual Unity URP renders**, including both themes,
early/late progression, 960/1280/1920/2560/3440-pixel-wide samples, 100/125/200%
text, dialogs, scrolling, expanded research, pinned goal, search, queue and compact
filters. PNG decoding, dimensions and hashes passed; those checks alone are not a
visual readability verdict. Direct inspection covered representative screens,
both themes, compact mode, expanded content and dialogs. The automated geometry
matrix also covers 150% text and all original routes.

Core queue tests compare a large offline span with one-second simulation steps
for both home and parallel economies, and test decision timing with every other
automation disabled. An initial audit found a missing queue decision boundary;
the final engine includes it. Queue defaults preserve the previous behavior.

**{len(saves)} existing user files remain hash-identical.** Test saves are isolated.
Progression constants match the previous release. Runtime sources predate the
final successful build. Final evidence: `Logs/ObservatoryReadability/final-verification.json`.

### Offscreen performance

{perf['device']}; {perf['cpu'].strip()}. Late-game fixture, 45 warmup frames and
240 measured frames per case, actual Core 60 Hz + uGUI + URP into a private texture
with synchronous one-pixel readback. **{len(perf['results'])} cases / {frames} measured
frames; {over} frames exceeded 16.67 ms.** Frame times include GPU readback and are
not a measurement of desktop/player presentation. Allocation measurement remains
unavailable. UI counters still read authoritative values each rendered frame.

{chr(10).join(table)}

Research now shows more rows, states and actions. At the same late-game fixture,
UI CPU mean changed from {next(r['uiMeanMs'] for r in previous['results'] if r['page']=='Research'):.3f}
to {next(r['uiMeanMs'] for r in perf['results'] if r['page']=='Research'):.3f} ms. This is a
measured cost of the additional visible information, within the sampled budget;
it is not described as a speedup. Short isolated samples cannot certify sustained
60 FPS. Earlier releases also recorded occasional readback spikes.
'''
report=root/'Docs/UI-Minimalist-Futuristic/READABILITY-RELEASE.md'
s=report.read_text(encoding='utf-8');a=s.index('## Evidence');b=s.index('## Boundaries',a)
report.write_text(s[:a]+'## Evidence\n\n'+evidence_text+'\n'+s[b:],encoding='utf-8')
state=root/'PROJECT_STATE.md';s=state.read_text(encoding='utf-8');a=s.index('## Current 2.4.0');b=s.index('## Previous 2.3.0',a)
section=f'''## Current 2.4.0 Observatory readability — 2026-10-11

IMPLEMENTED: six category accents/symbols, selected filters, compact expandable
research rows, exact requirements/ETA, real-engine purchase comparison, adaptive
navigation/filters, search/favorites/shortcuts, persisted preferences, pinned goal
notifications, per-economy research plans and opt-in gated queue execution.
Resets pause the queue and restore lost prerequisites. Online/offline decisions
use the same timing. English and the original custom icon remain in the build.

COMPILED: Core {core['pass']}/{core['pass']}, Unity Core {unity['pass']}/{unity['pass']},
UI {ui['pass']}/{ui['pass']}; actual EXE {len(player['checks'])} null-graphics checks passed.
BUILT: Windows Release 2.4.0, zero errors; ZIP has {artifact['verifiedEntries']} verified
files, {artifact['bytes']:,} bytes, SHA256
{artifact['sha256']}.

OBSERVED: {integrity['count']} private Unity URP renders; {len(perf['results'])} late-game
performance cases, {over}/{frames} samples above 16.67 ms. This is offscreen evidence;
physical input, OS DPI and sustained visible-player FPS remain UNVERIFIED.
No desktop control, screen capture or visible game/editor windows.
All {len(saves)} protected user files stayed identical. Progression constants unchanged;
the 400-hour campaign has not been remeasured with the new optional queue.

Report: Docs/UI-Minimalist-Futuristic/READABILITY-RELEASE.md.
Renders: Docs/UI-Minimalist-Futuristic/VISUAL-GALLERY.html.
Evidence: Logs/ObservatoryReadability, Logs/Observatory, Logs/ObservatoryVisual,
Logs/ObservatoryPerformance. Prior release/source backup:
Backups/ObservatoryReadability-20261011-115344.

'''
state.write_text(s[:a]+section+s[b:],encoding='utf-8')
print(json.dumps(evidence))
