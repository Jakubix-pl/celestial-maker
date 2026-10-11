# Release validation — 2.8.1

Evidence collected 2026-10-11. Historical 2.8.0 results remain in Git and [its report](../PrestigeAutomation/REPORT.md).

**IMPLEMENTED:** adjusted Prestige recovery and campaign pacing, cached Observatory work and optional five-step autobuyer lesson. English-only game and installer. [Implementation, migration, measurements and limits](../Observatory281/REPORT.md).

**COMPILED / TESTED:**

| Check | Passed | Scope |
| --- | --- | --- |
| Shared Core | 297/297 | Also from clean published-source copy |
| Extended systems | 63/63 | Also from clean source |
| Unity Core | 20/20 | Shared economy integration |
| Desktop UI | 1484/1484 | Early/late, Night/Dawn, HD–ultrawide, 100–200% text, actual callbacks |
| Phone geometry | 432/432 | Logical layout only; no APK or device |
| Packaged / installed EXE | 108/108 each | Actual Windows binary, isolated saves, null graphics |
| Installer lifecycle | 32/32 | Install, 193 payload hashes, upgrade, installed EXE, uninstall, preservation |

**BUILT:** Windows x64 Release, Unity 6000.6.3f1, zero errors. Installer/ZIP contain the tested executable. Per-user unsigned installer, optional icon shortcut, no automatic launch, saves preserved. [Sizes and SHA-256 hashes](artifacts.json).

**OBSERVED:** three fresh six-hour offline campaigns replayed all ticks and completed in 417.011 / 407.344 / 395.945 hours. Nine event-stepped online policies also meet 360–440 hours. [Raw pacing](pacing.json). Actual private Unity UI renders, lesson steps and 20 distinct animation phases were inspected. [Manifest](screenshots.json). Late-game UI CPU and whole-workload timing measured before/after; synchronous GPU readback and spikes remain in the data. [Report](../Observatory281/REPORT.md).

**UNVERIFIED:** physical FPS, human usability, Windows DPI, clean-machine install, other hardware, Android runtime/APK and refreshed WebGL. Mono allocation counter unsupported; no zero-allocation claim. Hosted CI remains inactive because the credential lacks workflow permission. No signing/store/service deployment claimed.

No desktop control/capture or visible game/editor. All campaign saves/exports/backups unchanged; 18 of 19 baseline files match. Unity's Editor analytics cache updated during rendering. Installer preservation checks passed before that rendering. Private paths, backups and detailed save evidence remain in ignored local Logs.
