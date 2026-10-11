# Celestial Maker 2.7.0 — Discovery

Build a connected cosmos at your own pace. This English-language Windows release includes the Observatory interface and the full Discovery expansion.

## Download

- **CelestialMaker-Setup-2.7.0.exe**: recommended per-user installer, with an optional desktop shortcut and game icon.
- **CelestialMaker-Observatory-Windows.zip**: portable game; extract every file together and launch `CelestialMaker.exe`.
- **CelestialMaker-2.7.0-SHA256.txt**: checksums for both downloads.

No account, shop or payment is required. Installation does not launch the game automatically. The installer is unsigned. Supported existing saves are migrated; uninstalling preserves player saves.

## Included

- Prestige, Ascension and Genesis, development trees, three staged megastructures, nine reality laws and parallel economies.
- Engineering module tradeoffs, ordered automation goals and reset holds.
- Six-hour plan comparisons on isolated copies, executed by a background worker.
- Twelve challenges with varied constraints, readiness trials and attempt reviews.
- Permanent expedition route discoveries and cooperating universe specializations.
- Optional safe lessons, a decision chronicle, campaign finale and separate scenario profiles.
- Colored category/resource icons, compact expandable screens, Night/Dawn themes, adjustable text, search, favorites and reduced motion.
- Uncapped, resumable offline replay through every 60 Hz tick, including active automation.

## Fixes and balance

Fixed numerical tick stalls at resource depletion, protected-stock accounting, blocking laboratory comparisons and the Overview's progress layout. Added precise transfer timing and stock-goal explanations.

Three scripted campaigns visiting every six hours completed in 415.625 h (Energy), 371.800 h (Matter) and 407.083 h (Time), measured with the real tick logic and serialized reloads. The target is around 400 hours of elapsed progression, including background/offline time. Individual play varies.

## Verification

Windows x64 Release built with zero errors. The actual release player and installed player each passed 94 checks; silent installer/upgrade/uninstall verification passed 26 checks. The portable Core suite has 250 checks and the extended system audit has 63. Unity UI, Core and framework checks are documented in the linked report.

The offscreen late-game benchmark covered 22 cases and 5,280 measured frames with zero samples above 16.67 ms on the tested RTX 4070 Ti / Ryzen 7 9800X3D. This does not certify physical desktop FPS. Human playtesting, Windows DPI behavior, clean-machine installation and other hardware remain unverified. No desktop was controlled or captured, and no visible game window was used for validation.

See [the game and screenshots](https://github.com/Jakubix-pl/celestial-maker), [validation](https://github.com/Jakubix-pl/celestial-maker/blob/main/Docs/Release/VALIDATION.md) and [building instructions](https://github.com/Jakubix-pl/celestial-maker/blob/main/Docs/BUILDING.md).
