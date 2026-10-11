# Observatory 2.3.0 — performance and release evidence

Generated 2026-10-11T09:46:12.994644+00:00 from saved measurements.

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

- Core: 117/117 tests; Unity Core: 20/20.
- UI: 1368/1368 geometry and callback checks across early/late,
  Night/Dawn, 1280×720, 1920×1080, 2560×1440 and 3440×1440, text 100–200%.
- Added checks cover 60 consecutive counter frames, state immutability,
  reset/downward values and 1e300, hidden rows becoming visible, parallel
  economy selection, progress bars, HD font sources, no raster upsampling,
  reduced motion, assigned icons, and isolated late-game save/reload.
- Actual release EXE: 33 checks passed with null graphics.
  This is runtime behavior evidence, not visual player evidence.
- The first state-immutability test exposed JsonUtility serializing a null
  inline OfflineReport with a fresh generated ID. The deterministic fixture now
  supplies a stable report. Only that serialization-generated ID differed;
  production state was not changed by presentation.

## OBSERVED / measured

Machine: AMD Ryzen 7 9800X3D 8-Core Processor, NVIDIA GeForce RTX 4070 Ti, pinned Unity 6000.6.3f1,
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
| Production | 0.524 | 0.364 | 0.580 | 2.04 | 2.61 | 0/240 |
| Analytics | 0.231 | 0.122 | 0.205 | 1.53 | 2.03 | 0/240 |
| Automation | 0.287 | 0.240 | 0.256 | 1.88 | 6.43 | 0/240 |
| Parallel universes | 0.156 | 0.083 | 0.126 | 1.11 | 1.30 | 0/240 |
| Trees | 0.294 | 0.235 | 0.337 | 2.06 | 2.32 | 0/240 |
| Research | 0.303 | 0.186 | 0.261 | 1.74 | 2.21 | 0/240 |
| Production-QHD | 0.622 | 0.354 | 0.516 | 2.45 | 2.79 | 0/240 |
| Production-4K | 0.630 | 0.371 | 0.644 | 3.01 | 3.44 | 0/240 |
| Overview | — | 0.081 | 0.156 | 1.34 | 1.54 | 0/240 |
| Prestige | — | 0.091 | 0.152 | 1.34 | 1.58 | 0/240 |
| Megastructures | — | 0.094 | 0.167 | 1.40 | 1.60 | 0/240 |
| Satellite | — | 0.346 | 0.540 | 2.31 | 2.98 | 0/240 |
| Automation burst | — | 0.154 | 0.232 | 1.98 | 2.83 | 0/240 |

In 4K production, UI CPU cost changed from 0.630
to 0.371 ms (41.0% lower) while counters update per
frame. The comparison is against naive 60 Hz, not against the old 4 Hz UI.
The original 4 Hz production 4K UI mean was
0.284 ms.

Across the final offscreen run, 0/3120 complete frames exceeded 16.67 ms.
Earlier baseline and initial-after runs had spikes around 42 ms, predominantly
in the synchronous render/readback portion. They did not recur above budget in
this final run;
this harness does not isolate the driver's GPU scheduling or native player
presentation. We do not claim to have eliminated every FPS dip. Baseline,
initial-after and final-after traces are retained. Short sampling and warmup
effects mean the total-render figures should not be used as a GPU speedup claim.
Allocation profiling was not reliable in this editor harness (the original
counter returned zero); final traces use -1 for unavailable. No zero-GC claim.

Twenty isolated late-game saves averaged 6.11 ms, p95
14.37 ms. This is a separate synchronous save measurement,
not included in each synthetic frame, and it varies with storage conditions.

255 actual Unity URP PNGs were exported and decoded/hash-checked.
Visual inspection covered all twenty late-game routes, both themes, representative
QHD and 200% states, long dialogs, menu art, control states and reveal phases.
The gallery includes actual frames 1/30/60 of the isolated running counter.
No desktop capture, OS input or visible game/editor window was used.

## BUILT

Windows Release 2.3.0: Succeeded, 0 errors.
`Builds/CelestialMaker-Observatory-Windows.zip`: 49,638,729 bytes;
193 entries verified against their extracted source.

SHA256: `883C0432B7D28AAD7E8E05905B9932C51916FB67171D87856FD667734F0F3078`.

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
