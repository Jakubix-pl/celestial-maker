# Celestial Maker 2.8.0 — Prestige Automation

Unlock automation with earned Prestige Points and follow a clearer, animated Observatory. Windows x64, English only.

## Changes

- Six chapter autobuyers cost **1 Prestige Point each**, covering four processes per chapter in every universe. They survive Prestige; Ascension and Genesis clear them. The optional Development Plan earns and spends points to restore them.
- Production runs continuously. Stop/start controls are removed, and old paused processes resume on load. Missing inputs and protected reserves still limit output.
- New buildings appear when chapter requirements and purchase resources are forecast to be reachable within 30 seconds. Discovery persists after spending. The first reset reveals every building; normal purchase requirements remain.
- Locked categories and views stay hidden until unlocked. Stronger text/panel contrast and category tints make the interface easier to scan.
- Overview has moving orbital bodies and megastructure arcs. Reduced Motion freezes the animation.
- Fixed an offline replay stall caused by floating-point dust in input allocation.

## Existing saves

No new campaign is required. Saves retain their progress and points. Old free autobuyer flags do not grant purchased packages: unlock packages in Automation, or let an enabled Development Plan buy them with your points. Paid packages remain separate from saved automation profiles.

## Downloads

- **CelestialMaker-Setup-2.8.0.exe** — per-user installer with an optional desktop shortcut and game icon. It does not launch the game automatically.
- **CelestialMaker-Observatory-Windows.zip** — extract the complete folder.
- **CelestialMaker-2.8.0-SHA256.txt** — checksums for both downloads.

The installer is unsigned. No account, shop or payment is required. Uninstalling preserves saves.

## Verification and pacing

Core **276/276**, extended system audit **63/63**, desktop UI **1481/1481**, Unity Core **20/20**, phone layout **432/432**. Windows Release built with zero errors. The packaged and installed EXE each passed **99 checks**; installer, upgrade, uninstall and preservation checks passed **32/32**.

Literal offline tick campaigns visiting every six hours completed in **430.878 h (Energy), 450.753 h (Matter), and 438.012 h (Time)**. The roughly 400-hour target is not an exact guarantee; Matter exceeds the earlier 360–440 h acceptance band.

Private UI renders and offscreen workloads were measured. Performance spikes are retained in the report; physical desktop FPS, human playtesting and a clean machine remain unverified. No desktop control/capture or visible game was used. Android APK work is deferred and no APK is included.

See [screenshots](https://github.com/Jakubix-pl/celestial-maker), [validation](https://github.com/Jakubix-pl/celestial-maker/blob/main/Docs/Release/VALIDATION.md) and the [implementation report](https://github.com/Jakubix-pl/celestial-maker/blob/main/Docs/PrestigeAutomation/REPORT.md).
