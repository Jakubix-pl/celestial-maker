# Release validation — 2.8.2

Evidence collected 2026-10-11. [Implementation and design references](CASUAL-2.8.2.md). Earlier evidence remains in Git and versioned Android reports.

**IMPLEMENTED:** revised chapter costs and upgrade targets, thin purchase funding feedback, research suggestion, strategy introductions and modal binding cleanup. Save IDs and owned progress retained. English only.

**COMPILED / TESTED:**

| Check | Passed | Scope |
| --- | --- | --- |
| Shared Core | 318/318 | Actual economy, also from clean source |
| Extended systems | 63/63 | Challenges/scenarios, also from clean source |
| Unity Core | 20/20 | Economy integration and serialization |
| Desktop UI | 1488/1488 | Early/late, Night/Dawn, resolutions, text sizes, callbacks and bars |
| Phone UI | 452/452 | Logical portrait/landscape layouts and purchase bars |
| Packaged EXE | 107/107 | Hidden null-graphics runtime |
| Installed EXE | 107/107 | Isolated installed payload |
| Installer | 32/32 | Install/upgrade/uninstall and file hashes |
| APK inspection | 34/34 | Signature, version, ARM64, manifest, icons, CRC/alignment |

The player's optional `Continue calculation` action is timing-dependent: the shortened replay completed before that action was sampled here, explaining one fewer runtime action than the older release. Literal offline campaigns independently replay every tick and save/reload at each visit.

**BUILT:** Windows Release and ARM64 IL2CPP APK, zero errors; installer and ZIP verified. Android target API 37 / minimum 26, version/code 2.8.2/28201, development signing, non-debuggable package. No billing/shop integration.

**OBSERVED:** actual uGUI/TMP private URP renders inspected for desktop and phone purchase examples. Twenty distinct Overview animation phases and 24 phone renders retained. UI callbacks and authoritative funding transitions pass, including prerequisites, reserves, catch-up, acquired items and modal cleanup. This does not establish physical touch behavior.

Literal offline campaign completion:

| Strategy | Completion |
| --- | --- |
| Energy | 416.936 h |
| Matter | 407.152 h |
| Time | 395.249 h |

Nine event-stepped policies complete in 370.554–416.936 hours. Active policies first Prestige at 1.25 h. Six-hour-only policies first Prestige at 12 h because purchases/first confirmations wait for visits. All scripted policies select opt-in plans/modules/roles; actual choices can change these times.

Performance: 22 late-game cases / 5,280 frames after warm-up, actual Core, UI and private GPU renders with synchronous readback. Worst workload p95 4.028 ms; worst UI p95 0.750 ms; 0 samples above 16.67 ms. Allocation counters are unsupported where reported as -1. This is a scoped host benchmark, not phone or physical-display FPS. [Raw results](performance.json).

All gameplay saves, exports and backups remain byte-identical. 18/19 protected files match; 1 non-save Editor Analytics cache file(s) changed during rendering. Clean-source publication audit excludes local saves, credentials, SDK/JDK, Library and release binaries.

**UNVERIFIED:** installation and interaction on the S26 Ultra, Android lifecycle on that device, physical Windows/phone FPS, human usability and a clean-machine installer test. No OS input, desktop capture, visible editor or visible player was used.

**PUBLISHED / DELIVERED:** [Public 2.8.2 release](https://github.com/Jakubix-pl/celestial-maker/releases/tag/v2.8.2), five assets anonymously downloaded and SHA-256 matched, README and four purchase images verified. Installer/ZIP/APK and checksums copied to Desktop / Calestial Maker with hash verification; previous versions retained. [Publication record](publication.json).
