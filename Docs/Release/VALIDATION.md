# Release validation — 2.8.0

Evidence collected on 2026-10-11. Earlier 2.7.0 records remain in Git history and their original release.

## IMPLEMENTED

Prestige-funded chapter autobuyers, continuous production, persistent 30-second building discovery, first-reset reveal, unlocked navigation, brighter colors and animated Overview. See [the implementation and migration rules](../PrestigeAutomation/REPORT.md). English-only interface and installer. Android work is deferred.

## COMPILED / TESTED

| Check | Result | Scope |
| --- | --- | --- |
| Shared Core | 276/276 | Also passed from a clean source copy without personal backups, Library or binaries. |
| Extended systems | 63/63 | Also passed from clean source: modules, goals, all 12 challenges and nine scenario/target combinations. |
| Unity Core | 20/20 | Current shared economy integration. |
| Desktop UI | 1481/1481 | Early/late, Night/Dawn, HD–ultrawide, 100–200% text, discovery/navigation/purchase callbacks. |
| Phone geometry | 432/432 | Logical portrait/landscape fixtures only; no APK or physical touch test. |
| Windows EXE | 99/99 | Actual final binary, null graphics, isolated saves; animated Update and reduced motion included. |
| Installed EXE | 99/99 | Same final binary launched from the installed directory. |
| Installer | 32/32 | Install, 193 payload hashes, upgrade, uninstall and protected-file checks. |

Core tests include paid/unpaid buyer boundaries, duplicate spending, reset retention, legacy migration, parallel ownership, exact 30-second reveal, sticky discovery, explicit goal holds and offline ledger agreement. The regression fixture for floating-point dust is a synthetic campaign state.

The runtime test was corrected to observe a new graphic after navigation rebuilds and to address the actual search modal input. Earlier failed attempts are retained in local logs. Unity framework and optional web/backend results from 2.7.0 were not rerun and are not counted as current passes.

## BUILT

Windows x64 Release, Unity 6000.6.3f1, zero build errors. The installer and ZIP contain the tested executable. All 193 ZIP entries match the packaged files. [Artifact sizes and hashes](artifacts.json). Per-user unsigned installer; optional desktop shortcut; no automatic launch; saves retained on removal.

## OBSERVED

Three fresh scripted campaigns replayed every offline tick at 60 Hz, with visits every six hours and serialized reloads. Earned points fund buyers; the Development Plan and module/role choices are enabled; first reset decisions are manual.

| Strategy | Campaign completion |
| --- | --- |
| Energy | 430.877531 h |
| Matter | 450.752531 h |
| Time | 438.011891 h |

The roughly 400-hour goal remains approximate. Matter exceeds the previous 360–440 h band, and no all-strategy balance pass is claimed. These are scripted policies, not human play. [Raw pacing measurements](pacing.json).

Actual uGUI/TMP/URP screenshots show fresh/first-reset states and a historical late-game fixture in the current UI. The historical fixture's displayed completion time is not the new pacing result. Twenty rendered animation phases have distinct hashes. Representative fresh production, buyer purchase and Overview images were inspected. [Manifest](screenshots.json).

The offscreen benchmark uses the actual 60 Hz Core, purchased buyers, two satellite economies, animated Overview and private RenderTextures with synchronous GPU readback. Hardware: RTX 4070 Ti / Ryzen 7 9800X3D. Each run contains 22 cases, 45 warm-up frames per case and 5,280 measured frames.

| Run | Samples >16.67 ms | Worst case p95 | Maximum sample |
| --- | --- | --- | --- |
| First | 198 | 20.0517 ms | 41.7630 ms |
| Repeat after other test jobs finished | 39 | 38.2127 ms | 43.7146 ms |

The repeated run's worst UI p95 is 1.4285 ms; worst Core p95 is 2.0072 ms. GPU readback overhead and host activity are included. Both runs are retained, rather than treating a short workload as a stable desktop FPS measurement. [First run](performance-first.json), [repeat](performance.json).

## UNVERIFIED / LIMITS

- Physical display FPS, Windows DPI, human usability, clean-machine installation and other hardware remain unverified.
- No Android APK, device runtime or refreshed WebGL artifact is covered.
- No code signing, store review or external service deployment is claimed.
- Hosted CI is inactive because the publication credential cannot write workflow files; the portable suites passed locally from clean source.

No desktop control/capture or visible game/editor was used. Packaging verified all 19 baseline files unchanged; installer preservation checks cover 18, excluding framework test XML. Private paths, backups and detailed personal-file logs remain outside Git.
