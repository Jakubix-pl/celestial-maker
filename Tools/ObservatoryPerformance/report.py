"""Create a release report from measured evidence, with explicit acceptance gaps."""
import json, hashlib
from pathlib import Path
from datetime import datetime, timezone
root=Path(__file__).resolve().parents[2]
def read(p):return json.loads((root/p).read_text(encoding='utf-8-sig'))
before=read('Logs/ObservatoryPerformance/before.json');after=read('Logs/ObservatoryPerformance/after.json')
core=read('Logs/Observatory/core-tests.json');unity=read('Logs/Observatory/unity-core-tests.json');ui=read('Logs/Observatory/ui-headless-tests.json');build=read('Logs/Observatory/build-release.json');player=read('Logs/Observatory/player-headless.json');artifact=read('Logs/Observatory/artifact.json');renders=read('Logs/ObservatoryVisual/after/render-manifest.json');autosave=read('Logs/ObservatoryPerformance/autosave.json')
assert core['fail']==unity['fail']==ui['fail']==0 and build['errors']==0 and player['pass']
assert after['version']==artifact['version']==renders['version']=='2.3.0'
assert len(after['results'])==13 and len(renders['files'])==255
old={r['page']:r for r in before['results'] if r['mode']=='naive-60hz'}
rows=[]
for r in after['results']:
 prev=old.get(r['page']);rows.append('| '+r['page']+' | '+(f"{prev['uiMeanMs']:.3f}" if prev else '—')+f" | {r['uiMeanMs']:.3f} | {r['uiP95Ms']:.3f} | {r['p95Ms']:.2f} | {r['p99Ms']:.2f} | {r['overBudget']}/{r['frames']} |")
total=sum(r['frames'] for r in after['results']);over=sum(r['overBudget'] for r in after['results']);p4=next(r for r in after['results'] if r['page']=='Production-4K');improvement=100*(1-p4['uiMeanMs']/old['Production-4K']['uiMeanMs'])
report=f'''# Observatory 2.3.0 — performance and release evidence

Generated {datetime.now(timezone.utc).isoformat()} from saved measurements.

## IMPLEMENTED

- Resource totals, visible production stock, analytics, reset currency/rewards,
  chapter progress and megastructure progress update per rendered frame.
  The default desktop cap is 60 FPS, with 120 optional. Desktop background
  execution retains that cap instead of dropping to five frames per second.
- Expensive derived rates, buy counts, affordability, metadata and descriptions
  remain on a separate 250 ms cadence. Invisible production/research bindings
  skip work. Live labels use viewport bounds and refresh when they become visible.
  A modal pauses underlying live page bindings while the resource bar stays live.
- Removed per-refresh ToArray copies; avoid duplicate home rate calculation;
  cache selected economy instead of repeatedly cloning parallel metadata.
- Resource, workspace and inspector canvases batch independently. Tooltips
  attach to the root canvas. Opaque desktop UI suppresses the covered 3D scene
  geometry and its rotation work while preserving audio and the portrait fallback.
- New 96pt/2048 SDF font assets preserve original font assets and metadata.
  Native pixel canvas, responsive layout, 80–200% accessibility. Raster assets
  preserve aspect ratio and only scale down from their real source dimensions.
- Original landscape and portrait Observatory illustrations, stellar C emblem,
  Windows EXE icon, multi-size ICO, workspace shortcut and desktop shortcut.
- 140 ms page/dialog/notification reveals and 100 ms control transitions,
  disabled by Reduced motion. No animation writes to gameplay balances.

Comparative research, source links and design rationale: `FINAL-DIRECTION.md`.
Exact image prompts and built-in generation method: `IMAGE-PROMPTS.json`.

## COMPILED / tested

- Core: {core['pass']}/{core['pass']} tests; Unity Core: {unity['pass']}/{unity['pass']}.
- UI: {ui['pass']}/{ui['pass']} geometry and callback checks across early/late,
  Night/Dawn, 1280×720, 1920×1080, 2560×1440 and 3440×1440, text 100–200%.
- Added checks cover 60 consecutive counter frames, state immutability,
  reset/downward values and 1e300, hidden rows becoming visible, parallel
  economy selection, progress bars, HD font sources, no raster upsampling,
  reduced motion, assigned icons, and isolated late-game save/reload.
- Actual release EXE: {len(player['checks'])} checks passed with null graphics.
  This is runtime behavior evidence, not visual player evidence.
- The first state-immutability test exposed JsonUtility serializing a null
  inline OfflineReport with a fresh generated ID. The deterministic fixture now
  supplies a stable report. Only that serialization-generated ID differed;
  production state was not changed by presentation.

## OBSERVED / measured

Machine: {after['cpu'].strip()}, {after['device']}, pinned Unity 6000.6.3f1,
Desktop URP. Late fixture is the real Core simulator's campaign-end Energy
save at about 382.6 hours: 24 owned process types, home plus two parallel
economies. User saves were never loaded into the test fixture.

Each case uses 45 warmup and 240 measured frames; Core advances 1/60 simulated
second per frame. Before has eight routes/resolution cases at original 4 Hz and
at a deliberately naive full refresh every frame. After has thirteen cases
with per-frame presentation, including selected parallel production and four
forced automation boundaries. The synthetic burst makes all three economies
due once per second to expose decision cost; it does not change campaign pacing.

CPU UI time includes binding execution and Canvas.ForceUpdateCanvases. Complete
frame time also includes Camera.Render to a private GPU RenderTexture and a
synchronous one-pixel readback. It is an offscreen workload measurement,
**not player FPS, display pacing, or proof of sustained 60 FPS**.

| Case | Naive 60 Hz UI mean ms | Final UI mean ms | UI p95 ms | Complete offscreen p95 ms | Offscreen p99 ms | >16.67 ms frames |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
{chr(10).join(rows)}

In 4K production, UI CPU cost changed from {old['Production-4K']['uiMeanMs']:.3f}
to {p4['uiMeanMs']:.3f} ms ({improvement:.1f}% lower) while counters update per
frame. The comparison is against naive 60 Hz, not against the old 4 Hz UI.
The original 4 Hz production 4K UI mean was
{next(r for r in before['results'] if r['mode']=='baseline-4hz' and r['page']=='Production-4K')['uiMeanMs']:.3f} ms.

Across the final offscreen run, {over}/{total} complete frames exceeded 16.67 ms.
Earlier baseline and initial-after runs had spikes around 42 ms, predominantly
in the synchronous render/readback portion. They did not recur above budget in
this final run;
this harness does not isolate the driver's GPU scheduling or native player
presentation. We do not claim to have eliminated every FPS dip. Baseline,
initial-after and final-after traces are retained. Short sampling and warmup
effects mean the total-render figures should not be used as a GPU speedup claim.
Allocation profiling was not reliable in this editor harness (the original
counter returned zero); final traces use -1 for unavailable. No zero-GC claim.

Twenty isolated late-game saves averaged {autosave['meanMs']:.2f} ms, p95
{autosave['p95Ms']:.2f} ms. This is a separate synchronous save measurement,
not included in each synthetic frame, and it varies with storage conditions.

{len(renders['files'])} actual Unity URP PNGs were exported and decoded/hash-checked.
Visual inspection covered all twenty late-game routes, both themes, representative
QHD and 200% states, long dialogs, menu art, control states and reveal phases.
The gallery includes actual frames 1/30/60 of the isolated running counter.
No desktop capture, OS input or visible game/editor window was used.

## BUILT

Windows Release 2.3.0: Succeeded, {build['errors']} errors.
`Builds/CelestialMaker-Observatory-Windows.zip`: {artifact['bytes']:,} bytes;
{artifact['verifiedEntries']} entries verified against their extracted source.

SHA256: `{artifact['sha256']}`.

The ZIP uses portable forward-slash paths, contains the original game ICO and
`Create Desktop Shortcut.ps1`. The desktop `Celestial Maker.lnk` and workspace
shortcut point to `Builds/Windows/Observatory-Release/CelestialMaker.exe` with
`CelestialMaker.ico`. The built EXE icon was independently extracted and viewed
(`Logs/ObservatoryPerformance/exe-icon.png`). No shortcut was launched.

## Integrity and remaining acceptance gaps

- Five protected user files match their before hashes. All 28 Core source/meta
  files match; balance and authoritative production logic were not altered.
- Prior release/source/evidence preserved under
  `Backups/ObservatoryPerformance-20261011-112036`.
- **UNVERIFIED:** physical input, OS DPI behavior, ordinary visible-player
  frame pacing, other GPUs and sustained gameplay performance. Offscreen
  renders and CPU timings do not certify those conditions.
- No public deployment, purchase integration, remote Git operation, or changes
  to other projects. Gameplay remains English.

Evidence: `Logs/ObservatoryPerformance`, `Logs/Observatory`,
`Logs/ObservatoryVisual/after`. Reproduce with `Tools/benchmark-observatory.ps1
-After`, `Tools/build-observatory.ps1`, and `Tools/render-observatory.ps1 -After`;
run Unity sequentially and preserve the existing no-visible-window boundary.
'''
(root/'Docs/UI-Minimalist-Futuristic/PERFORMANCE-RELEASE.md').write_text(report,encoding='utf-8')
state=root/'PROJECT_STATE.md';text=state.read_text(encoding='utf-8-sig');marker='## Current 2.2.1 Observatory visual review 2026-10-11'
if marker not in text:
 start=text.index('## Current 2.3.0 Observatory performance');end=text.index('## Previous 2.2.1 Observatory visual review')
 text=text[:start]+text[end:].replace('## Previous 2.2.1 Observatory visual review','## Current 2.2.1 Observatory visual review',1)
assert marker in text
summary=f'''## Current 2.3.0 Observatory performance and final visuals — 2026-10-11

IMPLEMENTED: per-frame resource/progress presentation (60 FPS desktop default,
120 optional), visible-row updates, separate 250 ms derived bindings, cached
economy, independent canvases, hidden 3D scene suppression, HD SDF font sources,
downsample-only artwork, reduced-motion-aware transitions, original art and icon.
Desktop and workspace shortcuts use the custom ICO; EXE embeds the same emblem.

COMPILED: Core {core['pass']}/{core['pass']}, Unity Core {unity['pass']}/{unity['pass']}, UI {ui['pass']}/{ui['pass']}; actual
Windows EXE passed {len(player['checks'])} null-graphics checks. No user save was used by fixtures.

BUILT: Windows Release 2.3.0, zero errors. Latest ZIP has
{artifact['verifiedEntries']} verified files, {artifact['bytes']:,} bytes, SHA256
{artifact['sha256']}.

OBSERVED: {len(renders['files'])} offscreen URP images; thirteen measured late-game cases.
4K UI CPU mean {old['Production-4K']['uiMeanMs']:.3f} → {p4['uiMeanMs']:.3f} ms versus naive 60 Hz.
Final offscreen run: {over}/{total} samples >16.67 ms; earlier runs had render/readback
spikes around 42 ms. This does not certify ordinary-player 60 FPS. Physical input, OS DPI and
visible-player frame pacing remain UNVERIFIED. No desktop control or visible window.

Five user files and all 28 Core source/meta files hash-unchanged. Prior 2.2.1
release backed up in Backups/ObservatoryPerformance-20261011-112036.
Report: Docs/UI-Minimalist-Futuristic/PERFORMANCE-RELEASE.md.
Design/research: Docs/UI-Minimalist-Futuristic/FINAL-DIRECTION.md.
Renders: Docs/UI-Minimalist-Futuristic/VISUAL-GALLERY.html.
Evidence: Logs/ObservatoryPerformance and Logs/Observatory.

## Previous 2.2.1 Observatory visual review 2026-10-11'''
state.write_text(text.replace(marker,summary,1),encoding='utf-8')
print(json.dumps({'report':'PERFORMANCE-RELEASE.md','uiPass':ui['pass'],'images':len(renders['files']),'samples':total,'overBudget':over,'ui4kImprovementPercent':improvement}))
