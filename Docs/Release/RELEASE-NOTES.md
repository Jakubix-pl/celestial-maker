# Celestial Maker 2.8.1 — Observatory Refinement

A smoother Observatory, an optional autobuyer lesson and measured pacing closer to 400 hours. Windows x64, English only.

## Changes

- The first Prestige, and the first recovery Prestige after Ascension/Genesis, earns one point at 2.52e5 lifetime Energy (or 7.18e5 Particles). Later runs keep the normal threshold. Buyers still cost earned Prestige Points; there are no free buyer unlocks.
- Stronger reset starting production, shorter first megastructure stages, adjusted later construction duration and improved Matter preparation remove automation recovery stalls. Conserved matter no longer penalizes primary production.
- Five optional practice steps explain spending a point, automatic purchases, Prestige retention and Ascension clearing. The practice economy is isolated; the campaign is unchanged. Reopen it from Automation or Practice mechanics.
- Cached static orbital scenery and visibility checks reduce repeated UI work. Snapshot refresh no longer presents twice. Pinned goals keep a stable status area. Live counters and animated Overview remain refreshed every frame; Reduced Motion remains available.

## Measured campaign pace

Literal offline tick replay, checking in every six hours:

| Strategy | Completion |
| --- | --- |
| Energy | 417.011 h |
| Matter | 407.344 h |
| Time | 395.945 h |

Nine event-stepped active/background/check-in policies completed in 376.5–417.0 hours. All tested policies are within 360–440 hours. These are scripted policies using the shared Core, with the optional Development Plan and explicit choices, not a promise for every player. The first reset of each type still requires confirmation.

## Downloads and saves

- **CelestialMaker-Setup-2.8.1.exe** — per-user English installer, optional icon shortcut, no automatic game launch.
- **CelestialMaker-Observatory-Windows.zip** — extract the entire folder.
- **CelestialMaker-2.8.1-SHA256.txt** — hashes for both downloads.

Existing supported saves retain progress. The new first-stage duration applies to current construction work; a project may complete sooner on its next simulation tick. Reset starting production changes apply on future resets. No new campaign is required. The installer is unsigned and uninstalling preserves saves.

## Verification

Core 297/297, systems 63/63, Unity Core 20/20, desktop UI 1484/1484, phone geometry 432/432. Windows Release built with zero errors. Packaged and installed EXE each passed 108 checks; installer/upgrade/uninstall checks passed 32/32. Protected saves remained unchanged.

Private UI renders and a late-game offscreen benchmark were inspected. Physical display FPS, human playtesting and a clean machine remain unverified. No desktop control/capture or visible game/editor was used. No APK is included.

See [validation](https://github.com/Jakubix-pl/celestial-maker/blob/main/Docs/Release/VALIDATION.md) and [screenshots](https://github.com/Jakubix-pl/celestial-maker).
